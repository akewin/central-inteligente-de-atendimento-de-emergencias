# Central Inteligente de Atendimento de Emergências

Plataforma full-stack para registrar, analisar e coordenar ocorrências de emergência. O sistema conecta a central de atendimento, equipes operacionais e recepções hospitalares em um fluxo único, com apoio de IA, recomendação geográfica de recursos, atualizações em tempo real, controle de acesso e auditoria.

> Projeto acadêmico/protótipo. As integrações públicas de mapas e os dados semeados não devem ser tratados como infraestrutura de missão crítica.

## Funcionalidades implementadas

### Central de atendimento

- Cadastro e consulta paginada de ocorrências, com descrição e endereço estruturado.
- Busca de endereço por CEP via ViaCEP e geocodificação pelo Nominatim.
- Classificação assistida por IA via Groq, com tipo, prioridade, serviços necessários, justificativa e confiança.
- Confirmação humana da classificação e dos serviços indicados.
- Máquina de estados: `ABERTA`, `EM_ANALISE`, `AGUARDANDO_CONFIRMACAO`, `DESPACHADA`, `EM_ATENDIMENTO`, `FINALIZADA` e `CANCELADA`.
- Recomendação de equipes disponíveis por serviço, compatibilidade e distância.
- Despacho de Polícia Militar (190), SAMU (192) e Corpo de Bombeiros (193).
- Linha do tempo com mudanças de estado e histórico operacional.

### Operação das equipes

- Cadastro e atualização de equipes, endereço, coordenadas e conta operacional vinculada.
- Estados `DISPONIVEL`, `RESERVADA`, `DESLOCAMENTO`, `EM_ATENDIMENTO` e `INDISPONIVEL`.
- Área “Meus chamados” restrita à equipe do usuário autenticado.
- Ciclo de despacho: atribuição, aceite, chegada, atendimento, conclusão ou cancelamento.
- Consulta de rota e estimativa de deslocamento.
- Solicitação de transporte, definição de hospital, início do transporte e conclusão no local.
- Atualizações operacionais em tempo real via SignalR.

### Hospitais e transporte

- Cadastro de hospitais e alas, capacidade total, ocupação e disponibilidade.
- Mapa da ocorrência com equipes, hospitais próximos e rotas via OSRM.
- Descoberta de hospitais com OpenStreetMap/Overpass e ranqueamento para atendimento.
- Seleção de destino e encaminhamento por equipe SAMU.
- Painel de recepção hospitalar com fila ativa e histórico.
- Confirmação de ciência, recebimento ou cancelamento pela recepção.
- Ciclo `AGUARDANDO_CENTRAL`, `AGUARDANDO_DESTINO`, `HOSPITAL_AVISADO`, `EM_TRANSPORTE`, `RECEBIDO` e `CANCELADO`.

### Administração e segurança

- Autenticação JWT, refresh token, logout e login externo suportado pela API.
- Recuperação de senha por e-mail com Resend.
- Gerenciamento de usuários, recursos e permissões RBAC.
- Permissões separadas para consulta, administração e operação de equipes e hospitais.
- Auditoria com filtros, paginação e detalhamento dos registros.
- Dashboard com indicadores e interface responsiva com tema claro/escuro.

## Arquitetura e tecnologias

| Camada | Tecnologias principais |
| --- | --- |
| API | .NET 8, ASP.NET Core, Entity Framework Core 9, PostgreSQL e Swagger |
| Segurança | JWT, refresh tokens, BCrypt e RBAC |
| Tempo real | SignalR |
| IA | API compatível com OpenAI da Groq; modelo configurável |
| Geolocalização | ViaCEP, Nominatim, Overpass, OSRM e OpenStreetMap |
| Web | React 19, TypeScript 5.9, Vite 7, Material UI 7, Axios e Leaflet |
| Testes | xUnit, ASP.NET Core Test Host e Testcontainers/PostgreSQL |
| Entrega | Docker Compose, GitHub Actions e Semantic Release |

O backend organiza regras em controllers, serviços, validações, repositórios e configurações do Entity Framework. O frontend usa contextos e hooks, serviços Axios, rotas protegidas e componentes Material UI. O PostgreSQL persiste usuários, permissões, ocorrências, análises, equipes, despachos, hospitais, transportes e logs.

## Estrutura do repositório

```text
.
├── Api/                         # API ASP.NET Core, domínio e persistência
│   ├── AI/                      # Cliente e contratos da análise por IA
│   ├── Auditing/                # Registro e consulta de auditoria
│   ├── Controllers/             # Endpoints HTTP
│   ├── Data/                    # DbContext, migrations e seeds
│   ├── Geography/               # Overpass, OSRM e contratos de mapa
│   ├── Realtime/                # Hub e notificações SignalR
│   ├── Security/                # JWT, senhas, permissões e políticas
│   ├── Services/                # Casos de uso
│   └── Validations/             # Regras e transições de estado
├── WebApp/                      # SPA React/TypeScript
│   └── src/
│       ├── components/          # Componentes visuais e operacionais
│       ├── contexts/            # Estado compartilhado
│       ├── pages/               # Telas do sistema
│       ├── permissions/         # Regras de acesso da interface
│       ├── routes/              # Rotas públicas e protegidas
│       └── services/            # API e SignalR
├── tests/Api.Tests/             # Testes unitários e de integração
├── docker-compose.development.yml
├── docker-compose.staging.yml
└── docker-compose.production.yml
```

## Pré-requisitos

Para a execução recomendada, instale Docker e Docker Compose. Para executar diretamente, use .NET SDK 8, PostgreSQL 14+ e Node.js compatível com Vite 7 (Node 20.19+ ou 22.12+). Os testes de integração usam Testcontainers e também precisam de Docker.

## Início rápido com Docker

1. Crie o arquivo da API:

   ```bash
   cp Api/.env.example Api/.env
   ```

   No PowerShell: `Copy-Item Api/.env.example Api/.env`.

2. Ajuste ao menos estes valores em `Api/.env`:

   ```env
   DB_HOST=db
   DB_PORT=5432
   DB_USER=postgres
   DB_PASSWORD=postgres
   DB_NAME=admin_panel_db

   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=postgres
   POSTGRES_DB=admin_panel_db

   RUN_USERS_SEED=true
   RUN_EMERGENCY_UNITS_SEED=true
   HOSPITAL_USERS_DEFAULT_PASSWORD=Hospital@123

   API_PORT=5210
   JWT_SECRET_KEY=substitua-por-uma-chave-longa-aleatoria-e-segura
   WEB_APP_URL=http://localhost:5173

   RESEND_API_KEY=chave-do-resend
   RESEND_FROM_EMAIL=remetente@exemplo.com
   ```

3. Inicie os serviços:

   ```bash
   docker compose -f docker-compose.development.yml up --build
   ```

4. Acesse:

   - Aplicação: <http://localhost:5173>
   - API: <http://localhost:5210/api>
   - Swagger: <http://localhost:5210/swagger>

As migrations e os dados iniciais são aplicados automaticamente na inicialização da API.

### Acesso inicial

- Usuário: `root`
- Senha: `root1234`

O `root` possui acesso administrativo total. Troque a senha antes de qualquer uso fora do desenvolvimento.

O seed cria cinco contas de recepção (`hospital.upa`, `hospital.prosaude`, `hospital.mandic`, `hospital.unimed` e `hospital.santacasa`) com a senha de `HOSPITAL_USERS_DEFAULT_PASSWORD`. As seis equipes simuladas só são criadas com `RUN_EMERGENCY_UNITS_SEED=true`.

Para encerrar sem apagar o volume `pgdata`:

```bash
docker compose -f docker-compose.development.yml down
```

## Execução local sem Docker

Suba somente o banco:

```bash
docker compose -f docker-compose.development.yml up -d db
```

No `Api/.env`, use `DB_HOST=localhost`, mantenha `API_PORT=5210` e configure as demais variáveis. Depois:

```bash
dotnet restore AdminPanel.sln
dotnet run --project Api/Api.csproj
```

Em outro terminal:

```bash
cd WebApp
cp .env.example .env
npm ci
npm run dev
```

No PowerShell, substitua `cp` por `Copy-Item`. Configure `WebApp/.env` assim:

```env
VITE_API_BASE_URL=http://localhost:5210/api
VITE_MAP_TILE_URL=https://tile.openstreetmap.org/{z}/{x}/{y}.png
```

## Configuração

### API (`Api/.env`)

| Variável | Finalidade |
| --- | --- |
| `DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`, `DB_NAME` | Conexão PostgreSQL da API |
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` | Inicialização do container PostgreSQL |
| `API_PORT` | Porta da API; o Compose de desenvolvimento publica `5210` |
| `JWT_SECRET_KEY` | Assinatura dos tokens; use valor longo, aleatório e secreto |
| `WEB_APP_URL` | Origem aceita pelo CORS |
| `RUN_USERS_SEED` | Cria usuários fictícios quando `true` |
| `RUN_EMERGENCY_UNITS_SEED` | Cria seis equipes simuladas quando `true` |
| `HOSPITAL_USERS_DEFAULT_PASSWORD` | Senha inicial das contas de recepção |
| `RESEND_API_KEY`, `RESEND_FROM_EMAIL` | Redefinição de senha por e-mail |
| `GROQ_API_KEY` | Habilita a IA; sem ela, o restante da aplicação permanece disponível |
| `GROQ_MODEL`, `GROQ_BASE_URL`, `GROQ_TIMEOUT_SECONDS` | Configuração do provedor de IA |
| `OVERPASS_BASE_URLS` | Endpoints de descoberta de hospitais, separados por vírgula |
| `OSRM_BASE_URL`, `NOMINATIM_BASE_URL` | Roteamento e geocodificação |
| `MAP_HTTP_TIMEOUT_SECONDS`, `MAP_USER_AGENT` | Timeout e identificação nas integrações de mapa |

Não versione arquivos `.env` com segredos reais.

### Frontend (`WebApp/.env`)

| Variável | Finalidade |
| --- | --- |
| `VITE_API_BASE_URL` | URL-base da API |
| `VITE_MAP_TILE_URL` | Provedor de tiles do Leaflet |

## Permissões

| Recurso | Escopo |
| --- | --- |
| `root` | Acesso administrativo total |
| `users` | Gerenciamento de usuários |
| `resources` | Gerenciamento de recursos/permissões |
| `reports` | Consulta da auditoria |
| `occurrences` | Operação da central |
| `units-view` | Consulta de equipes |
| `units-manage` | Administração de equipes |
| `unit-operations` | Chamados da equipe vinculada |
| `hospitals-view` | Consulta de hospitais |
| `hospitals-manage` | Administração de hospitais e alas |
| `hospital-operations` | Recepção do hospital vinculado |

O backend aplica as permissões e os vínculos operacionais; o frontend também protege rotas e ações.

## Fluxo operacional

1. A central registra a ocorrência e o endereço.
2. A IA sugere classificação, prioridade e serviços; um operador confirma ou corrige.
3. O sistema recomenda equipes e a central confirma os despachos.
4. Cada equipe acompanha o chamado e atualiza deslocamento e atendimento.
5. Se necessário, solicita transporte, define um destino e avisa o hospital.
6. A recepção confirma ciência e recebimento do paciente.
7. A ocorrência é finalizada, preservando histórico e auditoria.

## Qualidade e testes

```bash
dotnet test AdminPanel.sln
```

```bash
cd WebApp
npm ci
npm run lint
npm run build
```

A suíte cobre validações, transições de estado, distâncias, recomendações, permissões, API e persistência PostgreSQL.

## CI/CD

Os workflows em `.github/workflows` compilam e testam a API em pushes e pull requests. Em `main` e `staging`, o pipeline também executa Semantic Release, publica a imagem da API no Docker Hub e aciona o deploy configurado por webhook. O histórico fica em `CHANGELOG.md`.

Os commits seguem [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `refactor:`, `test:` e `chore:`.

## Integrações e cuidados

- Os endpoints públicos de Nominatim, Overpass, OSRM e OpenStreetMap têm políticas e limites próprios. Em produção, use infraestrutura com SLA adequado.
- A IA é assistiva: a confirmação humana faz parte do fluxo.
- Resend e Groq dependem de credenciais externas. Nunca inclua chaves no repositório.
- Dados iniciais, localizações e contas padrão são fictícios ou voltados ao ambiente acadêmico.
