param([switch]$Stop)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$localDir = Join-Path $projectRoot '.local'
$logsDir = Join-Path $localDir 'logs'
$stateFile = Join-Path $localDir 'processes.json'
$dataDir = Join-Path $localDir 'postgres-data'
$runtimeDir = Join-Path $localDir 'runtime'
$pgCtl = Join-Path $runtimeDir 'postgresql\pgsql\bin\pg_ctl.exe'
$dotnet = Join-Path $runtimeDir 'dotnet\dotnet.exe'

function Get-TrackedProcess($entry) {
    if (-not $entry) { return $null }
    $process = Get-Process -Id $entry.Id -ErrorAction SilentlyContinue
    if ($process -and $process.StartTime.ToUniversalTime().Ticks.ToString() -eq $entry.StartTicks) {
        return $process
    }
    return $null
}

$state = @{}
if (Test-Path $stateFile) {
    $saved = Get-Content $stateFile -Raw | ConvertFrom-Json
    foreach ($name in 'Api', 'Web') { $state[$name] = $saved.$name }
}

if ($Stop) {
    foreach ($name in 'Web', 'Api') {
        $process = Get-TrackedProcess $state[$name]
        if ($process) { Stop-Process -Id $process.Id }
    }
    if (Test-Path (Join-Path $dataDir 'postmaster.pid')) {
        if (-not (Test-Path $pgCtl)) { throw 'pg_ctl.exe ausente. Execute .\scripts\dev.ps1 para recuperar as ferramentas antes de parar o banco.' }
        & $pgCtl -D $dataDir -m fast -w stop
        if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel parar o PostgreSQL. Consulte .local/logs.' }
    }
    if (Test-Path $stateFile) { Remove-Item -LiteralPath $stateFile }
    Write-Host 'Servicos locais encerrados. Os dados do banco foram preservados.'
    return
}

& (Join-Path $PSScriptRoot 'setup-local.ps1')
New-Item -ItemType Directory -Path $logsDir -Force | Out-Null

function Save-State {
    $state | ConvertTo-Json | Set-Content -LiteralPath $stateFile -Encoding utf8
}

function Start-TrackedProcess($name, $executable, $arguments, $directory) {
    if (Get-TrackedProcess $state[$name]) {
        Write-Host "$name ja esta em execucao."
        return
    }
    $process = Start-Process -FilePath $executable -ArgumentList $arguments -WorkingDirectory $directory -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logsDir "$name.out.log") -RedirectStandardError (Join-Path $logsDir "$name.err.log") -PassThru
    $state[$name] = @{ Id = $process.Id; StartTicks = $process.StartTime.ToUniversalTime().Ticks.ToString() }
    Save-State
}

function Wait-Http($url, $name) {
    $deadline = (Get-Date).AddSeconds(60)
    do {
        if (-not (Get-TrackedProcess $state[$name])) { throw "$name encerrou durante a inicializacao. Consulte $logsDir." }
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) { return }
        } catch { }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    throw "Tempo esgotado aguardando $url. Consulte $logsDir."
}

$dbPortLine = Get-Content (Join-Path $projectRoot 'Api\.env') | Where-Object { $_ -match '^DB_PORT=' } | Select-Object -First 1
if (-not $dbPortLine) { throw 'DB_PORT ausente em Api/.env.' }
$dbPort = [int]($dbPortLine -replace '^DB_PORT=', '')
& $pgCtl -D $dataDir status *> $null
if ($LASTEXITCODE -ne 0) {
    & $pgCtl -D $dataDir -l (Join-Path $logsDir 'postgres.log') -o "-h 127.0.0.1 -p $dbPort" -w start
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar PostgreSQL.' }
}

$previousDotnetRoot = $env:DOTNET_ROOT
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:DOTNET_ROOT = Join-Path $runtimeDir 'dotnet'
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    if (-not (Get-TrackedProcess $state.Api)) {
        & $dotnet build (Join-Path $projectRoot 'Api\Api.csproj') --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Falha na compilacao da API.' }
        Start-TrackedProcess 'Api' $dotnet 'bin/Debug/net8.0/Api.dll' (Join-Path $projectRoot 'Api')
    }
    Wait-Http 'http://localhost:5210/swagger/v1/swagger.json' 'Api'

    $node = Join-Path $runtimeDir 'node\node.exe'
    if (-not (Test-Path $node)) { $node = (Get-Command node.exe -ErrorAction Stop).Source }
    Start-TrackedProcess 'Web' $node 'node_modules/vite/bin/vite.js --host 127.0.0.1 --port 5173 --strictPort' (Join-Path $projectRoot 'WebApp')
    Wait-Http 'http://localhost:5173' 'Web'
} finally {
    $env:DOTNET_ROOT = $previousDotnetRoot
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}

Write-Host 'Aplicacao: http://localhost:5173'
Write-Host 'Swagger:   http://localhost:5210/swagger'
Write-Host 'Login: root / root1234'
Write-Host 'Para parar: .\scripts\dev.ps1 -Stop'
