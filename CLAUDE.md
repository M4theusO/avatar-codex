# CLAUDE.md — Avatar Codex

Guia para o Claude Code trabalhar neste repositório. Este arquivo é o **resumo**; os detalhes estão em `docs/`.

## Visão geral

Aplicação web fullstack inspirada em *Avatar: The Last Airbender* e *The Legend of Korra*.
Não é um CRUD de personagens: é uma **enciclopédia interativa + plataforma de exploração + sistema de evolução do usuário** (XP, níveis, conquistas, quizzes, desafios, favoritos).

Projeto de **portfólio** (fins educacionais). Deve parecer um produto real, ter arquitetura justificável, ser testável e fácil de evoluir.

**Prioridades, em ordem:** 1. experiência visual · 2. organização · 3. arquitetura · 4. funcionalidade ponta a ponta · 5. testabilidade · 6. documentação.

## Status atual

**Fase 17 — Deploy** (próxima, ainda não iniciada). Fases 0 a 16 concluídas (`docs/development-phases.md`).

**Onde paramos (2026-10-30):** Fase 16 concluída (documentação revisada contra o código + novo `docs/testing.md` + teste num clone limpo seguindo só o README), commitada e publicada no GitHub (`main` = `origin/main`).
- **Imagens dos personagens (2026-10-30):** 12 de 37 com imagem real (Aang, Bolin, Iroh, Jinora, Korra, Lin Beifong, Mako, Ozai, Pakku, Sokka, Toph Beifong, Zuko). Ficam em `frontend/avatar-codex-web/public/images/characters/<nome-em-kebab>.jpg`, recortadas em retrato quadrado 320×320 (cabeça e ombros); os originais de corpo inteiro estão fora do repo, em `C:\Users\Matheus\Pictures\avatar-codex-originais`. Cada uma é referenciada em `characters.json` (`"image": { "url": "/images/characters/...", "sourceUrl": null, "credit": "Nickelodeon" }`) e o seeder sincroniza ao subir a API. Os outros 25 continuam com o placeholder (DA-04). A Avatar Wiki (Fandom) bloqueia hotlink, então as imagens precisam ser servidas pelo próprio projeto.
- **Conteúdo:** entidades, seed (37 personagens, 5 nações, 4 elementos, 6 conquistas, 4 quizzes × 10 perguntas), API com busca/filtros/paginação, tratamento global de erros.
- **Frontend base:** layout, navbar (completa a partir de 1280px), rotas lazy, Home, Characters (estado na URL), CharacterDetails, Nations, NationDetails, 404.
- **Autenticação:** cadastro, login, logout, JWT (15 min, em memória) + refresh rotacionado em cookie httpOnly, sessão recuperada ao recarregar, rotas protegidas, usuário demo (README).
- **Favoritos e descoberta:** favoritar otimista, descoberta de personagem/nação com +10 XP idempotente (testado com concorrência), `/favorites`, perfil com exploração.
- **XP e níveis:** `ProgressionService` (`GrantAsync` para um evento simples, `GrantAllAsync` para várias recompensas) + `ExperienceRewards` + `LevelCalculator` (fórmula só no backend), XpBar, modal de novo nível, histórico de XP.
- **Conquistas:** `AchievementRules` (desbloqueio e progresso num só lugar), avaliação após cada evento sem cascata (RN-08), desbloqueio único pelo banco (RN-05), "Primeiro passo" no cadastro, `/achievements`, toasts. "Mestre dos elementos" conta os desafios concluídos.
- **Quiz:** `QuizService` (correção no backend, validação das respostas), `GET /api/quizzes/{id}` sem `isCorrect`/explicação, XP só na 1ª conclusão (+20) e no 1º gabarito (+50), toda tentativa no histórico, `/quizzes`, `/quizzes/:id` (uma pergunta por vez + resultado com correção), `/quizzes/history`, quizzes e taxa de acerto no perfil.
- **Dashboard:** `GET /api/users/me/dashboard` (agregado; escolhas em `DashboardRules`: quiz em destaque, próxima conquista, recomendações com rotação diária), `/dashboard` como destino após login/cadastro, "Painel" na navbar para quem está logado.
- **Desafios diários:** `ChallengeRules` (sorteio determinístico pela data, progresso, conclusão) + `DailyChallengeService` (geração preguiçosa de 3 por dia em UTC, progresso calculado pelas atividades do dia); conclusão automática no `ProgressionService` antes das conquistas, PK garante XP único; `/challenges`, seção no painel, toast "Desafio concluído".
- **Timeline:** `Era` + `TimelineEvent` (seed: 4 eras, 17 eventos), `GET /api/timeline` agrupado por era com filtros `eraId`/`characterId`, `GET /api/eras`, `/timeline` com linha vertical (alterna lados ≥ 1024px), filtros na URL e entrada ao rolar.
- **Comparação:** `ComparisonAttribute` + `CharacterAttributeValue` (seed: 5 atributos × 37 personagens, CHECK 0–100), `ComparisonService` (ordem pedida, `leaderId` decidido no backend), `GET /api/characters/compare?ids=1,2`, `/compare?a=&b=` com seletores com busca, radar + barras (cores pela posição: 1º Água, 2º Fogo), tabela lado a lado e aviso fixo de não canonicidade (RN-11). Entradas pelo detalhe do personagem ("Comparar") e pela Home.
- **Testes de integração:** a fixture principal da API desativa os desafios diários (o sorteio depende da data); os testes de desafios usam `ChallengesApiFixture`, com outro banco.
- **Refinamento visual:** `CharacterAvatar` recebe o elemento e mostra iniciais sobre o símbolo do elemento (fallback para as iniciais se a imagem falhar); 5 Avatares numa linha na Home, nações 3 + 2 centralizadas, detalhe do personagem em 2 colunas (≥ 1024px), estatísticas do perfil em 2 colunas no celular, "vs" alinhado; transição entre páginas e "pop" ao favoritar.
- **Testes:** backend 285 (185 unitários + 100 integração), frontend 139 (`npm run test:coverage` mede a cobertura), E2E Cypress com os 9 fluxos (7 testes, no Chrome, contra o build + banco `avatar_codex_e2e`; passo a passo em `docs/testing.md`). Lint, format e build passando.
- **Line endings:** neste PC `core.autocrlf=true`; o Prettier reescreve os arquivos em LF e o `git status` lista todos como modificados, mas sem diferença de conteúdo (`git diff --ignore-cr-at-eol`). Commitar só os arquivos alterados de verdade.

**Próximo passo — Fase 17 (Deploy):** frontend (Vercel/Netlify), backend (Render/Azure/Railway) e PostgreSQL gerenciado, sem serviços pagos. Pontos já mapeados: cookie de refresh precisa de `SameSite=None` com domínios diferentes (`AuthController`), migrations fora do startup (`Database:MigrateAndSeedOnStartup` só em Development), CORS com a origem de produção, cold start do plano gratuito.
- **Não iniciar uma nova fase sem confirmação explícita do usuário.**

**Fluxo de trabalho combinado:** ao fim de cada fase → testes + lint + build + teste num Chrome real (CDP) → docs → commits pequenos → **push só quando o usuário pedir**. O usuário costuma pedir "rode o projeto para eu testar" (subir API + Vite em background) e depois "pare de rodar".

**Ambiente local:** sem Docker; PostgreSQL 18 local na porta 5432, banco `avatar_codex`, connection string e `Jwt:Secret` nos user-secrets do `AvatarCodex.Api`. Testes de banco precisam de `AVATARCODEX_TEST_DB` (connection string do servidor, sem `Database`).
- O usuário alterna entre **dois PCs**. Em um PC novo: instalar PostgreSQL, configurar os user-secrets (`ConnectionStrings:Default` e `Jwt:Secret`), `dotnet tool install -g dotnet-ef`, `npm install` e copiar `frontend/avatar-codex-web/.env.example` para `.env.local` (ver README). No PC atual tudo isso já está feito; a variável `AVATARCODEX_TEST_DB` não é persistente (definir na sessão ao rodar os testes).

## Decisões aprovadas

- **DA-01** — Descoberta de nação: +10 XP só na 1ª vez por usuário, registrada depois que o conteúdo principal foi exibido, com idempotência garantida no backend.
- **DA-02** — Paleta dark: fundo `#101820`, superfície `#1B2835`; acentos Água `#3186B5`, Fogo `#B86B35`, Terra `#638B58`, Ar `#D5C7A2`. Elementos são acentos, nunca dominantes; Água/Fogo/Terra não podem ser usadas como texto pequeno (contraste).
- **DA-03** — Tipografia: Cinzel em títulos e destaques narrativos (com moderação); Inter em todo o resto.
- **DA-04** — Imagens de personagens: retratos quadrados servidos pelo próprio app (`public/images/characters`); placeholder próprio (iniciais + símbolo e cor do elemento) para quem não tem imagem e como fallback.

## Stack

| Camada | Tecnologias |
|---|---|
| Frontend | React 19 · TypeScript · Vite · React Router · Axios · TanStack Query · CSS Modules + design tokens · lucide-react · Cinzel + Inter · oxlint + Prettier |
| Backend | .NET 10 · ASP.NET Core Web API · EF Core · C# · JWT · FluentValidation |
| Banco | PostgreSQL (migrations via EF Core) |
| Testes | xUnit (unit + integração) · Vitest + React Testing Library · Cypress (E2E) |
| Infra | Docker Compose · Git/GitHub |

TypeScript sempre — nunca JavaScript puro quando TS for possível.

## Arquitetura resumida

```text
frontend (React SPA) ──HTTP/JSON + JWT──► AvatarCodex.Api ──► Application ──► Domain
                                                 │                  ▲
                                                 └──► Infrastructure ┘ (EF Core, PostgreSQL)
```

- **Api**: controllers finos (recebem, validam, chamam serviço, retornam HTTP), middleware de erros, auth.
- **Application**: casos de uso, DTOs, validators, interfaces. **Toda regra de negócio fica aqui.**
- **Domain**: entidades, enums, exceções de domínio. Sem dependências externas.
- **Infrastructure**: DbContext, configurações EF, repositórios, migrations, seed, serviços técnicos (JWT, hash).
- **Gamificação** centralizada em um único serviço de progressão no backend (XP → nível → conquistas → desafios).

Detalhes: `docs/architecture.md`.

## Regras de desenvolvimento (obrigatórias)

1. **Uma fase por vez.** Após cada fase: executar → testar → verificar erros → revisar arquitetura → documentar → só então avançar.
2. **Sem regra de negócio em Controllers** nem em **componentes React**.
3. **Não confiar no cliente.** XP, níveis, conquistas, desafios e correção de quiz são decididos apenas pelo backend. Nunca existir endpoint do tipo `POST /xp { amount }`.
4. **Nunca retornar `IsCorrect`** das opções de quiz antes da submissão.
5. **Não duplicar lógica** entre frontend e backend.
6. **Conteúdo no banco** (personagens, habilidades, eventos, quizzes, conquistas). Nada hardcoded em componentes.
7. **Sem secrets no Git.** Usar `.env` (ignorado) + `.env.example` versionado.
8. **Senhas apenas como hash seguro.** Nunca expor dados sensíveis na API nem stack traces.
9. **Evitar overengineering**: arquitetura profissional, mas proporcional. Na dúvida, escolher a solução mais simples, sustentável, testável e fácil de explicar em uma entrevista.
10. **Componentes pequenos e reutilizáveis.** Nada de componentes gigantes.
11. Toda tela trata **loading, empty state, error state e feedback de sucesso**, é **responsiva (mobile-first)** e **acessível**.

## Definition of Done

Uma funcionalidade só está concluída com: backend + frontend + validações + loading + error state + responsividade + testes relevantes + documentação atualizada.

## Convenções

- **Commits**: Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `refactor:`, `chore:`), pequenos e focados em um assunto.
- **Idioma**: código, nomes de tabelas e endpoints em inglês; documentação e textos da UI em português.
- **API**: REST, recursos no plural, códigos HTTP corretos, formato de erro padronizado (ver `docs/api.md`).

## Comandos

```bash
# Banco (Docker)
docker compose up -d

# Backend (http://localhost:5080, Swagger em /swagger)
cd backend && dotnet build && dotnet test
dotnet run --project src/AvatarCodex.Api --launch-profile http

# Migrations (o startup em Development aplica migrations + seed automaticamente)
dotnet ef migrations add <Nome> -p src/AvatarCodex.Infrastructure -s src/AvatarCodex.Api

# Testes de banco (sem a variável, são ignorados)
AVATARCODEX_TEST_DB="Host=localhost;Port=5432;Username=postgres;Password=..." dotnet test

# Frontend (http://localhost:5173)
cd frontend/avatar-codex-web
npm run dev | npm run test | npm run lint | npm run format:check | npm run build

# E2E (Chrome): API com Database=avatar_codex_e2e + `npm run build && npm run preview` → `npm run e2e`
```

Connection string via `dotnet user-secrets` no projeto `AvatarCodex.Api` (ver README).

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/requirements.md](docs/requirements.md) | Requisitos funcionais, regras de negócio, requisitos não funcionais, status de implementação |
| [docs/architecture.md](docs/architecture.md) | Arquitetura, estrutura de pastas, fluxos, decisões técnicas, riscos |
| [docs/database.md](docs/database.md) | Entidades, relacionamentos, constraints, diagrama ER, seed |
| [docs/api.md](docs/api.md) | Endpoints, payloads, autenticação, erros, paginação |
| [docs/ui-ux.md](docs/ui-ux.md) | Identidade visual, rotas e páginas, componentes, estados, acessibilidade |
| [docs/testing.md](docs/testing.md) | Suítes de teste, como rodar, fixtures, E2E passo a passo |
| [docs/development-phases.md](docs/development-phases.md) | Fases 0–17, prioridades P0–P3, checklist da Fase 1 |
