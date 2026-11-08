$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localDir = Join-Path $projectRoot '.local'
$runtimeDir = Join-Path $localDir 'runtime'
$cacheDir = Join-Path $localDir 'cache'
$dataDir = Join-Path $localDir 'postgres-data'
$apiEnvFile = Join-Path $projectRoot 'Api\.env'
$webEnvFile = Join-Path $projectRoot 'WebApp\.env'
$utf8 = New-Object System.Text.UTF8Encoding($false)
New-Item -ItemType Directory -Path $runtimeDir, $cacheDir -Force | Out-Null
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

function Get-LocalEnvValue($key) {
    $line = Get-Content -LiteralPath $apiEnvFile | Where-Object { $_ -match "^$key=" } | Select-Object -First 1
    if (-not $line) { throw "Variavel $key ausente em Api/.env." }
    return ($line.Substring($key.Length + 1)).Trim().Trim('"').Trim("'")
}

function Get-Download($url, $destination) {
    $previousProgress = $ProgressPreference
    try {
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $url -OutFile "$destination.part" -UseBasicParsing
        Move-Item -LiteralPath "$destination.part" -Destination $destination -Force
    } finally { $ProgressPreference = $previousProgress }
}

function Expand-PortableZip($archivePath, $destination, $prefixes, $stripPrefix = '') {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $destinationRoot = [IO.Path]::GetFullPath($destination).TrimEnd('\') + '\'
    $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        foreach ($entry in $archive.Entries) {
            $include = $false
            foreach ($prefix in $prefixes) {
                if ($entry.FullName.StartsWith($prefix, [StringComparison]::Ordinal)) { $include = $true; break }
            }
            if (-not $include -or -not $entry.Name) { continue }
            $relativePath = $entry.FullName.Substring($stripPrefix.Length)
            $target = [IO.Path]::GetFullPath((Join-Path $destination $relativePath))
            if (-not $target.StartsWith($destinationRoot, [StringComparison]::OrdinalIgnoreCase)) {
                throw 'Caminho invalido no arquivo ZIP.'
            }
            [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
        }
    } finally { $archive.Dispose() }
}

function New-LocalSecret($length) {
    $bytes = New-Object byte[] $length
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [BitConverter]::ToString($bytes).Replace('-', '')
}

if ((Test-Path (Join-Path $dataDir 'PG_VERSION')) -and (Get-Content (Join-Path $dataDir 'PG_VERSION') -Raw).Trim() -ne '17') {
    throw 'O banco existente nao e PostgreSQL 17. Nenhum dado foi alterado.'
}

$dotnet = Join-Path $runtimeDir 'dotnet\dotnet.exe'
$hasSdk = $false
if (Test-Path $dotnet) { $hasSdk = [bool](@(& $dotnet --list-sdks | Where-Object { $_ -match '^8\.' }).Count) }
if (-not $hasSdk) {
    Write-Host 'Preparando SDK .NET 8 no projeto (download da Microsoft)...'
    $installer = Join-Path $cacheDir 'dotnet-install.ps1'
    Get-Download 'https://dot.net/v1/dotnet-install.ps1' $installer
    & $installer -Channel 8.0 -Quality GA -InstallDir (Join-Path $runtimeDir 'dotnet') -NoPath
    if (-not (Test-Path $dotnet)) { throw 'Falha ao preparar o SDK .NET 8.' }
    if (-not (@(& $dotnet --list-sdks | Where-Object { $_ -match '^8\.' }).Count)) { throw 'SDK .NET 8 indisponivel apos instalacao.' }
}

$postgresDir = Join-Path $runtimeDir 'postgresql'
$requiredPgFiles = 'pgsql\bin\pg_ctl.exe', 'pgsql\bin\initdb.exe', 'pgsql\bin\postgres.exe', 'pgsql\bin\psql.exe', 'pgsql\share\postgres.bki', 'pgsql\share\postgresql.conf.sample'
$missingPgFiles = @($requiredPgFiles | Where-Object { -not (Test-Path (Join-Path $postgresDir $_)) })
if ($missingPgFiles.Count -gt 0) {
    Write-Host 'Preparando PostgreSQL 17 no projeto...'
    $pgArchive = Join-Path $cacheDir 'postgresql-17.11-5-windows-x64-binaries.zip'
    $expectedHash = '80379B2C04D51C30225532E0AE04509899141E9957ED096FE749D7FD9DF8F82F'
    if (-not (Test-Path $pgArchive) -or (Get-FileHash -LiteralPath $pgArchive -Algorithm SHA256).Hash -ne $expectedHash) {
        Write-Host 'Baixando PostgreSQL da EnterpriseDB...'
        Get-Download 'https://get.enterprisedb.com/postgresql/postgresql-17.11-5-windows-x64-binaries.zip' $pgArchive
    }
    if ((Get-FileHash -LiteralPath $pgArchive -Algorithm SHA256).Hash -ne $expectedHash) { throw 'Download do PostgreSQL com checksum invalido.' }
    Expand-PortableZip $pgArchive $postgresDir @('pgsql/bin/', 'pgsql/lib/', 'pgsql/share/')
}

$node = Join-Path $runtimeDir 'node\node.exe'
if (-not (Test-Path $node)) {
    $systemNode = Get-Command node.exe -ErrorAction SilentlyContinue
    if ($systemNode) { $node = $systemNode.Source }
}
$nodeVersion = if (Test-Path $node) { [version]((& $node --version).Trim().TrimStart('v')) } else { [version]'0.0' }
$supportedNode = $nodeVersion -ge [version]'22.12' -or ($nodeVersion.Major -eq 20 -and $nodeVersion -ge [version]'20.19')
if (-not $supportedNode) {
    Write-Host 'Preparando Node.js 22 LTS no projeto (download de nodejs.org)...'
    $releases = Invoke-RestMethod -Uri 'https://nodejs.org/dist/index.json'
    $release = $releases | Where-Object { $_.version -like 'v22.*' -and $_.lts } | Select-Object -First 1
    if (-not $release) { throw 'Nao foi possivel localizar Node.js 22 LTS.' }
    $nodePackage = "node-$($release.version)-win-x64"
    $nodeArchive = Join-Path $cacheDir "$nodePackage.zip"
    $checksums = (Invoke-WebRequest -Uri "https://nodejs.org/dist/$($release.version)/SHASUMS256.txt" -UseBasicParsing).Content
    $checksumLine = $checksums -split '\r?\n' | Where-Object { $_ -match "\s+$([regex]::Escape($nodePackage)).zip$" } | Select-Object -First 1
    if (-not $checksumLine) { throw 'Checksum oficial do Node.js indisponivel.' }
    $nodeHash = ($checksumLine -split '\s+')[0]
    if (-not (Test-Path $nodeArchive) -or (Get-FileHash -LiteralPath $nodeArchive -Algorithm SHA256).Hash -ne $nodeHash) {
        Get-Download "https://nodejs.org/dist/$($release.version)/$nodePackage.zip" $nodeArchive
    }
    if ((Get-FileHash -LiteralPath $nodeArchive -Algorithm SHA256).Hash -ne $nodeHash) { throw 'Download do Node.js com checksum invalido.' }
    Expand-PortableZip $nodeArchive (Join-Path $runtimeDir 'node') @("$nodePackage/") "$nodePackage/"
    $node = Join-Path $runtimeDir 'node\node.exe'
}

if (-not (Test-Path $apiEnvFile)) {
    if (Test-Path (Join-Path $dataDir 'PG_VERSION')) {
        throw 'Api/.env ausente com banco existente. Restaure esse arquivo para preservar a senha do banco.'
    }
    Write-Host 'Criando configuracao local com credenciais aleatorias...'
    $content = [IO.File]::ReadAllText((Join-Path $projectRoot 'Api\.env.example'))
    $dbPassword = New-LocalSecret 24
    $settings = @{ DB_HOST = '127.0.0.1'; DB_PORT = '55432'; DB_NAME = 'central_emergencias_dev'; DB_PASSWORD = $dbPassword; POSTGRES_DB = 'central_emergencias_dev'; POSTGRES_PASSWORD = $dbPassword; JWT_SECRET_KEY = (New-LocalSecret 64) }
    foreach ($key in $settings.Keys) { $content = $content -replace "(?m)^$key=[^\r\n]*", "$key=$($settings[$key])" }
    [IO.File]::WriteAllText($apiEnvFile, $content, $utf8)
}
if (-not (Test-Path $webEnvFile)) {
    $content = [IO.File]::ReadAllText((Join-Path $projectRoot 'WebApp\.env.example')) -replace '(?m)^VITE_API_BASE_URL=[^\r\n]*', 'VITE_API_BASE_URL=http://localhost:5210/api'
    [IO.File]::WriteAllText($webEnvFile, $content, $utf8)
}
if ((Get-LocalEnvValue 'DB_HOST') -notin @('localhost', '127.0.0.1')) {
    throw 'Para o banco portatil, configure DB_HOST=127.0.0.1 em Api/.env.'
}

if (-not (Test-Path (Join-Path $dataDir 'PG_VERSION'))) {
    Write-Host 'Inicializando banco local...'
    $passwordFile = Join-Path $localDir 'initdb-password.txt'
    try {
        [IO.File]::WriteAllText($passwordFile, (Get-LocalEnvValue 'DB_PASSWORD'), $utf8)
        & (Join-Path $postgresDir 'pgsql\bin\initdb.exe') -D $dataDir -U (Get-LocalEnvValue 'DB_USER') --auth=scram-sha-256 --encoding=UTF8 --locale=C "--pwfile=$passwordFile"
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao inicializar banco local. Nenhum banco existente foi removido.' }
    } finally {
        if (Test-Path $passwordFile) { Remove-Item -LiteralPath $passwordFile }
    }
}

if (-not (Test-Path (Join-Path $projectRoot 'WebApp\node_modules\vite\bin\vite.js'))) {
    Write-Host 'Instalando dependencias do frontend...'
    $previousPath = $env:PATH
    Push-Location (Join-Path $projectRoot 'WebApp')
    try {
        $env:PATH = (Split-Path $node -Parent) + ';' + $env:PATH
        & (Join-Path (Split-Path $node -Parent) 'npm.cmd') ci --no-fund --no-audit
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao instalar dependencias do frontend.' }
    } finally {
        $env:PATH = $previousPath
        Pop-Location
    }
}
