# Testes

## 1. Visão geral

| Suíte | Ferramenta | Quantidade | Onde | Precisa de |
|---|---|---|---|---|
| Unitários backend | xUnit | 185 | `tests/AvatarCodex.UnitTests` | só o .NET SDK |
| Integração backend | xUnit + `WebApplicationFactory` + PostgreSQL real | 100 | `tests/AvatarCodex.IntegrationTests` | PostgreSQL + `AVATARCODEX_TEST_DB` |
| Frontend | Vitest + React Testing Library | 139 | `frontend/avatar-codex-web/src/**/*.test.ts(x)` | só o Node |
| E2E | Cypress + Testing Library, no Chrome | 7 testes (9 fluxos) | `frontend/avatar-codex-web/cypress/e2e` | API + build do frontend + PostgreSQL + Chrome |

Antes de considerar uma mudança pronta: `dotnet test` (com o banco), `npm run test`, `npm run lint`, `npm run format:check` e `npm run build`. O E2E roda ao fim de cada fase ou quando um fluxo principal muda.

## 2. Backend

### 2.1 Como rodar

```bash
cd backend

# Só os unitários e os de integração que não usam banco (os demais aparecem como "ignorados")
dotnet test

# Tudo, com PostgreSQL real
AVATARCODEX_TEST_DB="Host=localhost;Port=5432;Username=postgres;Password=<senha>" dotnet test
```

No PowerShell: `$env:AVATARCODEX_TEST_DB = "Host=localhost;Port=5432;Username=postgres;Password=<senha>"; dotnet test`.

`AVATARCODEX_TEST_DB` é a connection string do **servidor**, **sem** `Database`: cada classe de teste cria um banco `avatar_codex_test_<guid>`, aplica as migrations e o remove no fim. O usuário precisa poder criar bancos. Com Docker, use a porta 5433 e o usuário do `.env`.

Para rodar um grupo: `dotnet test --filter "FullyQualifiedName~Quiz"`.

### 2.2 Organização

**Unitários** (`tests/AvatarCodex.UnitTests`), por área, com repositórios falsos em `Fakes/` e `TimeProvider` controlado:

| Pasta | Cobre |
|---|---|
| `Auth` | `AuthService` (cadastro, login, refresh rotacionado, logout), validators de cadastro/login, validação de `Jwt:Secret` |
| `Characters` | `CharacterService` e o validator dos filtros/paginação |
| `Progress` | `LevelCalculator` (fórmula RN-07), `ExperienceRewards`, `ProgressionService` (idempotência, subida de nível, sem cascata de conquistas), `AchievementRules` |
| `Quizzes` | `QuizService`: correção, validação das respostas, XP só na 1ª conclusão e no 1º gabarito |
| `Challenges` | `ChallengeRules` (sorteio determinístico pela data, progresso, conclusão) e `DailyChallengeService`, simulando dias diferentes |
| `Dashboard` | `DashboardRules` (quiz em destaque, próxima conquista, recomendações com rotação diária) |
| `Timeline`, `Comparison` | Agrupamento e filtros da timeline; ordem pedida e `leaderId` da comparação |
| `Seed` | `ContentSeedData.Validate()`: nomes duplicados e referências quebradas nos JSON |

**Integração** (`tests/AvatarCodex.IntegrationTests`):

| Pasta | Cobre |
|---|---|
| `Api/` | A API inteira em memória (`WebApplicationFactory`) sobre um banco descartável com migrations + seed. Um arquivo por área: auth (cookies, refresh, 401), personagens (busca, filtros, 404, 400), favoritos e descobertas (inclusive **requisições concorrentes** sem XP duplicado), quiz (**`isCorrect` nunca exposto** antes da submissão), conquistas, desafios, dashboard, timeline, comparação, catálogo e o formato padronizado de erro |
| `Database/` | Constraints do banco (unicidade, CHECKs, PKs compostas) e idempotência do `ContentSeeder` |

Fixtures:
- `ContentApiFixture`: usada pela maioria dos testes de API. **Desativa os desafios diários**, porque o sorteio depende da data e somaria XP a eventos de outras áreas, tornando os totais imprevisíveis.
- `ChallengesApiFixture`: usada só pelos testes de desafios, com outro banco e os desafios ativos. Os testes leem os desafios sorteados no dia e agem conforme eles; a simulação de dias diferentes fica nos testes unitários, com `TimeProvider`.
- `TestDatabase`: cria e remove o banco descartável.

Sem `AVATARCODEX_TEST_DB`, os testes que precisam de banco usam `SkippableFact` e aparecem como ignorados, sem falhar.

## 3. Frontend

### 3.1 Como rodar

```bash
cd frontend/avatar-codex-web
npm run test           # uma execução
npm run test:watch     # modo watch
npm run test:coverage  # cobertura v8: resumo no terminal e relatório HTML em coverage/
```

### 3.2 Organização

- Os testes ficam ao lado do código (`*.test.tsx`). A maioria testa **páginas inteiras**: `src/test/renderWithProviders.tsx` monta o app com os providers reais (TanStack Query, sessão, toasts, modal de nível) e o roteador, numa rota escolhida.
- Os `services` (chamadas HTTP) são mockados com `vi.mock`, com dados de `src/test/fixtures.ts`. Componentes e hooks são os reais.
- Consultas por papel e nome acessível (`getByRole`, `getByLabelText`), sem `data-testid`: o teste também confere a acessibilidade básica.
- `services/api.test.ts` cobre o interceptor do Axios (token em memória, um único refresh compartilhado ao receber 401, sessão encerrada quando o refresh falha).
- `styles/tokens.test.ts` confere o contraste WCAG das cores de texto da paleta (DA-02).

Áreas cobertas: login, cadastro, rotas protegidas, layout/navbar, listagem com busca/filtros/paginação na URL, detalhe e descoberta, favoritos, nações, Home, painel, quiz (jogar, resultado, histórico), conquistas, desafios, timeline, comparação, XP/nível e o `CharacterAvatar` (placeholder e fallback quando a imagem falha).

## 4. E2E (Cypress)

### 4.1 O que cobre

Os 9 fluxos principais, em 3 arquivos:

| Arquivo | Fluxos |
|---|---|
| `auth.cy.ts` | Cria conta pelo formulário (cai no painel com "Primeiro passo"); entra, mantém a sessão ao recarregar e sai; senha errada não entra |
| `explore.cy.ts` | Pesquisa, abre o personagem, ganha +10 XP pela descoberta e favorita (aparece em `/favorites`); a segunda visita não dá XP |
| `progress.cy.ts` | Faz um quiz (correção no backend, XP na 1ª conclusão, histórico); descobre as 5 nações, desbloqueia "Historiador" e confere em `/achievements` e no perfil |

Cada teste cria o próprio usuário (pelo formulário de cadastro ou pelo comando `cy.registerViaApi`, em `cypress/support/commands.ts`), então o banco não precisa ser limpo entre execuções. Os desafios do dia podem somar XP ao mesmo evento; por isso os testes conferem o XP do próprio evento (no toast) e um mínimo no perfil, não um total exato.

### 4.2 Como rodar

O E2E roda contra o app de verdade: o **build** do frontend (`vite preview` na porta 5173, a origem liberada no CORS) e a API usando um banco **só para E2E** (`avatar_codex_e2e`, criado com migrations e seed na primeira subida).

Pré-requisitos: Google Chrome instalado e a porta 5173 livre (pare o `npm run dev`).

```bash
# Terminal 1: API com o banco de E2E (mesma connection string do user-secrets, trocando só o Database)
cd backend
ConnectionStrings__Default="Host=localhost;Port=5432;Database=avatar_codex_e2e;Username=postgres;Password=<senha>" \
  dotnet run --project src/AvatarCodex.Api --launch-profile http

# Terminal 2: build do frontend servido na 5173
cd frontend/avatar-codex-web
npm run build && npm run preview

# Terminal 3
cd frontend/avatar-codex-web
npm run e2e        # headless, no Chrome
npm run e2e:open   # interface do Cypress
```

No PowerShell, o terminal 1 fica:

```powershell
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=avatar_codex_e2e;Username=postgres;Password=<senha>"
dotnet run --project src/AvatarCodex.Api --launch-profile http
```

Em caso de falha, o Cypress salva capturas em `cypress/screenshots/` (ignorado pelo Git).

### 4.3 Por que assim

- **Chrome, não Electron:** no Electron (navegador padrão do Cypress) a troca de página depois do login às vezes não acontecia; no Chrome, 10 de 10 execuções seguidas passaram. `npm run e2e` já usa `--browser chrome`.
- **Contra o build:** mais próximo de produção e sem a otimização de dependências do servidor de desenvolvimento no meio do teste.
- **Sem esperas fixas nem `data-testid`:** o Cypress espera pelos elementos acessíveis, como um usuário os encontraria.
