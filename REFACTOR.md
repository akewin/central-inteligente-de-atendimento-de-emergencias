# REFACTOR.md

## Objetivo

Este documento descreve a refatoração arquitetural proposta para o **Template de Painel Administrativo**,
considerando escalabilidade institucional, manutenção de longo prazo, RBAC, auditoria robusta e facilidade
de onboarding de novos desenvolvedores.

O objetivo é transformar o projeto em um **padrão institucional reutilizável**, evitando acoplamentos,
helpers genéricos e regras implícitas.

---

## Visão Geral da Arquitetura

A arquitetura evolui para uma abordagem **orientada a features (vertical slice)**, separando claramente:

- **Controllers** → Entrada HTTP
- **Application** → Casos de uso
- **Domain** → Regras de negócio
- **Infrastructure** → Implementações técnicas (EF, banco, serviços externos)
- **CrossCutting** → Preocupações transversais (Auth, Logging técnico, Paginação, Exceções)

---

## Estrutura Final Recomendada

```text
Api/
├── Controllers/
│    ├── UsersController.cs
│    ├── AuthController.cs
│    ├── SystemResourcesController.cs
│    └── ReportsController.cs
│
├── Application/
│    ├── Users/
│    │    ├── Create/
│    │    ├── Update/
│    │    ├── Delete/
│    │    └── Get/
│    │
│    ├── Auth/
│    │    ├── Login/
│    │    ├── ExternalLogin/
│    │    └── ResetPassword/
│    │
│    ├── SystemResources/
│    │    ├── Create/
│    │    ├── Update/
│    │    ├── Delete/
│    │    └── Get/
│    │
│    ├── SystemLogs/
│    │    ├── Create/
│    │    └── GetReport/
│    │
│    └── SystemStats/
│         └── GetGeneralStats/
│
├── Domain/
│    ├── Entities/
│    │    ├── User.cs
│    │    ├── SystemResource.cs
│    │    ├── AccessPermission.cs
│    │    └── SystemLog.cs
│    │
│    ├── Policies/
│    │    ├── UserCreationPolicy.cs
│    │    ├── PermissionAssignmentPolicy.cs
│    │    └── AdminRestrictionsPolicy.cs
│    │
│    ├── Logging/
│    │    └── SystemLogActions.cs
│    │
│    ├── Exceptions/
│    │    └── DomainException.cs
│    │
│    └── Interfaces/
│         ├── IUserRepository.cs
│         ├── ISystemResourceRepository.cs
│         ├── IAccessPermissionRepository.cs
│         └── ISystemLogRepository.cs
│
├── Infrastructure/
│    ├── Data/
│    │    └── ApiDbContext.cs
│    │
│    ├── Repositories/
│    │    ├── UserRepository.cs
│    │    ├── SystemResourceRepository.cs
│    │    ├── AccessPermissionRepository.cs
│    │    └── SystemLogRepository.cs
│    │
│    └── Migrations/
│
├── CrossCutting/
│    ├── Auth/
│    │    ├── Jwt/
│    │    ├── Password/
│    │    └── CurrentUser/
│    │
│    ├── Authorization/
│    │    └── RequirePermissionAttribute.cs
│    │
│    ├── Logging/
│    │    └── AuditPayloadSerializer.cs
│    │
│    ├── Pagination/
│    │    └── PaginatedResult.cs
│    │
│    ├── Exceptions/
│    │    └── GlobalExceptionMiddleware.cs
│    │
│    └── Extensions/
│         └── ServiceCollectionExtensions.cs
│
├── Program.cs
└── appsettings.json
```

---

## Modelagem de Domínio

### Entidades

- **User**: entidade raiz de autenticação e autorização.
- **SystemResource**: representa permissões/capacidades do sistema.
- **AccessPermission**: vínculo User + SystemResource (grant).
- **SystemLog**: auditoria institucional obrigatória.

> Um User **não existe sem permissões**. A composição User + AccessPermission é regra de negócio.

---

## Auditoria (SystemLogs)

### Princípios

- Toda ação CREATE, UPDATE, DELETE e LOGIN gera log.
- Payloads são serializados:
  - CREATE → payload recebido
  - UPDATE → estado anterior + payload
- Consulta de logs é protegida por permissão (`reports`).

### Organização

- **Domain/Logging** → Define o *significado* do log.
- **Application/SystemLogs** → Executa criação e consultas.
- **CrossCutting/Logging** → Serialização técnica de payloads.

---

## Autenticação e Segurança

### JWT

- JWT é **infraestrutura**, não domínio.
- Backend mantém controle do token.
- Frontend apenas consome informações do usuário autenticado.

### Password

- Hash e verificação via BCrypt.
- Interface `IPasswordHasher` evita acoplamento.

### Autorização

- Bloqueio de rotas por permissão (`SystemResource`).
- Attribute ou middleware centralizado.

---

## Validações

| Tipo | Onde |
|----|----|
Estrutura do DTO | Application (Validator) |
Regra de negócio | Domain/Policies |
Permissão | Authorization |
Existência | Repositórios específicos |

❌ Evitar validações genéricas via reflection.

---

## Paginação

- Padronizada para `GetAll` e `Search`.
- Implementada como CrossCutting.
- Frontend consome sempre o mesmo contrato.

---

## Estatísticas do Sistema

- Feature somente leitura.
- Implementada em `Application/SystemStats`.
- Retorna `GeneralSystemStatsDto`.

---

## Frontend (Contexto para a API)

- RBAC refletido totalmente na UI.
- Renderização condicional baseada em permissões.
- Contextos:
  - Auth
  - Permissions
  - Theme
  - Notifications
- Forms reutilizáveis e tabelas paginadas.

A API foi estruturada para **servir exatamente esse padrão de consumo**, sem acoplamento direto.

---

## O que foi removido na refatoração

- Helpers genéricos
- GenericRepository<T>
- Services monolíticos
- Validações implícitas
- Regras escondidas em middleware

---

## Conclusão

Esta refatoração transforma o projeto em:

- Um **template institucional sólido**
- Fácil de entender
- Fácil de estender
- Seguro
- Auditável
- Preparado para troca de equipe

Este padrão deve ser replicado em todos os novos projetos derivados.

