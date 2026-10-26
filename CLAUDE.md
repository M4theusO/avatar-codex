# CLAUDE.md — Avatar Codex

Guia para o Claude Code trabalhar neste repositório. Este arquivo é o **resumo**; os detalhes estão em `docs/`.

## Visão geral

Aplicação web fullstack inspirada em *Avatar: The Last Airbender* e *The Legend of Korra*.
Não é um CRUD de personagens: é uma **enciclopédia interativa + plataforma de exploração + sistema de evolução do usuário** (XP, níveis, conquistas, quizzes, desafios, favoritos).

Projeto de **portfólio** (fins educacionais). Deve parecer um produto real, ter arquitetura justificável, ser testável e fácil de evoluir.

**Prioridades, em ordem:** 1. experiência visual · 2. organização · 3. arquitetura · 4. funcionalidade ponta a ponta · 5. testabilidade · 6. documentação.

## Status atual

**Fase 4 — Frontend base** (próxima, ainda não iniciada). Fases 0 a 3 concluídas e publicadas no GitHub (`docs/development-phases.md`).

**Onde paramos (2026-10-26):**
- Backend pronto até a API de conteúdo: entidades, migration, seed (37 personagens), endpoints de personagens/nações/elementos/habilidades/séries, tratamento global de erros, 64 testes passando.
- Frontend ainda é só o setup da Fase 1 (página inicial com status da API).
- **Decisão pendente antes da Fase 4:** imagens dos personagens. Recomendação apresentada: placeholders próprios (iniciais + cor do elemento) agora e curadoria de imagens reais na Fase 15. Aguardando resposta do usuário.
- **Não iniciar uma nova fase sem confirmação explícita do usuário.**

**Ambiente local:** sem Docker; PostgreSQL 18 local na porta 5432, banco `avatar_codex`, connection string nos user-secrets do `AvatarCodex.Api`. Testes de banco precisam de `AVATARCODEX_TEST_DB` (connection string do servidor, sem `Database`).

## Decisões aprovadas

- **DA-01** — Descoberta de nação: +10 XP só na 1ª vez por usuário, registrada depois que o conteúdo principal foi exibido, com idempotência garantida no backend.
- **DA-02** — Paleta dark: fundo `#101820`, superfície `#1B2835`; acentos Água `#3186B5`, Fogo `#B86B35`, Terra `#638B58`, Ar `#D5C7A2`. Elementos são acentos, nunca dominantes; Água/Fogo/Terra não podem ser usadas como texto pequeno (contraste).
- **DA-03** — Tipografia: Cinzel em títulos e destaques narrativos (com moderação); Inter em todo o resto.

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
```

Connection string via `dotnet user-secrets` no projeto `AvatarCodex.Api` (ver README).

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/requirements.md](docs/requirements.md) | Requisitos funcionais, regras de negócio, requisitos não funcionais |
| [docs/architecture.md](docs/architecture.md) | Arquitetura, estrutura de pastas, fluxos, decisões técnicas, riscos |
| [docs/database.md](docs/database.md) | Entidades, relacionamentos, constraints, diagrama ER, seed |
| [docs/api.md](docs/api.md) | Endpoints, payloads, autenticação, erros, paginação |
| [docs/ui-ux.md](docs/ui-ux.md) | Identidade visual, rotas e páginas, componentes, estados, acessibilidade |
| [docs/development-phases.md](docs/development-phases.md) | Fases 0–17, prioridades P0–P3, checklist da Fase 1 |
