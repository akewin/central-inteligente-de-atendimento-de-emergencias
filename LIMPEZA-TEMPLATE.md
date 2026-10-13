# Revisão de sobras do template

Revisão realizada em 12/10/2026, por referências no código, rotas, registros de
dependências, chamadas da interface e testes. Ausência de referência textual não
foi usada isoladamente para remover componentes registrados por reflexão.

## Removido

| Item | Motivo |
| --- | --- |
| `DashboardDevelopmentTips` | Exibia instruções do template para desenvolvedores no dashboard. |
| `App.css`, `index.css`, `assets/logo.svg` | Nenhum import ou referência nos arquivos da aplicação. A aparência atual não dependia deles. |
| `detectAndFetchEntity` | Helper exportado, sem consumidores. |
| `filterAssignablePermissions` | Função exportada, sem chamadas. As regras ativas de permissão foram preservadas. |
| `confirmOccurrenceHospital` no frontend | Cliente HTTP sem chamadas na interface. O endpoint da API foi preservado. |
| `AccessPermissionCreateDto`, `SystemLogCreateDto` | Classes sem consumidores no backend ou nos testes. |
| `@mui/x-date-pickers-pro` | Sem imports; os relatórios usam a versão comum de `@mui/x-date-pickers`, mantida. O lockfile foi atualizado. |
| `Microsoft.AspNetCore.OpenApi` | Sem uso das APIs deste pacote; a documentação Swagger utiliza Swashbuckle, mantido. |
| `docker-compose.yml` | Arquivo vazio; os arquivos específicos de ambiente continuam disponíveis. |
| `REFACTOR.md` | Proposta antiga para transformar o template em outro template, sem representar a arquitetura atual. |

## Ajustado

- `SimulatedGeocodingService` foi movido da API para `tests/Api.Tests/Support`:
  os únicos consumidores eram testes. O serviço real Nominatim permanece na API.
- Os READMEs da API e da interface receberam o nome do projeto; a indicação de
  React foi corrigida para 19 e a referência inexistente a repositório genérico foi removida.
- A gravação de hospitais passou a usar `if/else` no lugar de uma expressão
  condicional com efeitos colaterais, corrigindo o erro existente de lint.

## Mantido por ter uso ou exigir contexto externo

- Autenticação local e externa, refresh tokens, recuperação de senha, usuários,
  recursos, permissões e auditoria: possuem fluxos conectados ao sistema.
- Configurações do Entity Framework e classes descobertas por reflexão:
  não são código morto apenas por não terem uma instanciação explícita.
- Migrations antigas e colunas históricas: necessárias para bancos existentes.
- Endpoints públicos sem consumidor atual na interface: ausência de chamadas
  na SPA não comprova ausência de clientes externos.
- Font Awesome, calendários comuns, date-fns e demais bibliotecas usadas nos imports.
- Workflows de publicação e ambientes staging/produção: ainda contêm referências
  herdadas (`therermz/admin-boiler-plate`, `app-name` e uma branch antiga no workflow
  de release). Não há evidência local suficiente para concluir que esses destinos
  e execuções manuais podem ser excluídos sem afetar uma implantação.

## Validação

- Suíte completa do backend: 140 testes aprovados, nenhum com falha ou ignorado,
  incluindo testes de integração com PostgreSQL temporário no Docker.
- Lint completo e compilação de produção da interface: aprovados.
- Interface e Swagger responderam com HTTP 200 após as alterações.
- O lockfile passou de 373 para 346 entradas de pacotes, removendo 27 entradas
  ao retirar a dependência Pro e suas dependências exclusivas.
- Permanecem avisos de compilação sobre nulabilidade e métodos sem `await` no
  backend, além dos avisos do frontend sobre tamanho do bundle e anotações SignalR.
- Conferência final em 13/10/2026: `git diff --check` sem problemas.

## Limites

Esta revisão identifica sobras verificáveis no repositório. Ela não mede quais
telas são usadas pelos operadores nem audita serviços externos, secrets do GitHub
ou implantações remotas. Essas informações não podem ser inferidas apenas pelos imports.
