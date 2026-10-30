# Fases de Desenvolvimento

## Regras do processo

- **Uma fase por vez.** Não implementar tudo de uma vez.
- Ao fim de cada fase: **executar → testar → verificar erros → revisar arquitetura → documentar → avançar**.
- Não avançar enquanto a fase atual estiver quebrada ou sem validação.
- Commits pequenos e focados (Conventional Commits), ex.:
  `feat: add character listing` · `feat: implement authentication` · `fix: prevent duplicate experience rewards` · `test: add character service tests`
- **Definition of Done** de cada funcionalidade: backend + frontend + validações + loading + error state + responsividade + testes relevantes + documentação.

## Prioridades

| Prioridade | Escopo |
|---|---|
| **P0** | Arquitetura, banco, personagens, autenticação, frontend, perfil, favoritos |
| **P1** | Descoberta, XP, níveis, conquistas, quiz |
| **P2** | Desafios, timeline, comparação |
| **P3** | Ranking, teste de elemento, funcionalidades sociais |

Se o projeto crescer demais, finalizar P0 e P1 antes de avançar.

## Fases

| Fase | Nome | Prioridade | Entregas | Critério de aceite |
|---|---|---|---|---|
| 0 | Planejamento | P0 | Análise da especificação, decisões, arquitetura, entidades, relacionamentos, rotas, endpoints, páginas, componentes, documentação inicial | Docs aprovados pelo usuário ✅ (2026-10-26) |
| 1 | Setup | P0 | Repositório, solução .NET (4 projetos + 2 de teste), projeto React/TS/Vite, Docker com PostgreSQL, configuração de ambiente, `.gitignore`, `.env.example`, README inicial | Frontend e backend executando separadamente; API conecta no banco ✅ (2026-10-26) |
| 2 | Banco e domínio | P0 | Element, BendingSubtype, Nation, Series, Character, Ability e tabelas de junção, User, RefreshToken; configurações EF, DbContext, migrations, seed | Banco sobe do zero com dados de demonstração ✅ (2026-10-26) |
| 3 | API de conteúdo | P0 | Characters, Nations, Elements, Abilities, Series; busca, filtros, paginação, ordenação; middleware de erros; Swagger | Endpoints funcionando e com testes de integração básicos ✅ (2026-10-26) |
| 4 | Frontend base | P0 | Tokens/tema (paleta DA-02, fontes Cinzel/Inter DA-03, validação de contraste), layout, navbar, rotas, componentes base, cards, loading/error/empty, responsividade; páginas Home, Characters, CharacterDetails, Nations, Timeline (placeholder) | Navegação completa sobre dados reais, mobile-first; foco visual ✅ (2026-10-26) |
| 5 | Autenticação | P0 | Cadastro, login, logout, JWT + refresh, recuperação de sessão, rotas protegidas, perfil básico | Usuário real cria conta, loga, recarrega a página e continua logado ✅ (2026-10-26) |
| 6 | Favoritos e descoberta | P0/P1 | Favoritos, descoberta de personagens e nações, histórico de descobertas, XP inicial | Experiência personalizada; descoberta de personagem e de nação (+10 XP, DA-01) registrada só após o conteúdo ser exibido, sem XP duplicado mesmo com requisições concorrentes (testado) ✅ (2026-10-26) |
| 7 | XP e níveis | P1 | ExperienceTransaction, ProgressionService, LevelCalculator, XpBar, notificações de nível, `RewardResult` | Regras centralizadas no backend, com testes unitários ✅ (2026-10-26) |
| 8 | Conquistas | P1 | Achievement, UserAchievement, regras, desbloqueio, página de conquistas, notificações | Conquistas desbloqueiam automaticamente e uma única vez ✅ (2026-10-26) |
| 9 | Quiz | P1 | Quizzes, perguntas, opções, submissão, validação, resultado, XP, histórico; UI atraente | `isCorrect` nunca exposto antes da submissão; testes cobrindo isso ✅ (2026-10-26) |
| 10 | Dashboard | P1/P2 | Endpoint agregado + página: XP, nível, progresso, conquistas, desafios, descobertas, quiz recomendado, favoritos | Destino padrão após login ✅ (2026-10-27) |
| 11 | Desafios diários | P2 | ChallengeTemplate, geração diária, progresso, conclusão automática, recompensa, histórico | Testes com `TimeProvider` simulando dias ✅ (2026-10-27) |
| 12 | Timeline | P2 | Era, TimelineEvent, personagens relacionados, navegação visual, filtros | Eras e eventos no banco, filtros por era e personagem ✅ (2026-10-27) |
| 13 | Comparação | P2 | Atributos, seleção de dois personagens, visualização (barras/radar), aviso de não-canonicidade | Comparação lado a lado com atributos do banco e aviso fixo ✅ (2026-10-27) |
| 14 | Testes | — | Completar unit, integração, frontend e E2E (ver abaixo) | Regras de negócio cobertas; E2E verde ✅ (2026-10-28) |
| 15 | Refinamento visual | — | Espaçamento, tipografia, responsividade, acessibilidade, animações, loading/empty/error, microinterações | Aparência de produto final ✅ (2026-10-28) |
| 16 | Documentação final | — | README, arquitetura, banco, API, instalação, testes, decisões técnicas | Outro dev executa o projeto só com a doc ✅ (2026-10-30) |
| 17 | Deploy | — | Frontend (Vercel/Netlify), backend (Render/Azure/Railway), PostgreSQL gerenciado; sem serviços pagos | Aplicação pública; decisões documentadas |

> Os testes **não ficam todos para a Fase 14**: cada fase já entrega os testes relevantes dela. A Fase 14 completa a cobertura e os fluxos E2E.

## Cobertura de testes esperada

**Backend:** cadastro, login, autenticação, busca de personagens, personagem inexistente, favoritos, XP, evolução de nível, conquistas, quiz, validação de respostas, permissões.

**Frontend:** login, cadastro, listagem de personagens, filtros, busca, página de personagem, favoritos, perfil, quiz, feedback de respostas.

**E2E (Cypress):**
1. Usuário cria conta.
2. Usuário realiza login.
3. Usuário pesquisa personagem.
4. Usuário abre personagem.
5. Usuário favorita personagem.
6. Usuário realiza quiz.
7. Usuário ganha XP.
8. Usuário desbloqueia conquista.
9. Usuário consulta seu progresso.

## Checklist da Fase 1 — Setup

**Repositório**
- [x] `git init`, branch `main`
- [ ] Repositório remoto no GitHub *(pendente: depende do usuário)*
- [x] `.gitignore` (.NET, Node, `.env`, IDEs, `bin/`, `obj/`, `node_modules/`, `dist/`)
- [x] `.env.example` na raiz (variáveis do PostgreSQL)

**Docker**
- [x] `docker-compose.yml` com PostgreSQL 18 (volume, healthcheck, variáveis vindas do `.env`, porta padrão 5433)
- [ ] `docker compose up -d` validado *(pendente: Docker não instalado na máquina de desenvolvimento; alternativa com PostgreSQL local documentada no README)*

**Backend**
- [x] `backend/AvatarCodex.sln` (.NET 10)
- [x] Projetos `AvatarCodex.Api`, `.Application`, `.Domain`, `.Infrastructure` em `backend/src/` com referências entre camadas (Api → Application/Infrastructure; Infrastructure → Application/Domain; Application → Domain)
- [x] Projetos `tests/AvatarCodex.UnitTests` e `tests/AvatarCodex.IntegrationTests` (xUnit) na solução
- [x] Pacotes: EF Core + Npgsql + NamingConventions (snake_case), HealthChecks EF, EF Design, OpenAPI nativo + Swagger UI
- [x] `AppDbContext` vazio, com a connection string fora do Git (user-secrets/env)
- [x] `GET /api/health` (JSON com o status do banco: 200 Healthy / 503 Unhealthy)
- [x] CORS para `http://localhost:5173` com credenciais
- [x] Swagger em Development (`/swagger`)
- [x] `dotnet build` (0 avisos) e `dotnet test` passando
- [x] Conexão real com o banco validada (PostgreSQL 18 local, `/api/health` → 200 Healthy)

**Frontend**
- [x] `frontend/avatar-codex-web` com Vite 8 + React 19 + TypeScript (strict)
- [x] Dependências: React Router, Axios, TanStack Query, lucide-react, @fontsource (Cinzel/Inter)
- [x] Dev: Vitest + React Testing Library + jsdom; oxlint + Prettier
- [x] `services/api.ts` com `VITE_API_URL`; `.env.example` do frontend
- [x] Tokens da paleta (DA-02) e fontes (DA-03) carregados
- [x] Página inicial consumindo `/api/health` (estados: verificando / conectado / banco indisponível / API indisponível)
- [x] `npm run build`, `npm run test`, `npm run lint` e `npm run format:check` passando

**Documentação**
- [x] README inicial: conceito, stack, pré-requisitos, como subir banco/backend/frontend, variáveis de ambiente, aviso de direitos autorais

**Ajustes em relação ao planejado:**
- **Linter:** o template atual do Vite usa **oxlint** no lugar do ESLint. Mantido, por ser mais rápido e sem configuração extra.
- **Swagger:** o .NET 10 gera o OpenAPI nativamente (`Microsoft.AspNetCore.OpenApi`); o Swashbuckle entra só para a UI (`Swashbuckle.AspNetCore.SwaggerUI`).
- **JwtBearer e FluentValidation** serão instalados nas fases que os usam (Fase 5 e Fase 3), para não deixar dependências sem uso.
- **Pastas vazias** da estrutura (`components/`, `hooks/`, `Controllers/`…) serão criadas quando receberem o primeiro arquivo, porque o Git não versiona pastas vazias.
- **Portas:** API `5080` (http) / `7080` (https), frontend `5173`, PostgreSQL via Docker `5433`.

**Resultado esperado:** frontend e backend executando separadamente, com a API conectada ao PostgreSQL. ✅ Concluído.

## Fase 2 — Banco e domínio (concluída)

- [x] Entidades: `Element`, `BendingSubtype`, `Nation`, `Series`, `Ability`, `Character`, `CharacterRelationship`, `User`, `RefreshToken` + value object `ImageInfo`
- [x] Configurações EF (`IEntityTypeConfiguration`), `snake_case`, FKs, índices, uniques, check constraints, `citext` para email/username
- [x] `AppDbContext` com timestamps automáticos via `TimeProvider`
- [x] Migration `InitialContentSchema`
- [x] Seed em JSON com validação e upsert idempotente (37 personagens, 23 habilidades, 9 subtipos, 5 nações, 4 elementos, 2 séries, 79 relações)
- [x] Migrate + seed automáticos no startup em Development
- [x] Testes: 5 unitários (validação do seed) + 8 de integração com PostgreSQL real (seed, idempotência, atualização, uniques case-insensitive, check constraints, timestamps)

**Ajustes em relação ao planejado:**
- `Character.Name` passou a ser **único**: é a chave natural do seed.
- `IClock` foi substituído pelo `TimeProvider` nativo do .NET.
- Testes de integração usam um banco descartável no PostgreSQL configurado em `AVATARCODEX_TEST_DB` em vez de Testcontainers, porque não há Docker no ambiente atual. A troca para Testcontainers fica restrita a `TestDatabase.cs`.
- `CharacterRelationship` entrou já na Fase 2 (estava implícita), por fazer parte do conteúdo do personagem.
- Imagens e usuário de demonstração ficam para as Fases 4 e 5.

## Fase 3 — API de conteúdo (concluída)

- [x] `GET /api/characters` com busca (nome + apelidos), filtros (nação, elemento, tipo, status, série), ordenação (nome, nação, elemento; asc/desc) e paginação
- [x] `GET /api/characters/{id}` e `GET /api/characters/{id}/abilities`
- [x] `GET /api/nations`, `GET /api/nations/{id}`, `GET /api/elements`, `GET /api/abilities`, `GET /api/series`
- [x] Validação com FluentValidation e mensagens em português
- [x] Tratamento global de erros (`IExceptionHandler`) com o formato `{ status, message, errors }`, inclusive para erros de binding
- [x] Swagger documentando respostas 200/400/404
- [x] Testes: 17 unitários (serviço e validator) + 33 de integração (endpoints com PostgreSQL real e handler de erros)

**Ajustes em relação ao planejado:**
- Personagens sem nação/elemento ficam sempre no fim da ordenação por esses campos, e o desempate por id garante paginação estável.
- `%` e `_` na busca são tratados como texto, não como curinga.
- Os campos `isFavorite`/`isDiscovered` dos endpoints públicos ficam para a Fase 6.

## Fase 4 — Frontend base (concluída)

- [x] Tokens completos: paleta DA-02, cores de feedback (`success`/`danger`/`warning`), escala tipográfica, espaçamento, raios, sombras, transições
- [x] Validação automática de contraste (`tokens.test.ts` lê os valores reais de `tokens.css`)
- [x] Layout com navbar fixa (menu hambúrguer no mobile), skip link, rodapé com aviso de direitos autorais
- [x] Rotas com React Router (data router) e code splitting por página; Home no bundle principal
- [x] Componentes base: `Button`/`ButtonLink`, `Badge`/`ElementBadge`, `ElementIcon`, `CharacterAvatar`, `SearchInput`, `Select`, `Pagination`, `Skeleton`, `EmptyState`, `ErrorState`, `PageHeader`, `Section`, `BackLink`
- [x] Páginas: Home (hero, Avatares, elementos), Characters (busca com debounce, filtros, ordenação, paginação, tudo na URL), CharacterDetails, Nations, NationDetails, Timeline (placeholder), NotFound
- [x] Loading com skeletons, empty state com ação, error state com "Tentar novamente", 404 de conteúdo
- [x] Placeholders de personagem com iniciais e cor do elemento (DA-04)
- [x] Testes: 43 (contraste, parse da URL, paginação, iniciais, listagem, detalhe, navegação/404)
- [x] Verificação visual em 1366px e 390px com dados reais

**Ajustes em relação ao planejado:**
- A cor de acento dos componentes vem do campo `color` do elemento retornado pela API (`--element-color` inline), em vez de `data-element`: o conteúdo continua no banco e não há mapeamento duplicado no frontend.
- Filtros no mobile/tablet ficam num painel recolhível (botão "Filtros" com `aria-expanded`) em vez de drawer com trap de foco; no desktop viram sidebar. O drawer pode entrar na Fase 15 se fizer falta.
- No mobile o card de personagem vira item de lista (avatar ao lado do texto), mais compacto que o grid de 2 colunas.
- O status da API saiu da Home: cada seção trata o próprio erro.
- MSW não foi adotado: os testes mockam `contentService` com `vi.mock`, o que é suficiente para o volume atual.

## Fase 5 — Autenticação (concluída)

- [x] `POST /api/auth/register` (já inicia a sessão), `login`, `refresh`, `logout` e `GET /api/auth/me`
- [x] Access token JWT (15 min, HMAC-SHA256) no header; refresh token opaco (7 dias) em cookie `httpOnly`/`Secure`/`SameSite=Strict`, salvo como hash e rotacionado a cada uso
- [x] Senhas com PBKDF2 (`PasswordHasher<User>`), mesma mensagem para email inexistente e senha errada
- [x] Validação com FluentValidation (RN-01, RN-02), conflito de email/username → 409, inclusive na corrida entre cadastros simultâneos (unique do banco)
- [x] 401 no formato padrão de erro, inclusive o desafio do JwtBearer
- [x] `LevelCalculator` (RN-07) antecipado da Fase 7, para o `level` do usuário vir do backend
- [x] Usuário demo em Development (README)
- [x] Frontend: token só em memória, refresh automático após 401 com uma única chamada simultânea, recuperação de sessão ao recarregar, `AuthProvider`/`useAuth`, toasts (`ToastProvider`/`useToast`)
- [x] Páginas Login e Cadastro (erros da API por campo com `aria-describedby`, mostrar senha), Perfil básico (`/profile`), rotas `RequireAuth`/`GuestOnly` com `?redirect=` protegido contra open redirect
- [x] Navbar com sessão (entrar/criar conta ou usuário + sair), CTA de cadastro na Home para visitantes
- [x] Testes: backend 111 (59 unitários + 52 de integração) e frontend 67
- [x] Fluxo validado num Chrome real: cadastro inválido → cadastro → reload (sessão mantida) → logout → rota protegida → senha errada → login → reload no mobile

**Ajustes em relação ao planejado:**
- `logout` passou a ser 🔓 com cookie (em vez de 🔒): funciona mesmo com o access token expirado e é idempotente.
- O cadastro já devolve a sessão (corpo do login + cookie), para o usuário não precisar logar logo depois de criar a conta.
- O frontend **não repete as regras de validação**: os erros por campo vêm da resposta 400 da API (regra "não duplicar lógica"). Os textos de ajuda dos campos só descrevem o formato esperado.
- Destino padrão depois do login é `/profile`, até o Dashboard existir (Fase 10 — hoje é `/dashboard`).
- O "Primeiro passo" (conquista de cadastro) fica para a Fase 8.

## Fase 6 — Favoritos e descoberta (concluída)

- [x] Entidades `UserCharacterDiscovery`, `UserNationDiscovery`, `UserFavoriteCharacter`, `ExperienceTransaction` (+ enum `ExperienceType`) e migration `AddFavoritesDiscoveriesAndExperience`
- [x] `ProgressionService` (XP idempotente + subida de nível) e `ExperienceRewards` (tabela oficial de XP em um só lugar)
- [x] `POST /api/characters/{id}/discovery` e `POST /api/nations/{id}/discovery` (+10 XP só na 1ª vez, DA-01), `POST/DELETE /api/characters/{id}/favorite`, `GET /api/users/me/favorites|discoveries|statistics`
- [x] `isFavorite`/`isDiscovered` na listagem, no detalhe do personagem e nos personagens da nação (token opcional)
- [x] Frontend: botão de favoritar otimista com rollback (cards e detalhe), selo "Descoberto", descoberta enviada uma vez por página e só depois do conteúdo exibido, toasts de descoberta/nível com o `reward` da API, página `/favorites`, perfil com exploração (x / total com barras) e descobertas recentes, atalho de favoritos na navbar
- [x] Testes: backend 139 (70 unitários + 69 de integração), incluindo **10 descobertas simultâneas do mesmo personagem → 1 XP** e **10 descobertas diferentes em paralelo → 100 XP**; frontend 79
- [x] Validado num Chrome real: 1ª visita com toast e +10 XP, reload sem toast (uma chamada por visita, mesmo em StrictMode), favoritar, nação, favoritos, perfil com 20 XP e 1/37 · 1/5

**Ajustes em relação ao planejado:**
- O `ProgressionService` entrou já nesta fase (a Fase 7 completa com XpBar, modal de nível e histórico de XP); o toast de "Novo nível" é provisório até o modal da Fase 7.
- `GET /api/users/me/statistics` saiu parcial (descobertas e favoritos); quizzes e conquistas entram com as fases deles.
- O histórico de descobertas junta personagens e nações em memória antes de paginar: são dezenas de itens por usuário, e isso evita um UNION com tipos owned. Rever se o conteúdo crescer muito.
- Favoritos ficam como ícone (estrela) na área do usuário da navbar, para não apertar a navbar em 1024px.
- Nos testes de frontend, a espera padrão do Testing Library subiu para 3s: com a suíte inteira em paralelo, o primeiro carregamento lazy de uma página podia passar de 1s.

## Fase 7 — XP e níveis (concluída)

> `ExperienceTransaction`, `ProgressionService`, `LevelCalculator` e `RewardResult` foram antecipados nas Fases 5 e 6; esta fase fechou o que faltava.

- [x] `LevelCalculator.GetProgress`: nível, XP mínimo do nível atual e do próximo e porcentagem (0–99) — a fórmula (RN-07) continua num único lugar
- [x] `UserDto` e `RewardResult` passam a trazer `currentLevelXp`, `nextLevelXp` e `levelProgress`: o frontend não conhece a fórmula
- [x] `GET /api/users/me/experience`: histórico paginado de XP (RF-XP-01)
- [x] `XpBar` (perfil completo e versão compacta na navbar) com animação de ganho; cor do elemento ou neutro claro (DA-02)
- [x] Modal de novo nível com `<dialog>` nativo (foco preso, Esc, fundo inerte), em fila quando há vários níveis; substitui o toast provisório da Fase 6 (RF-XP-03)
- [x] Perfil com "Histórico de XP" paginado
- [x] Testes: backend 158 (87 unitários + 71 de integração), incluindo a fórmula em níveis de fronteira e a consistência nível × progresso de 0 a 10.000 XP; frontend 83
- [x] Validado num Chrome real: 10 descobertas pela interface → barra da navbar sobe (50/100 na 5ª) → modal de nível 2 com foco em "Continuar" → Esc fecha → perfil "Nível 2 · 100 / 250 XP" e 10 itens no histórico

**Ajustes em relação ao planejado:**
- `GET /api/users/me` não foi criado: `GET /api/auth/me` já devolve o perfil com XP e progresso. Ele entra quando houver dados de perfil a mais (ex.: `PATCH` de avatar/elemento).
- O `<dialog>` nativo não existe no jsdom; os testes usam um polyfill mínimo em `test/setup.ts`.

## Fase 8 — Conquistas (concluída)

- [x] Entidades `Achievement` (`Code` único, `RequirementType`, `RequirementValue`, `ExperienceReward > 0`) e `UserAchievement` (PK `UserId + AchievementId`), migration `AddAchievements`
- [x] Seed em `achievements.json` (upsert por `Code`, validado no carregamento): Primeiro passo, Curioso, Explorador, Historiador, Conhecedor de Avatar e Mestre dos elementos
- [x] `AchievementRules`: um só lugar decide o desbloqueio e o progresso exibido (meta, valor atual, progresso limitado à meta)
- [x] `ProgressionService` avalia as conquistas depois de cada evento de XP: métricas lidas uma vez, antes do XP das conquistas, então esse XP não dispara nova avaliação (RN-08); cada desbloqueio grava `UserAchievement` + `ExperienceTransaction` numa transação, e a unicidade do banco impede desbloquear duas vezes (RN-05)
- [x] "Primeiro passo" no cadastro (`ProgressionService.EvaluateAchievementsAsync`), com o `reward` na resposta de `/api/auth/register`
- [x] `GET /api/achievements` (catálogo público), `GET /api/users/me/achievements` (desbloqueadas com data, bloqueadas com progresso) e `unlockedAchievements`/`totalAchievements` em `/api/users/me/statistics`
- [x] Frontend: página `/achievements` (resumo x / total, cards desbloqueados com data e bloqueados esmaecidos com barra de progresso, loading/erro/vazio), toast de conquista (um por conquista, separado do toast da descoberta), atalho com troféu na navbar, conquistas no perfil e troféu no histórico de XP
- [x] Testes: backend 179 (102 unitários + 77 de integração), incluindo **10 descobertas diferentes em paralelo → "Curioso" desbloqueado uma única vez (160 XP)**, Historiador na última nação e ausência de cascata; frontend 90
- [x] Validado num Chrome real (CDP): cadastro → toast "Primeiro passo" → 10 descobertas → toast "Curioso" junto do da descoberta → revisita sem toast → `/achievements` 2 de 6 (desktop e 390px) → perfil "Conquistas 2 / 6" e "Conquista: Curioso" no histórico

**Ajustes em relação ao planejado:**
- **Explorador** pede 30 personagens, não 50: o seed tem 37, e com 50 a conquista seria impossível. A meta volta a subir quando o conteúdo crescer (só muda o JSON).
- `ExperienceReward` é `> 0` (e não `≥ 0`): todo desbloqueio gera uma `ExperienceTransaction`, que exige `amount > 0`.
- O evento e cada conquista são gravados em transações separadas, não numa só: se algo falhar entre elas, a conquista pendente é desbloqueada no próximo evento (a avaliação sempre olha todas as conquistas ainda bloqueadas). Pelo mesmo motivo, usuários criados antes desta fase recebem o "Primeiro passo" na próxima atividade.
- Conhecedor de Avatar e Mestre dos elementos já estão no catálogo, com progresso 0 até os quizzes (Fase 9) e os desafios (Fase 11) existirem.
- `xpGained` do `reward` soma o XP do evento e o das conquistas; o toast da descoberta mostra só a parte do evento (`utils/reward.ts`) e cada conquista tem o seu toast.

## Fase 9 — Quiz (concluída)

- [x] Entidades `Quiz` (`Title` único, `Category` texto livre, `Difficulty`), `Question` (`Order` único por quiz, `Explanation`), `QuestionOption` (índice único parcial: no máximo uma correta por pergunta), `UserQuizAttempt` e `UserQuizAnswer`; migration `AddQuizTables`
- [x] Seed em `quizzes.json`: 4 quizzes × 10 perguntas (Personagens/Fácil, Dobra/Médio, História/Difícil, A Lenda de Korra/Médio), validado (≥ 2 opções e exatamente uma correta por pergunta); perguntas e opções sincronizadas pela posição, sem remover nada
- [x] `GET /api/quizzes` (público, filtros `category`/`difficulty`, melhor resultado com token), `GET /api/quizzes/{id}` (🔒, **sem `isCorrect` nem explicação**: o DTO de opção só tem `id` e `text`), `POST /api/quizzes/{id}/attempts` (correção no backend, 201) e `GET /api/users/me/quiz-history`
- [x] Validação da submissão: todas as perguntas respondidas uma única vez e cada opção pertencente à sua pergunta (400 com erro em `answers`)
- [x] XP pelo `ProgressionService.GrantAllAsync` (várias recompensas de um evento, conquistas avaliadas uma vez): +20 na 1ª conclusão e +50 no 1º gabarito de cada quiz (RN-06); toda tentativa é gravada, inclusive sem XP
- [x] Métricas de quiz nas conquistas (quizzes concluídos, perfeitos e respostas corretas): "Conhecedor de Avatar" agora pode ser desbloqueada; estatísticas com `completedQuizzes`, `totalQuizzes`, `quizAttempts` e `accuracyPercent`
- [x] Frontend: `/quizzes` (cards com categoria, dificuldade, nº de perguntas e melhor resultado; filtros na URL), `/quizzes/:id` (uma pergunta por vez com rádios nativos estilizados, voltar, enviar só com tudo respondido; resultado com pontuação, XP, conquistas e correção com explicação; jogar de novo), `/quizzes/history`, "Quizzes" na navbar, quizzes e taxa de acerto no perfil
- [x] Testes: backend 208 (124 unitários + 84 de integração), incluindo o JSON de `GET /api/quizzes/{id}` sem `isCorrect`/`explanation`, **5 primeiras tentativas simultâneas → XP uma vez** e "Conhecedor de Avatar" na 10ª tentativa perfeita; frontend 103
- [x] Validado num Chrome real (CDP): cadastro → lista → jogo 9/10 (+20) → resposta de `GET /api/quizzes/1` sem `isCorrect` → gabarito (+50) → refeito (sem XP, aviso) → histórico com as 3 tentativas → "Melhor: 10/10" → perfil "1 / 4" e "97%"; pergunta em 390px

**Ajustes em relação ao planejado:**
- "Conhecedor de Avatar" conta as respostas corretas de **todas** as tentativas, inclusive quizzes refeitos: contando só perguntas distintas, o máximo seria 40 e a meta de 100 ficaria impossível. Refazer não rende XP do quiz, só ajuda nessa conquista.
- A tentativa é gravada antes do XP (para as conquistas contarem as respostas dela), e o `ExperienceEarned` é atualizado depois; se algo falhar entre as duas gravações, a tentativa fica com 0 XP no histórico, mas o XP do usuário está correto.
- Sem paginação em `GET /api/quizzes`: o catálogo tem poucos itens. As categorias do filtro vêm do próprio catálogo.
- A navbar completa passou de 1024px para **1280px**: com "Quizzes" (cinco seções + área do usuário), entre 1024 e 1279px a barra transbordava. Abaixo de 1280px, menu recolhido.

## Fase 10 — Dashboard (concluída)

- [x] `GET /api/users/me/dashboard` (🔒): usuário com progresso do nível, estatísticas, até 3 últimas conquistas, a próxima conquista, até 5 descobertas recentes, 4 recomendações, quiz em destaque e até 4 favoritos, numa chamada
- [x] `DashboardService` reaproveita os serviços de cada área (`AchievementService`, `QuizService`, repositório do usuário); as escolhas ficam em `DashboardRules` (puras, com testes unitários):
  - **Quiz em destaque:** o primeiro não concluído (ordem do catálogo); se todos foram concluídos, o de melhor resultado ainda não gabaritado; todos gabaritados → `null`
  - **Próxima conquista:** a bloqueada com a maior fração da meta cumprida (empate: ordem do catálogo)
  - **Recomendações:** personagens ainda não descobertos, sorteados de forma estável por usuário e por dia (não mudam ao recarregar, mudam no dia seguinte; dia em UTC via `TimeProvider`)
- [x] Mapeamentos `User → UserDto` e `DiscoveryEntry → DiscoveryDto` extraídos para `UserMappings` (usados por auth, descobertas e dashboard)
- [x] Frontend: `/dashboard` com saudação + XpBar (do usuário da sessão, aparece sem esperar a API), 4 indicadores clicáveis, quiz em destaque, conquistas (recentes + próxima), "Continue explorando", descobertas recentes e favoritos; cada seção com estado vazio próprio, skeleton no carregamento e erro com "Tentar novamente"
- [x] `/dashboard` é o destino padrão após login e cadastro; na navbar, "Início" vira "Painel" para quem está logado (a Home continua no logo)
- [x] O XP/nível do dashboard atualiza o usuário em memória (caso tenha mudado em outra aba/dispositivo); favoritar, descobrir ou jogar invalida o dashboard junto com as demais consultas `['me', ...]`
- [x] Testes: backend 220 (133 unitários + 87 de integração), frontend 109
- [x] Validado num Chrome real (CDP): cadastro → `/dashboard` com 4 recomendações e "O Time Avatar" em destaque → abrir um recomendado (+10 XP) → painel com a descoberta, sem o personagem nas recomendações, "1 / 37" e 20 XP → login leva ao painel; 390px sem rolagem horizontal; console sem erros

**Ajustes em relação ao planejado:**
- Desafios do dia ficam para a Fase 11 (o agregado ganhará o campo junto com a funcionalidade).
- `nextAchievement` não estava no plano: mostra uma meta concreta mesmo para quem só tem o "Primeiro passo".
- As recomendações carregam os não descobertos e sorteiam em memória: são no máximo 37 personagens. Rever se o conteúdo crescer muito.
- Nos estados vazios do dashboard, mensagens curtas no lugar do `EmptyState` grande, para o painel não ficar dominado por caixas vazias.

## Fase 11 — Desafios diários (concluída)

- [x] Entidades `ChallengeTemplate` (`Code` único, tipo, meta, XP, `IsActive`), `DailyChallenge` (UNIQUE `ChallengeTemplateId` + `Date`) e `UserDailyChallenge` (PK `UserId` + `DailyChallengeId`); migration `AddDailyChallenges`
- [x] Seed em `challenges.json`: 8 modelos (descobrir 3/5 personagens, explorar 1/2 nações, completar 1/2 quizzes diferentes, acertar 80%, gabaritar), validados (meta > 0, XP > 0, porcentagem ≤ 100, ao menos um ativo) e sincronizados por `Code`
- [x] `ChallengeRules` (puro): sorteio do dia determinístico pela data (prefere tipos diferentes), progresso, conclusão e o intervalo do dia em UTC
- [x] `DailyChallengeService`: geração preguiçosa de 3 desafios na primeira consulta do dia (D8, sem job), progresso calculado pelas atividades do dia, histórico
- [x] `ProgressionService` (passo 4 da arquitetura): depois do XP do evento, conclui os desafios cumpridos (`UserDailyChallenge` + `ExperienceTransaction` `ChallengeCompleted`) e só então avalia as conquistas, então **"Mestre dos elementos" pode desbloquear no mesmo evento**; `reward.challengesCompleted` passa a vir preenchido
- [x] `GET /api/challenges/daily`, `GET /api/users/me/challenges` e `dailyChallenges` no dashboard; sem endpoint de conclusão (D9)
- [x] Frontend: `/challenges` (desafios de hoje com progresso, "x de 3 concluídos", horário do próximo reset no fuso local, histórico paginado), seção "Desafios de hoje" no painel, toast "Desafio concluído" a partir de qualquer `reward`, atalho (alvo) na área do usuário da navbar
- [x] Testes: backend 244 (153 unitários + 91 de integração), incluindo **virada do dia com `TimeProvider`**, desafio e "Mestre dos elementos" no mesmo evento, **descobertas em paralelo concluindo cada desafio uma única vez** e **5 gerações simultâneas do mesmo dia → 3 desafios**; frontend 115
- [x] Validado num Chrome real (CDP): cadastro → painel com os 3 desafios → abrir 5 personagens e 2 nações → toasts "Desafio concluído — Descubra 3 personagens! +30 XP" e "Explore 2 nações! +40 XP" → `/challenges` com "2 de 3 concluídos" e histórico; navbar sem transbordo de 1280 a 1920px (nome de 30 caracteres); 390px sem rolagem horizontal; console sem erros

**Ajustes em relação ao planejado:**
- **Progresso calculado, não armazenado:** `UserDailyChallenge` guarda só a conclusão (sem `Progress`/`IsCompleted`). O progresso vem das descobertas e tentativas do dia (UTC), como nas conquistas: não há contador para dessincronizar com requisições simultâneas, e a PK garante XP uma única vez.
- `ChallengeTemplate` ganhou `Code` (chave estável do seed, como em `Achievement`).
- "Completar quiz" conta quizzes **diferentes** concluídos no dia, inclusive refeitos; "acertar X%" usa a melhor tentativa do dia. Quizzes sempre podem ser refeitos, então esses desafios são sempre possíveis.
- Bug evitado: o `experienceEarned` da tentativa de quiz descontava só o XP das conquistas; agora também desconta o dos desafios concluídos no mesmo evento.
- **Limitação conhecida:** os desafios do dia são os mesmos para todos. Quem já descobriu todas as nações (ou todos os personagens) não consegue cumprir os desafios de descoberta. Rever quando o conteúdo crescer (Fase 15) ou com desafios por usuário.
- Navbar: entre 1280 e 1439px os espaçamentos ficaram mais justos para caber o quarto atalho da área do usuário.

## Fase 12 — Timeline (concluída)

- [x] Entidades `Era` (`Name` e `Order` únicos) e `TimelineEvent` (`Title` e `Order` únicos, `DateLabel`, imagem owned, `EraId`) + N:N `timeline_event_characters`; migration `AddTimeline`
- [x] Seed em `eras.json` (4 eras) e `timeline.json` (17 eventos em ordem cronológica, só com personagens do seed). A ordem é a posição no arquivo; o seed valida eras e personagens inexistentes, personagem repetido e evento fora da ordem das eras, e grava valores temporários antes de reordenar (sem violar o `Order` único)
- [x] `GET /api/timeline` (eventos agrupados por era, filtros `eraId`/`characterId` combináveis, 400 para id inválido, lista vazia para filtro sem resultado) e `GET /api/eras` (com `eventCount`)
- [x] Frontend: `/timeline` no lugar do placeholder: eras com título em Cinzel, cards de evento numa linha vertical (à esquerda no celular, alternando lados a partir de 1024px), data, descrição e personagens com avatar e link; filtros de era e personagem na URL (opções de personagem a partir da timeline completa); entrada suave ao rolar (`useReveal`, respeita `prefers-reduced-motion`); loading, erro e estado vazio com "Limpar filtros"
- [x] Testes: backend 254 (159 unitários + 95 de integração), incluindo a validação do seed da timeline e a **reordenação de eventos pelo seeder**; frontend 121
- [x] Validado num Chrome real (CDP), como visitante: 4 eras e 17 eventos, todos visíveis depois de rolar; filtro "Zuko" → 6 eventos em 2 eras com `?characterId=` na URL; link do personagem abre o detalhe; 390px sem rolagem horizontal; console sem erros

**Ajustes em relação ao planejado:**
- **Linha vertical também em telas largas** (com cards alternando dos dois lados), em vez de horizontal: os eventos têm texto e personagens demais para uma faixa horizontal legível.
- `GET /api/timeline` já devolve os eventos agrupados por era (`TimelineEraDto[]`), em vez de uma lista plana: o frontend não precisa montar os grupos.
- Datas em AG/BG (anos depois/antes do genocídio dos Nômades do Ar, convenção dos fãs), explicadas no topo da página. Onde a data exata não é bem estabelecida, o rótulo é descritivo ("Era de Kyoshi", "Pós-guerra").
- Imagens dos eventos ficam para a curadoria da Fase 15 (o campo já existe).
- **Correção nos testes da Fase 11:** o sorteio dos desafios depende da data, e em alguns dias somava XP de desafio a testes de outras áreas que conferem XP exato. A fixture principal da API agora desativa os modelos de desafio, e os testes de desafios rodam numa fixture própria (outro banco) com eles ativos.

## Fase 13 — Comparação (concluída)

- [x] Entidades `ComparisonAttribute` (`Name` único, `Order` de exibição) e `CharacterAttributeValue` (PK `CharacterId` + `ComparisonAttributeId`, `CHECK` 0–100); migration `AddCharacterComparison`
- [x] Seed: `comparison-attributes.json` (5 atributos: Domínio elemental, Combate, Agilidade, Estratégia, Liderança) e `character-attributes.json` (valores para os 37 personagens). O seed valida personagem e atributo inexistentes, duplicados, faixa 0–100, atributo faltando e o atributo marcado com `requiresBending` (0 para quem não dobra, maior que 0 para dobradores)
- [x] `GET /api/characters/compare?ids=1,2` (público): os dois personagens na ordem pedida (elemento, nação, tipo, situação, primeira aparição, séries, subtipos e habilidades) e, por atributo, os dois valores e o `leaderId` (maior valor; `null` em empate). 400 para ids ausentes, inválidos, diferente de dois ou repetidos; 404 para personagem inexistente
- [x] Frontend: `/compare?a=1&b=2` com dois seletores com busca (debounce, 6 resultados, sem o personagem do outro lado), "Trocar" e "Inverter lados", cards frente a frente, radar SVG + barras pareadas com o valor escrito e coroa no maior, tabela de informações lado a lado e o aviso fixo de não canonicidade (RN-11). Entradas: botão "Comparar" no detalhe do personagem (já preenche o primeiro lado) e chamada na Home. Loading, erro com "Tentar novamente", estado inicial e personagem inexistente tratados
- [x] Testes: backend 274 (175 unitários + 99 de integração), incluindo a validação do seed, o seeder (reordenar atributos, mudar valor, rodar duas vezes sem reescrever linhas) e o `CHECK` do banco; frontend 126
- [x] Validado num Chrome real (CDP), como visitante: detalhe do Zuko → "Comparar" → `?a=` preenchido → busca "Sok" (sem o Zuko nos resultados) → comparação Zuko × Sokka com radar, barras e tabela; "Inverter lados" troca a URL; Zuko × Azula (mesmo elemento) com cores distintas; 390px sem rolagem horizontal; console sem erros

**Ajustes em relação ao planejado:**
- **Cores pela posição, não pelo elemento:** o primeiro personagem é sempre Água e o segundo sempre Fogo (validados contra a superfície escura, inclusive para daltonismo). Pela cor do elemento, Zuko × Azula ficariam iguais. As cores só aparecem nas marcas; textos e valores ficam nas cores de texto (DA-02).
- **Radar e barras juntos:** o radar dá a forma geral; as barras trazem os números exatos e são a leitura acessível (o radar é uma imagem com descrição e dica por ponto).
- O "maior valor" vem da API (`leaderId`), não é calculado no componente.
- A comparação **não** entrou na navbar: com uma sexta seção, a barra não cabe a partir de 1280px. O acesso é pelo detalhe do personagem e pela Home.
- Os atributos são estimativas do projeto; o aviso aparece sempre, antes do resultado.
- **Limitação conhecida:** a 390px, palavras muito longas na tabela (ex.: "Redirecionamento") quebram no meio, porque o Chrome no Windows não hifeniza português. Não gera rolagem horizontal.

## Fase 14 — Testes (concluída)

- [x] **Cobertura medida** antes de escrever testes: backend ~96% das linhas (unit + integração, sem código gerado e migrations) e frontend 87% (`npm run test:coverage`, novo). Os testes novos foram para as lacunas reais:
  - backend: cada tipo de conquista lê a própria métrica; **cadastros simultâneos com o mesmo email** criam uma conta só (os demais recebem 409, decidido pela unique do banco); `Jwt:Secret` curto falha com mensagem útil
  - frontend: página de nações (lista, singular/plural, vazio, erro), Home (Avatares, elementos, convite só para visitante, erro isolado por seção), filtros de elemento/tipo/status/série combinados na URL e na consulta, erro nos favoritos
- [x] **E2E com Cypress** (`frontend/avatar-codex-web/cypress`), os 9 fluxos em 3 specs:
  - `auth.cy.ts`: cria conta pelo formulário (painel + "Primeiro passo"); entra, mantém a sessão ao recarregar e sai; senha errada não entra
  - `explore.cy.ts`: pesquisa, abre o personagem, ganha +10 XP pela descoberta e favorita (aparece em `/favorites`); segunda visita não dá XP
  - `progress.cy.ts`: faz um quiz (correção no backend, XP na 1ª conclusão, histórico); descobre as 5 nações, desbloqueia "Historiador" e confere em `/achievements` e no perfil
- [x] Testes: backend 285 (185 unitários + 100 de integração, 3 vezes seguidas), frontend 136, E2E 7 testes cobrindo os 9 fluxos (3 vezes seguidas, todos verdes)

**Encontrado pelos testes:**
- **Bug corrigido:** quando uma descoberta também concluía um desafio do dia, o toast da descoberta mostrava o XP do desafio junto (ex.: "+40 XP" em vez de "+10 XP"), além do toast próprio do desafio. `eventXp` agora desconta conquistas **e** desafios (o mesmo cuidado que o backend já tinha no `QuizService`).
- **Simplificação:** depois do login/cadastro, o formulário e o `GuestOnly` navegavam os dois para o mesmo destino; agora só o `GuestOnly` redireciona.
- **E2E no Chrome, não no Electron:** no Electron (navegador padrão do Cypress, marcado como depreciado) a troca de página depois do login às vezes não acontecia, mesmo com a API respondendo em menos de 100ms. No Chrome, 10 de 10 execuções seguidas passaram (e 15 de 15 num teste à parte via CDP). `npm run e2e` usa `--browser chrome`.
- **E2E contra o build:** `vite preview` na porta 5173 (a mesma origem liberada no CORS), mais próximo de produção e sem a otimização de dependências do servidor de desenvolvimento.

**Ajustes em relação ao planejado:**
- O E2E não usa `data-testid`: as consultas são por papel e nome acessível (Testing Library), como nos testes de frontend. Assim o E2E também confere a acessibilidade básica.
- "Ganha XP" e "consulta o progresso" não conferem um total exato: os desafios do dia (sorteados pela data) podem somar XP ao mesmo evento. O E2E confere o XP do evento no toast e um mínimo no perfil.

## Fase 15 — Refinamento visual (concluída)

- [x] **Auditoria primeiro:** capturas de todas as telas a 390px e 1440px (logado como demo) num Chrome real via CDP. A base já tinha skeletons, estados vazio/erro, foco visível, skip link, `prefers-reduced-motion`, hover e entrada dos cards; o trabalho foi corrigir o que as capturas mostraram.
- [x] **Placeholders melhores (DA-04):** o `CharacterAvatar` recebe o elemento (cor + ícone) em vez de só a cor e mostra as iniciais sobre o símbolo do elemento em marca d'água, com anel interno e brilho na cor do elemento. Não dobradores ficam só com as iniciais. No tamanho pequeno (40px) o símbolo é omitido.
- [x] **Pronto para imagens reais:** o seed de personagens aceita `image` (`url`, `sourceUrl`, `credit`), gravado no `ImageInfo` (atualizado no lugar, sem reescrever a linha a cada subida). Com imagem, ela cobre as iniciais; se o link falhar (`onError`), as iniciais voltam.
- [x] **Layouts:** os 5 Avatares da Home numa linha a partir de 1024px; nações em flex centralizado (3 + 2 no desktop, sem card órfão); detalhe do personagem em duas colunas a partir de 1024px (história e habilidades | subtipos, relações e aparições); estatísticas do perfil em 2 colunas no celular e 3 + 3 no desktop; cards do "vs" da comparação com a mesma altura.
- [x] **Microinterações:** transição discreta entre páginas (fade + 6px, 250ms, só quando muda o pathname, não nos filtros) e "pop" da estrela ao favoritar (só no clique, não ao carregar). Ambas zeradas pelo `prefers-reduced-motion`.
- [x] Testes: backend 285 (185 unitários + 100 de integração), frontend 139 (novo `CharacterAvatar.test.tsx`: iniciais + símbolo, sem símbolo para quem não tem elemento, volta para as iniciais quando a imagem falha), E2E 7 testes / 9 fluxos verdes. Lint, format e build passando.
- [x] Validado num Chrome real (CDP): capturas de antes e depois a 390px e 1440px; favoritar dispara a animação e desfavoritar volta ao estado anterior; navegação pela navbar dispara a transição.

**Ajustes em relação ao planejado:**
- **Imagens reais ficam pendentes.** A ideia era usar as imagens da Avatar Wiki (Fandom), mas a Fandom responde com `Cross-Origin-Resource-Policy: same-origin`, e o navegador bloqueia o hotlink em qualquer site. A decisão do usuário foi melhorar os placeholders agora e adicionar as imagens depois: basta preencher `image` em `characters.json`, sem mudança de código.
- Os toasts já ficavam no rodapé da tela; nas capturas de página inteira eles apareciam no meio por causa do `position: fixed`.

**Pendências:**
- Imagens reais dos personagens (o usuário vai fornecer). Imagens dos eventos da timeline continuam sem curadoria.

## Fase 16 — Documentação final (concluída)

- [x] **Auditoria primeiro:** cada documento comparado com o código (rotas dos controllers, pastas, migrations, configurações, testes). Encontrado: README parado na Fase 9; `api.md` com `GET`/`PATCH /api/users/me`, que nunca existiram; `architecture.md` com pastas que não existem (`Api/Filters`, `Infrastructure/Services`), `LevelCalculator` no lugar errado (Domain em vez de Application) e o fluxo de progressão descrito como "uma única transação" (são transações separadas por recompensa); `database.md` sem as migrations `AddAchievements` e `AddQuizTables`.
- [x] **README** reescrito para quem nunca viu o projeto: funcionalidades, pré-requisitos com versões, passo a passo com Docker **ou** PostgreSQL local, user-secrets (com as diferenças do PowerShell), usuário demo, variáveis de ambiente, tabela de problemas comuns e mapa da documentação.
- [x] **Novo `docs/testing.md`:** as quatro suítes (quantidade, onde ficam, do que precisam), o que cada pasta cobre, fixtures (por que os desafios ficam desligados na fixture principal), E2E passo a passo e o porquê das escolhas (Chrome, build, sem `data-testid`).
- [x] **`architecture.md`:** estrutura de pastas real, §3.3 reescrita como descrição do fluxo final (`GrantAsync`/`GrantAllAsync`/`EvaluateAchievementsAsync`, ordem desafios → conquistas, gravação idempotente), DA-04 na tabela de decisões, Docker opcional, risco de imagens atualizado.
- [x] **`api.md`:** rotas inexistentes removidas, `/api/health` documentado, exemplo com `image`, `isFavorite` e `isDiscovered`.
- [x] **`database.md`:** migrations completas e seção de imagens (como adicionar um retrato).
- [x] **`requirements.md`:** nova §9 "Status de implementação" com o que ficou parcial (imagens de 12/37, sem página de elementos nem de habilidades, sem edição de perfil).
- [x] **Marcas de andamento** ("✅ Fase N") retiradas de `api.md`, `database.md` e `ui-ux.md`: são documentos de referência; o histórico fica neste arquivo.
- [x] **Critério de aceite verificado:** clone limpo do repositório, seguindo só o README, com um banco que ainda não existia. A API sem `Jwt:Secret` falha com a mensagem que o README descreve; com as configurações, cria o banco, aplica migrations e seed, o health check responde `Healthy`, o usuário demo entra, e o frontend (`npm install` + `npm run dev`) lista os 37 personagens com os retratos. Banco e clone removidos depois.

**Pendências:** imagens de 25 personagens (sem mudança de código, ver `database.md` §4). A próxima é a Fase 17 (deploy).
