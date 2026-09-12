# Admin Pannel BoilerPlate Frontend - React + Vite + MaterialUI

> Frontend em **React** com **Vite** e **TypeScript**, integrado com a API .NET deste projeto.  
> Fornece um painel administrativo moderno, seguro e escalável, permitindo gerenciamento de **usuários com controle de permissões de acesso** e também o gerenciamento de **recursos do sistema**, além de permitir **auditoria** das ações executadas.
> Inclui um **fluxo completo de autenticação via JWT**, **recuperação de senha** e **rotas protegidas** contra acesso indevido.

---

## Tecnologias Utilizadas

- [**React 18**](https://reactjs.org/): Biblioteca para criação de interfaces declarativas e reativas.
- [**Vite**](https://vitejs.dev/): Bundler moderno e rápido para desenvolvimento frontend.
- [**TypeScript**](https://www.typescriptlang.org/): Superset de JavaScript que adiciona tipagem estática.
- [**MaterialUI (MUI)**](https://mui.com/): Biblioteca de componentes para React com design consistente e responsivo.
- [**Axios**](https://axios-http.com/): Cliente HTTP para consumo da API.
- [**React Router**](https://reactrouter.com/): Gerenciamento de rotas do frontend.

---

## Estrutura do Projeto

```
generic-login-dotnet-react/
│
├── WebApp/
│   ├── public/                # Arquivos estáticos
│   ├── src/
│   │   ├── api/               # Instância Axios configurada com baseURL, headers e interceptors JWT
│   │   ├── components/        # Componentes reutilizáveis (UserTable, UserForm, LoginForm)
│   │   ├── contexts/          # Configuração do ContextApi
│   │   ├── helpers/           # Funções auxiliares
│   │   ├── hooks/             # Hooks personalizados
│   │   ├── interfaces/        # Contratos de Tipagem TypeScript
│   │   ├── pages/             # Páginas principais da aplicação
│   │   ├── permissions/       # Regras do role based access control (RBAC)
│   │   ├── routes/            # Configuração das rotas
│   │   ├── App.tsx            # Configuração do layout principal
│   │   └── main.tsx           # Entrada do React e renderização do App
│   ├── tsconfig.json          # Configuração TypeScript
│   └── package.json           # Dependências e scripts do projeto
│
└── Api/                       # Backend PostgreSQL + .NET
```

---

## Funcionalidades

##### - Login

- Permite autenticação utilizando `username` ou `email` (`identifier`) e `password`.
- Disponibiliza autenticação por redirecionamento enviando `token` via url (desde que utilizando o mesmo `JWT_SECRET_KEY`).
- Armazena token no `localStorage` e configura cabeçalho `Authorization` para todas as requisições.

##### - Perfil

- Exibe as informações do usuário logado, permitindo edição (de acordo com RBAC).

##### - Gerenciamento de Usuários

- Listagem paginada e pesquisável de usuários.
- Formulário de `Criação de usuários`.
- `Edição e exclusão` de usuários diretamente da tabela.
- `Controle de permissões` por recurso do sistema.
- Exibição e edição condicionais com base nas regras RBAC.

##### - Recursos de Sistema (System Resources)

- Listagem paginada e pesquisável de recursos de sistema.
- Formulário de `Criação de recursos de sistema`.
- `Edição e exclusão` de recursos do sistema diretamente da tabela.
- Integração com a gestão de usuários (cada usuário tem uma lista `permissions`, baseada nos recursos do sistema que ele deve acessar).

##### - Relatórios de Auditoria (System Logs)

- Listagem paginada e filtrável dos logs de sistema.
- Geração de relatórios com filtros cumulativos por:
  - Período (início e fim)
  - Usuário específico
  - Ação executada (`create`, `update`, `delete`, `login`, `senha`)

##### - Hooks Personalizados

- `useAuth` gerencia token, login, logout e mantém cabeçalho de autorização configurado.
- `useUsers`, `useSystemResources` e `useReports` fazem a abstração entre a camada services e a UI, persistindo dados para exibição, ações CRUD e paginação.

---

## Rodando a aplicação localmente

### 1. Instalar dependências

```bash
cd WebApp
npm install
```

### 2. Configurar base URL da API

- Crie um arquivo `WebApp/.env` e nele defina VITE_API_BASE_URL com a url de sua api.
  > Essa variável será utilizada pelo arquivo `src/api/index.ts` para configurar a instância do axios.

### 3. Rodar a aplicação

```bash
npm run dev
```

> A aplicação estará disponível em `http://localhost:5173`.

---

## Estrutura de Rotas

| Rota            | Descrição                                                |
| --------------- | -------------------------------------------------------- |
| `/login`        | Tela de autenticação                                     |
| `/profile`      | Informações do usuário logado                            |
| `/unauthorized` | Redirecionamento em caso de acesso à rota não autorizada |
| `/users`        | Painel de gestão de usuários                             |
| `/resources`    | Página de gerenciamento de recursos do sistema           |
| `/reports`      | Relatórios de auditoria filtráveis e paginados           |

---

## Integração com API

- Todos os endpoints da aplicação são consumidos via instância configurada do **Axios**.
- Token JWT é enviado automaticamente no header `Authorization: Bearer <token>` após login.
- Todas as chamadas à api são gerenciadas pela camada `services`.

---

## 🛠️ Guia para Integrar Novos Endpoints na Interface

Para manter a consistência e facilitar a manutenção, siga estes passos ao integrar novos recursos da API na interface:

### 1. Definir Interfaces TypeScript

- Em `src/interfaces/`, crie tipos para a entidade e payloads.
- Use nomes descritivos e siga o padrão existente.

```typescript
// src/interfaces/NewEntity.ts
export interface NewEntity {
  id: number;
  name: string;
  createdAt: string;
}

export interface NewEntityCreatePayload {
  name: string;
}

export interface NewEntityUpdatePayload {
  name: string;
}
```

### 2. Criar Serviço de API

- Em `src/services/`, crie funções para consumir os endpoints.
- Use a instância Axios configurada em `src/api/index.ts`.

```typescript
// src/services/newEntityService.ts
import api from '../api';

export const getNewEntities = async (params?: any) => {
  const response = await api.get('/new-entities', { params });
  return response.data;
};

export const createNewEntity = async (payload: NewEntityCreatePayload) => {
  const response = await api.post('/new-entities', payload);
  return response.data;
};

export const updateNewEntity = async (
  id: number,
  payload: NewEntityUpdatePayload
) => {
  const response = await api.put(`/new-entities/${id}`, payload);
  return response.data;
};

export const deleteNewEntity = async (id: number) => {
  await api.delete(`/new-entities/${id}`);
};
```

### 3. Implementar Contexto (Context API)

- Em `src/contexts/`, crie `NewEntityContext.tsx`.
- Siga o padrão de `UsersContext.tsx` ou `SystemResourcesContext.tsx`.

```typescript
// src/contexts/NewEntityContext.tsx
import React, { createContext, useContext, useReducer, useEffect } from 'react';
import { NewEntity } from '../interfaces/NewEntity';
import * as newEntityService from '../services/newEntityService';

interface NewEntityState {
  entities: NewEntity[];
  loading: boolean;
  error: string | null;
  pagination: { page: number; pageSize: number; total: number };
}

const NewEntityContext = createContext<any>(null);

export const useNewEntity = () => {
  const context = useContext(NewEntityContext);
  if (!context)
    throw new Error('useNewEntity must be used within NewEntityProvider');
  return context;
};

export const NewEntityProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  // Implementação do reducer e funções CRUD...
};
```

### 4. Criar Hook Personalizado

- Em `src/hooks/`, crie `useNewEntity.ts` que usa o contexto.

```typescript
// src/hooks/useNewEntity.ts
import { useNewEntity as useNewEntityContext } from '../contexts/NewEntityContext';

export const useNewEntity = () => {
  return useNewEntityContext();
};
```

### 5. Desenvolver Componentes

- Em `src/components/`, crie componentes reutilizáveis.
- Use Material-UI e siga o padrão existente.

```typescript
// src/components/NewEntityTable.tsx
import { DataGrid } from '@mui/x-data-grid';
import { useNewEntity } from '../hooks/useNewEntity';

export const NewEntityTable: React.FC = () => {
  const { entities, loading, deleteEntity } = useNewEntity();

  // Implementação da tabela com ações...
};
```

### 6. Criar Página

- Em `src/pages/`, crie `NewEntity/index.tsx`.
- Use layout consistente e integre notificações.

```typescript
// src/pages/NewEntity/index.tsx
import { useNewEntity } from '../../hooks/useNewEntity';
import { NewEntityTable } from '../../components/NewEntityTable';
import { ConfirmDialog } from '../../components/ConfirmDialog';

export const NewEntity: React.FC = () => {
  // Implementação com estado, handlers e notificações...
};
```

### 7. Configurar Rotas

- Em `src/routes/index.tsx`, adicione a nova rota.
- Use provider e proteção de permissão.

```typescript
// src/routes/index.tsx
import { NewEntityProvider } from '../contexts/NewEntityContext';
import { NewEntity } from '../pages/NewEntity';

const routes = [
  // ... outras rotas
  {
    path: '/new-entities',
    element: (
      <NewEntityProvider>
        <NewEntity />
      </NewEntityProvider>
    ),
    requiresAuth: true,
    permission: 'PermissionsMap.NEW_ENTITIES',
  },
];
```

### 8. Adicionar Permissões

- Em `src/permissions/`, defina novas regras RBAC se necessário.
- Atualize a lógica de permissões conforme requerido.

### Padrões Importantes

- **Context API**: Use para estado global e compartilhamento entre componentes.
- **Hooks Personalizados**: Abstraem a lógica de negócio da UI.
- **Notificações**: Use `SnackbarNotification` para feedback de ações.
- **Confirmações**: Use `ConfirmDialog` para ações destrutivas.
- **Error Handling**: Trata erros de API e mostra mensagens adequadas.

---

## Sobre o Desenvolvedor

[Bruno Riwerson Silva](https://www.linkedin.com/in/bruno-riwerson/) é um **desenvolvedor full-stack** apaixonado por tecnologia e boas práticas de engenharia de software. Proficiente no uso de **React+MaterialUI** no front-end e **NodeJS+Express** no back-end, além de conhecer outras tecnologias como `Golang`, `Java`, `Docker`, entre outras. Possui experiência no uso de bancos de dados relacionais e não-relacionais, o que o torna um profissional dinâmico e apto a criar soluções escaláveis, seguras e bem estruturadas.
