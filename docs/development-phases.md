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
| 7 | XP e níveis | P1 | ExperienceTransaction, ProgressionService, LevelCalculator, XpBar, notificações de nível, `RewardResult` | Regras centralizadas no backend, com testes unitários |
| 8 | Conquistas | P1 | Achievement, UserAchievement, regras, desbloqueio, página de conquistas, notificações | Conquistas desbloqueiam automaticamente e uma única vez |
| 9 | Quiz | P1 | Quizzes, perguntas, opções, submissão, validação, resultado, XP, histórico; UI atraente | `isCorrect` nunca exposto antes da submissão; testes cobrindo isso |
| 10 | Dashboard | P1/P2 | Endpoint agregado + página: XP, nível, progresso, conquistas, desafios, descobertas, quiz recomendado, favoritos | Destino padrão após login |
| 11 | Desafios diários | P2 | ChallengeTemplate, geração diária, progresso, conclusão automática, recompensa, histórico | Testes com `TimeProvider` simulando dias |
| 12 | Timeline | P2 | Era, TimelineEvent, personagens relacionados, navegação visual, filtros | — |
| 13 | Comparação | P2 | Atributos, seleção de dois personagens, visualização (barras/radar), aviso de não-canonicidade | — |
| 14 | Testes | — | Completar unit, integração, frontend e E2E (ver abaixo) | Regras de negócio cobertas; E2E verde |
| 15 | Refinamento visual | — | Espaçamento, tipografia, responsividade, acessibilidade, animações, loading/empty/error, microinterações | Aparência de produto final |
| 16 | Documentação final | — | README, arquitetura, banco, API, instalação, testes, decisões técnicas | Outro dev executa o projeto só com a doc |
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
- Destino padrão depois do login é `/profile`, até o Dashboard existir (Fase 10).
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
