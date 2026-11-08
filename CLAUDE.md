# CLAUDE.md — Avatar Codex

Guia para o Claude Code trabalhar neste repositório. Este arquivo é o **resumo**; os detalhes estão em `docs/`.

## Visão geral

Aplicação web fullstack inspirada em *Avatar: The Last Airbender* e *The Legend of Korra*.
Não é um CRUD de personagens: é uma **enciclopédia interativa + plataforma de exploração + sistema de evolução do usuário** (XP, níveis, conquistas, quizzes, desafios, favoritos).

Projeto de **portfólio** (fins educacionais). Deve parecer um produto real, ter arquitetura justificável, ser testável e fácil de evoluir.

**Prioridades, em ordem:** 1. experiência visual · 2. organização · 3. arquitetura · 4. funcionalidade ponta a ponta · 5. testabilidade · 6. documentação.

## Status atual

**Todas as fases (0–22) concluídas.** Aplicação no ar em **https://avatar-codex.vercel.app** (`docs/development-phases.md`, `docs/deployment.md`).

**Onde paramos (2026-11-08):** Fases 18–21 publicadas (push feito). **Fase 22** (editar nome de usuário e foto de perfil), dicas nos ícones da navbar, correção da largura da navbar e favicon dos elementos: tudo publicado (push em 2026-11-08; o startup do Render aplica a migration `AddUserAvatars` no Neon).
- **Descoberta de habilidades (2026-11-08, ainda sem push):** abrir `/abilities/:id` logado dá +10 XP na 1ª vez (`POST /api/abilities/{id}/discovery`, `UserAbilityDiscovery`, migration `AddAbilityDiscoveries`, `ExperienceType.AbilityDiscovered`); aparece nas descobertas recentes, no histórico de XP, nas estatísticas do perfil (6 indicadores; o card de favoritos saiu, o acesso é pelo atalho da navbar) e no painel (5 indicadores). Descobertas recentes ficam **só no painel** (saíram do perfil). Conquistas/desafios de habilidades ficaram de fora.
- **Fase 22:** `PATCH /api/users/me` (nome), `PUT`/`DELETE /api/users/me/avatar` (foto), `GET /api/avatars/{publicId}`; tabela `user_avatars` com PK = `UserId` (uma foto por usuário, sobrescrita a cada envio; migration `AddUserAvatars`); o navegador reduz a 256×256 e o backend confere tipo pelos bytes e 512 KB. Conta demo não muda nome nem foto. Para editar, usar `IUserRepository.GetForUpdateAsync` (o `GetByIdAsync` é sem rastreamento).
- **Feito em 2026-11-08:** (1) retratos dos 25 personagens que faltavam (37/37), com push; (2) **Fase 18** página de habilidades (`/abilities`, `/abilities/:id`, `GET /api/abilities/{id}`); (3) **Fase 19** excluir conta (`DELETE /api/users/me` com senha, conta demo protegida) e **E2E apagando os próprios usuários** no `afterEach`; (4) **Fase 20** teste de elemento (`/element-test`, perguntas no banco, `ElementTestRules`, +30 XP na 1ª vez, migration `AddElementTest`); (5) **Fase 21** ranking (`/ranking`, `GET /api/ranking`, geral/mês/elemento, só logados, empate divide a posição). Detalhes em `docs/development-phases.md`.
- **Deploy das imagens (push de 2026-11-08):** a Vercel publicou; o Render **não tinha aplicado** a mudança 25 min depois (API em produção ainda com 12 imagens). O próximo push (que inclui `backend/**`) deve redeployar; se não, conferir o painel do Render (é do usuário).
- **Navbar:** a barra tem a largura máxima do conteúdo (1200px), então não ganha espaço em telas maiores. Com 6 seções + área do usuário (5 atalhos, com dica no hover/foco), no desktop os links ficam só com texto e os espaçamentos são justos; o E2E mede `scrollWidth <= clientWidth` com login a 1280px **depois de `document.fonts.ready`** (com a fonte de fallback a barra é ~55px mais estreita e o problema não aparece; foi assim que um estouro de 2px passou despercebido até 2026-11-08). Uma 7ª seção não cabe: novos atalhos de usuário vão para a área do usuário, e mesmo ali o espaço está no limite.
- **Sincronizado em 2026-11-02:** o PC "Pichau" puxou os commits das Fases 15–17 (sem dependências nem migrations novas); os dois PCs estão no mesmo ponto.
- **Feito em 2026-10-30:** (1) 12 retratos de personagens adicionados; (2) Fase 16: docs revisadas contra o código, novo `docs/testing.md`, §9 de status em `requirements.md`, teste num clone limpo; (3) Fase 17: deploy Vercel + Render + Neon (detalhes abaixo e em `docs/development-phases.md`); (4) `.agents/` e `skills-lock.json` (skills do Neon) no `.gitignore`.
- **Produção:** Vercel (frontend, raiz `frontend/avatar-codex-web`) → rewrite `/api/*` → Render (`avatar-codex-api.onrender.com`, Docker, free, virginia, `render.yaml`) → Neon (AWS us-east-1, host direto sem pooler). Cookie de refresh segue `SameSite=Strict` (mesma origem pelo proxy), sem CORS em produção. Migrations + seed + usuário demo no startup (`appsettings.Production.json`; demo desligável com `Database__SeedDemoUser=false`). Deploy automático a cada push na `main` (Render só quando `backend/**` muda).
- **Segredos de produção** ficam só nos painéis: `ConnectionStrings__Default` no Render (colar sem aspas nem nada depois do último `;`) e `Jwt__Secret` gerado pelo Render. Nada disso no repo.
- **Dados de teste em produção:** os E2E agora excluem os próprios usuários; os `e2e_*@example.com` antigos continuam no Neon até o usuário rodar o SQL de `docs/deployment.md` §8 (que também explica como zerar o demo, que tem 20 XP de testes).
- **Pendências:** CI no GitHub Actions (adiado pelo usuário); opcionais em `docs/deployment.md` §7 (restringir a API ao tráfego da Vercel, domínio próprio). Skills do Neon ficam locais em `.agents/` (ignorado pelo Git); a skill `neon-postgres` confirmou o host direto para migrations.
- **Dicas de verificação:** o Chrome headless não renderiza larguras abaixo de ~500px (o print a 390px parece quebrado); para medir mobile, usar o viewport do Cypress. E2E contra produção: `npx cypress run --browser chrome --config baseUrl=https://avatar-codex.vercel.app --expose apiUrl=https://avatar-codex.vercel.app`.
- **Favicon:** os quatro elementos, gerados de `Desktopavatar-imageselements.jpg` em `public/favicon-32.png`, `favicon-48.png` e `apple-touch-icon.png` (System.Drawing). Nunca colocar arquivos em `dist/` (é gerado pelo build e ignorado pelo Git).
- **Imagens dos personagens:** **37 de 37 com imagem real** (12 em 2026-10-30, os 25 restantes em 2026-11-08). Ficam em `frontend/avatar-codex-web/public/images/characters/<nome-em-kebab>.jpg`, recortadas em retrato quadrado 320×320 (cabeça e ombros, JPEG qualidade 85, feitas com System.Drawing no PowerShell); os originais de corpo inteiro estão fora do repo, em `C:\Users\Matheus\Desktop\avatar-images` (os 37) e `C:\Users\Matheus\Pictures\avatar-codex-originais` (os 12 primeiros). Cada uma é referenciada em `characters.json` (`"image": { "url": "/images/characters/...", "sourceUrl": null, "credit": "Nickelodeon" }`) e o seeder sincroniza ao subir a API. O placeholder (DA-04) continua como fallback. A Avatar Wiki (Fandom) bloqueia hotlink, então as imagens precisam ser servidas pelo próprio projeto.
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
- **Testes:** backend 349 (220 unitários + 129 integração), frontend 174 (`npm run test:coverage` mede a cobertura), E2E Cypress com 12 testes (9 fluxos + habilidades, exclusão de conta, teste de elemento, ranking e edição do perfil; no Chrome, contra o build + banco `avatar_codex_e2e`; passo a passo em `docs/testing.md`). Lint, format e build passando. O Vitest às vezes estoura 5s em páginas lazy com a máquina carregada; isolado ou com `npx vitest run --maxWorkers=4` passa.
- **Dicas de shell:** heredocs com aspas e `\d` em regex já quebraram scripts várias vezes; para edições grandes, gravar um `.js` no scratchpad e rodar com `node`, ou usar o Edit. Prints do Cypress em larguras acima da janela headless saem cortados: medir com `getBoundingClientRect`, não pelo print.
- **Line endings:** nos dois PCs `core.autocrlf=true`; o Prettier reescreve os arquivos em LF e o `git status` lista todos como modificados, mas sem diferença de conteúdo (`git diff --ignore-cr-at-eol`). Commitar só os arquivos alterados de verdade.

**Próximo passo:** nada pendente no código. Candidatos a combinar: CI no GitHub Actions, opcionais de `docs/deployment.md` §7, limpeza dos `e2e_*` em produção (§8), funcionalidades sociais (P3).
- **Não iniciar trabalho novo sem confirmação explícita do usuário.**

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
| [docs/development-phases.md](docs/development-phases.md) | Fases 0–22, prioridades P0–P3, checklist da Fase 1 |
