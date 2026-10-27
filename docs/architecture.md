# Arquitetura

## 1. Visão geral

```text
┌──────────────────────────┐        HTTPS / JSON         ┌──────────────────────────────┐
│  Frontend (SPA)          │  Authorization: Bearer JWT  │  Backend (ASP.NET Core API)  │
│  React + TS + Vite       │ ──────────────────────────► │  Api → Application → Domain  │
│  React Router, Axios,    │ ◄────────────────────────── │        ↑                     │
│  TanStack Query          │  refresh token (cookie      │  Infrastructure (EF Core)    │
└──────────────────────────┘   httpOnly)                 └──────────────┬───────────────┘
                                                                        │
                                                               ┌────────▼────────┐
                                                               │   PostgreSQL    │
                                                               └─────────────────┘
```

- Monorepo com frontend e backend independentes, cada um executável separadamente.
- PostgreSQL via Docker Compose em desenvolvimento.
- Comunicação exclusivamente via API REST.

## 2. Estrutura de diretórios

```text
avatar-codex/
├── .claude/
├── docs/
├── backend/
│   ├── AvatarCodex.sln
│   └── src/
│       ├── AvatarCodex.Api/
│       │   ├── Controllers/
│       │   ├── Middleware/          # tratamento global de exceções
│       │   ├── Filters/
│       │   ├── Extensions/          # registro de DI, auth, CORS, Swagger
│       │   └── Program.cs
│       ├── AvatarCodex.Application/
│       │   ├── DTOs/
│       │   ├── Services/            # casos de uso + ProgressionService
│       │   ├── Interfaces/
│       │   ├── Validators/          # FluentValidation
│       │   └── Mappings/            # mapeamento manual entidade ↔ DTO
│       ├── AvatarCodex.Domain/
│       │   ├── Entities/
│       │   ├── Enums/
│       │   ├── Exceptions/
│       │   ├── Interfaces/
│       │   └── ValueObjects/        # ImageInfo
│       └── AvatarCodex.Infrastructure/
│           ├── Data/                # AppDbContext, DatabaseInitializer, Seed/ (JSON + ContentSeeder)
│           ├── Repositories/
│           ├── Configurations/      # IEntityTypeConfiguration<T>
│           ├── Migrations/
│           └── Services/            # JwtTokenService, PasswordHasher, Clock
├── frontend/
│   └── avatar-codex-web/
│       ├── cypress/                 # testes E2E
│       └── src/                     # ver §4
├── tests/
│   ├── AvatarCodex.UnitTests/
│   └── AvatarCodex.IntegrationTests/
├── .env.example
├── .gitignore
├── CLAUDE.md
├── README.md
└── docker-compose.yml
```

> **Decisão D1:** testes .NET em `tests/` na raiz (referenciados pela solução); testes de frontend (Vitest) ficam junto dos componentes (`*.test.tsx`) e o Cypress fica dentro do projeto frontend, onde estão as dependências Node.

## 3. Backend

### 3.1 Camadas e dependências

```text
Api ──► Application ──► Domain
 │            ▲
 └──► Infrastructure ──► Application, Domain
```

| Camada | Responsabilidade | Não pode |
|---|---|---|
| **Api** | Rotas HTTP, autenticação, model binding, chamar serviço, mapear resultado para status HTTP, middleware de erros. | Conter regra de negócio ou acessar o DbContext diretamente. |
| **Application** | Casos de uso (`CharacterService`, `QuizService`, `ProgressionService`…), DTOs, validators, interfaces de repositório e de serviços técnicos. | Depender de EF Core, ASP.NET ou detalhes de infraestrutura. |
| **Domain** | Entidades, enums, exceções de domínio (`NotFoundException`, `ConflictException`, `BusinessRuleException`), regras puras (ex.: `LevelCalculator`). | Ter dependências externas. |
| **Infrastructure** | `AppDbContext`, configurações EF, repositórios, migrations, seed, JWT, hash de senha, relógio (`TimeProvider`). | Conter regra de negócio. |

### 3.2 Padrões adotados

- **Repositórios simples** apenas onde agregam (consultas com filtros/paginação); sem repositório genérico nem Unit of Work próprio (o `DbContext` já é um).
- **Mapeamento manual** de DTOs (sem AutoMapper): explícito e fácil de explicar.
- **FluentValidation** para os DTOs de entrada.
- **`TimeProvider`** (nativo do .NET) abstrai o relógio para tornar testáveis desafios diários e timestamps.
- **Timestamps automáticos** (`CreatedAt`/`UpdatedAt`) via override de `SaveChangesAsync`.
- **Swagger/OpenAPI** em desenvolvimento.
- **Serviços de aplicação** como classes concretas registradas no DI (`CharacterService`, `NationService`, `CatalogService` para elementos/habilidades/séries). Só os repositórios têm interface, porque são a fronteira com a infraestrutura e o ponto de troca nos testes.
- **Repositórios de leitura** (`ICharacterRepository`, `INationRepository`, `ICatalogRepository`) usam `AsNoTracking` e devolvem entidades; o mapeamento para DTO acontece na Application (`Mappings/ContentMappings.cs`).
- **Validação** em `Validators/` (FluentValidation), executada pelo serviço com `ValidateAndThrowAsync`, para que a regra valha para qualquer chamador, não só para o controller.

### 3.3 Gamificação centralizada

Todos os ganhos de progresso passam por um único serviço:

```text
evento (descoberta, quiz concluído, …)
        │
        ▼
ProgressionService.RegisterAsync(userId, activity)
  1. grava ExperienceTransaction (idempotente: unique UserId+Type+SourceId;
     violação de unicidade = "já concedido", sem XP e sem erro)
  2. atualiza User.ExperiencePoints
  3. LevelCalculator → detecta subida de nível
  4. atualiza progresso dos desafios diários → conclui e concede XP se atingiu a meta
  5. avalia conquistas → desbloqueia e concede XP (sem nova avaliação em cascata)
  6. retorna RewardResult { xpGained, totalXp, level, leveledUp, achievementsUnlocked[], challengesCompleted[] }
        │
        ▼
tudo em uma única transação de banco
```

O `RewardResult` volta na resposta do endpoint que originou o evento, e o frontend usa esse retorno para os toasts de XP, nível e conquista. Assim nenhuma regra de gamificação é duplicada no frontend.

**Implementado na Fase 6 (passos 1–3 e 6):** `ProgressionService.GrantAsync` usa a tabela de XP de `ExperienceRewards` (único lugar com os valores, RN-04) e o `IProgressionRepository`, que numa transação grava o registro da atividade (ex.: a descoberta) e a `ExperienceTransaction` e soma o XP com `UPDATE ... SET experience_points = experience_points + n` (sem perder XP com recompensas simultâneas diferentes). Violação de unicidade → rollback e `null` ("já concedido", sem erro). Desafios (passo 4) entram na Fase 11.

**Implementado na Fase 8 (passo 5):** depois do XP do evento, o `ProgressionService` lê o catálogo, as conquistas já desbloqueadas e as métricas do usuário (`IAchievementRepository`) e desbloqueia as cumpridas segundo `AchievementRules` (o mesmo código calcula o progresso exibido em `/api/users/me/achievements`). As métricas são lidas uma única vez, antes do XP das conquistas, por isso esse XP não reavalia nada (RN-08). Cada desbloqueio usa o mesmo `SaveWithExperienceAsync` (`UserAchievement` + `ExperienceTransaction` do tipo `AchievementUnlocked`): com requisições simultâneas, a unicidade faz só uma delas desbloquear. O evento e as conquistas ficam em transações separadas; uma conquista que não foi gravada sai no próximo evento, porque a avaliação sempre considera todas as que continuam bloqueadas. O cadastro chama `EvaluateAchievementsAsync` para o "Primeiro passo".

**Fase 9:** eventos com mais de uma recompensa usam `GrantAllAsync(userId, ExperienceGrant[])`: cada `ExperienceGrant` (tipo, `SourceId`, descrição e, opcionalmente, o registro da atividade) passa pelo mesmo `SaveWithExperienceAsync`, as já concedidas são ignoradas, e as conquistas são avaliadas uma vez no fim, mesmo sem XP novo (um quiz refeito muda as respostas corretas). No quiz, a tentativa é gravada antes (sempre) e as recompensas são `QuizCompleted` + `QuizPerfect` com `SourceId` = id do quiz. A correção fica no `QuizService`; o DTO de opção usado para jogar não tem `IsCorrect`, então não há como vazar a resposta por engano (RN-09).

- **Token opcional:** `GET /api/characters`, `/api/characters/{id}` e `/api/nations/{id}` leem o usuário do token quando ele existe e preenchem `isFavorite`/`isDiscovered` com uma consulta extra pelos ids da página (`IUserCollectionRepository.GetMarksAsync`).
- **Frontend:** as queries de personagens incluem o id do usuário na chave e só disparam depois que a sessão foi verificada; a descoberta é enviada por `useDiscovery` (uma vez por página, só com o conteúdo já exibido) e o `reward` atualiza XP/nível em memória (`AuthProvider.updateUser`).

### 3.4 Autenticação

- **Access token JWT** de curta duração (15 min), enviado no header `Authorization: Bearer`.
- **Refresh token** opaco de longa duração (7 dias), em **cookie httpOnly + Secure + SameSite**, armazenado no banco como **hash** e rotacionado a cada uso.
- O frontend guarda o access token **apenas em memória**. Ao recarregar a página, chama `POST /api/auth/refresh` para recuperar a sessão.
- Senhas com `PasswordHasher<User>` do ASP.NET Core Identity (PBKDF2), sem usar o Identity completo.
- CORS restrito à origem do frontend, com `AllowCredentials` por causa do cookie.
- **Implementação (Fase 5):** `AuthService` (Application) concentra cadastro, login, refresh, logout e "me"; `JwtTokenService` e `PasswordHasher` (Infrastructure/Security) ficam atrás de `ITokenService`/`IPasswordHasher`. O refresh token é um valor aleatório de 512 bits; no banco fica só o SHA-256 dele. O access token leva apenas `sub`, `unique_name` e `jti`.
- **Frontend:** `services/api.ts` guarda o access token em memória, injeta o header e, ao receber 401, faz **um** refresh e repete a requisição. Refreshes simultâneos compartilham a mesma promessa, porque cada refresh token só vale uma vez (isso também evita a corrida do StrictMode ao recuperar a sessão). Se o refresh falha, a sessão local é encerrada.
- **Cookie em produção:** `SameSite=Strict` funciona em dev porque `localhost:5173` e `localhost:5080` são o mesmo site. Com frontend e API em domínios diferentes (Fase 17), será preciso `SameSite=None` (ou servir a API sob o mesmo domínio).
- **Fora do escopo por enquanto:** detecção de reuso de refresh token (revogar todas as sessões ao reapresentar um token já rotacionado) e limite de tentativas de login. Ficam registrados como melhorias de segurança.

> **Decisão D2:** existe uma alternativa mais simples, com JWT único no `localStorage`. Ela é mais vulnerável a XSS e contraria a regra de não guardar informação sensível no frontend.

### 3.5 Tratamento de erros

Um `IExceptionHandler` (`Api/Middleware/GlobalExceptionHandler.cs`) converte exceções no formato padrão (`api.md` §3). Erros de model binding (ex.: `?page=abc`) passam pelo `InvalidModelStateResponseFactory` e saem no mesmo formato:

| Exceção | HTTP |
|---|---|
| `ValidationException` (FluentValidation) | 400 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `BusinessRuleException` | 422 |
| não tratada | 500 (mensagem genérica, detalhes apenas no log) |

## 4. Frontend

```text
src/
├── assets/
├── components/
│   ├── common/        # Button, Badge, CharacterAvatar, Field, Pagination, Skeleton, StatusMessage, PageHeader, Section…
│   ├── layout/        # AppLayout (skip link, rodapé), Navbar
│   ├── characters/    # CharacterCard, CharacterGrid, CharacterFilters, CharacterHero, AbilityList, RelationshipList
│   ├── nations/       # NationCard
│   ├── auth/          # AuthCard, RouteGuards (RequireAuth, GuestOnly)
│   ├── quiz/
│   ├── achievements/
│   └── profile/
├── pages/             # Home, Login, Register, Dashboard, Characters, CharacterDetails, Nations, NationDetails,
│                      # Timeline, Compare, Quizzes, QuizPlay, QuizHistory, Profile, Favorites, Achievements,
│                      # Challenges, NotFound
├── services/          # api.ts (Axios + interceptors), contentService, authService, quizService…
├── hooks/             # useContent, useCharacterQueryParams, useAuth, useAuthForm, useLogout, useToast…
├── contexts/          # AuthProvider/authContext (sessão), ToastProvider/toastContext
├── lib/               # queryClient (padrões do TanStack Query)
├── types/             # tipos espelhando os DTOs da API
├── styles/            # tokens.css, global.css
├── test/              # setup, renderRoute (app completo em memória), fixtures
├── utils/             # parse da query de personagens, rótulos dos enums, paginação, contraste
├── router.tsx         # rotas (data router) com lazy loading
└── App.tsx
```

- **Axios** com interceptors: injeta o access token e, quando recebe 401, tenta `refresh` uma vez e repete a requisição.
- **TanStack Query** para cache, deduplicação de chamadas e estados de loading/erro. *(Decisão D3.)*
- **CSS Modules + CSS custom properties** (design tokens) para estilização, e **lucide-react** para ícones. *(Decisão D4.)*
- Rotas carregadas sob demanda com `lazy` do React Router (code splitting); a Home fica no bundle principal.
- Estado de listagem (busca, filtros, ordenação, página) vive na URL: é compartilhável e funciona com "voltar". Valores inválidos na URL são descartados antes de chegar à API.
- Componentes só cuidam da interface; regras ficam no backend e a orquestração em hooks/services.

## 5. Configuração e ambiente

- `.env` (não versionado) e `.env.example` (versionado) na raiz com: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_PORT`.
- Backend: `appsettings.json` sem secrets; `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`, `Cors__AllowedOrigins` vêm de variáveis de ambiente ou de `dotnet user-secrets`.
- Frontend: `VITE_API_URL` em `.env.local` (com `.env.example` no projeto).

## 6. Docker

- `docker-compose.yml` inicial: **PostgreSQL** com volume persistente e healthcheck.
- Opcional (fases posteriores): serviços `api` e `web`.
- Migrations e seed aplicados no startup da API **apenas em Development**.

## 7. Deploy (Fase 17)

Proposta sem custo: frontend na **Vercel**, backend no **Render** (container Docker) e PostgreSQL gerenciado (**Neon** ou Render). A decisão final será documentada na fase.

## 8. Estratégia de testes

| Tipo | Ferramenta | Foco |
|---|---|---|
| Unitário backend | xUnit | `LevelCalculator`, `ProgressionService`, regras de conquistas e desafios, validação de quiz, validators. |
| Integração backend | xUnit + `WebApplicationFactory` + PostgreSQL real em banco descartável (`AVATARCODEX_TEST_DB`; Testcontainers quando houver Docker) | Cadastro/login, auth/permissões, personagens (busca, 404), favoritos, idempotência de XP, quiz sem `IsCorrect`. |
| Frontend | Vitest + React Testing Library (+ MSW para mock da API) | Login, cadastro, listagem/filtros/busca, detalhe, favoritos, perfil, quiz e feedback. |
| E2E | Cypress | 9 fluxos de `development-phases.md` (Fase 14), contra API + banco de teste com seed. |

## 9. Decisões do projeto

### 9.1 Decisões aprovadas

| # | Decisão | Detalhe | Aprovada em | Onde está documentada |
|---|---|---|---|---|
| DA-01 | XP por descoberta de nação | +10 XP na 1ª descoberta de cada nação por usuário, registrada só depois que o conteúdo principal foi exibido; validação no backend e idempotência garantida por PK composta + unique em `ExperienceTransaction` | 2026-10-26 | `requirements.md` §5.2 (RN-12), `api.md`, `database.md` |
| DA-02 | Paleta visual | Dark: fundo `#101820`, superfícies `#1B2835`; Água `#3186B5`, Fogo `#B86B35`, Terra `#638B58`, Ar `#D5C7A2`, usadas como acentos e inspiradas no anime (não reprodução oficial) | 2026-10-26 | `ui-ux.md` §2 |
| DA-03 | Tipografia | Cinzel em títulos e destaques narrativos, com moderação; Inter em texto corrido, navegação, formulários, botões e componentes | 2026-10-26 | `ui-ux.md` §3 |

**Ajustes feitos para manter a consistência com as decisões aprovadas:**
- O acento dourado `#E0B354` foi removido (conflitava visualmente com o Ar). Botões primários passam a usar um neutro claro e a barra de XP usa a cor do elemento do usuário.
- Água, Fogo e Terra ficam entre 3,7:1 e 3,8:1 sobre `surface`, abaixo de AA para texto pequeno. Por isso foram restritas a acentos (bordas, ícones, barras, títulos grandes), conforme `ui-ux.md` §2.3.
- O fluxo de descoberta de personagem segue as mesmas regras de DA-01 (RN-12), para não haver dois comportamentos diferentes.

### 9.2 Decisões técnicas (aprovadas em 2026-10-26)

| # | Decisão | Proposta | Motivo |
|---|---|---|---|
| D1 | Local dos testes | .NET em `tests/`; Vitest junto dos componentes; Cypress no frontend | Segue as convenções de cada ecossistema |
| D2 | Armazenamento de sessão | Access token em memória + refresh token em cookie httpOnly | Segurança contra XSS e recuperação de sessão |
| D3 | Data fetching | Axios + TanStack Query | Cache e deduplicação prontos, estados de loading/erro padronizados |
| D4 | Estilização / ícones | CSS Modules + tokens CSS / lucide-react | Sem dependência pesada; tema por elemento fácil de fazer |
| D5 | Nível | **Não persistido**: calculado do XP. `ExperiencePoints` persistido como total desnormalizado, atualizado na mesma transação da `ExperienceTransaction` | Evita duplicar dado derivado e mantém consultas de perfil/ranking baratas |
| D6 | Personagens com vários elementos (Avatares) | `ElementId` = elemento principal/nativo + `IsAvatar`; subtipos via N:N `CharacterBendingSubtype` | Modelo simples que cobre os Avatares sem tabela extra de elementos |
| D7 | XP de quiz refeito | XP só na 1ª conclusão; bônus de perfeito só na 1ª vez em que gabarita | Evita farm de XP |
| D8 | Desafios diários | Pool de `ChallengeTemplate` (seed) e geração preguiçosa de 3 `DailyChallenge` por dia (UTC), na primeira consulta do dia; progresso automático pelo backend | Sem job agendado, determinístico e testável com `TimeProvider` |
| D9 | `POST /challenges/{id}/complete` | **Removido**: conclusão automática no backend quando a meta é atingida | Cumpre a regra "não confiar no cliente" |
| D10 | Descoberta | Registrada por `POST` explícito ao abrir o detalhe (não como efeito colateral de `GET`) | `GET` deve ser seguro/idempotente (REST) |
| D11 | Administração de conteúdo | Sem painel admin na v1; conteúdo via seed | Escopo |
| D12 | IDs | `int` para conteúdo; `Guid` para `User` | URLs legíveis; evita enumeração de usuários |
| D13 | Paginação de personagens | Paginação numérica sincronizada com a URL (query string) | Links compartilháveis, acessível e fácil de testar |
| D14 | Metadados de imagem | Tipo owned `ImageInfo` (Url, SourceUrl, Credit) | Imagens substituíveis e com crédito (direitos autorais) |

## 10. Riscos técnicos

| Risco | Mitigação |
|---|---|
| Requisições de descoberta duplicadas (StrictMode, duplo clique, abas) | PK composta + unique em `ExperienceTransaction`; violação tratada como `isNew: false` (RN-12); teste de integração com chamadas concorrentes. |
| Escopo grande demais | Fases incrementais; fechar P0 e P1 antes de P2. |
| Volume de conteúdo para o seed (personagens, quizzes, textos) | Meta v1: ~40 personagens, 4 nações, 4 elementos, ~25 habilidades, 3–5 quizzes. Seed em arquivos JSON, separado do código. |
| Imagens e direitos autorais | URLs externas com `SourceUrl`/`Credit` e fallback visual; fácil de trocar. |
| Recompensas duplicadas / condições de corrida | Constraints únicas + transação; tratar violação de unique como "já concedido". |
| Complexidade do refresh token + CORS com cookies | Implementar e testar na Fase 5 com testes de integração; documentar a configuração. |
| Fuso horário dos desafios diários | Tudo em UTC via `TimeProvider`; UI informa o horário do reset. |
| E2E instável | Banco dedicado com seed determinístico, seletores `data-testid`, sem esperas fixas. |
| Migrations no Docker | Aplicar no startup só em Development; script/documentação para os demais ambientes. |
| Cold start no deploy gratuito (Render) | Documentar a limitação; loading amigável no frontend. |
