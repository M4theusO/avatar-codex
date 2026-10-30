# Avatar Codex

Enciclopédia interativa + plataforma de exploração + sistema de evolução do usuário, inspirada em *Avatar: The Last Airbender* e *The Legend of Korra*.

O usuário explora personagens, nações, a linha do tempo e comparações entre personagens, e constrói sua própria jornada: descobertas, favoritos, quizzes, XP, níveis, conquistas e desafios diários. Toda regra de progresso é decidida pelo backend.

> **Status:** v1 concluída (Fases 0–16). Próxima etapa: deploy (Fase 17). Plano completo em [docs/development-phases.md](docs/development-phases.md).

## Funcionalidades

| Área | O que tem |
|---|---|
| Conteúdo | 37 personagens com busca, filtros, ordenação e paginação (estado na URL); detalhe com habilidades e relações; 5 nações; linha do tempo com 4 eras e 17 eventos; comparação de dois personagens (radar, barras e tabela) |
| Conta | Cadastro, login, logout e sessão recuperada ao recarregar (JWT em memória + refresh token em cookie httpOnly) |
| Exploração | Descoberta de personagens e nações (+10 XP na primeira vez), favoritos, histórico de descobertas |
| Gamificação | XP e níveis, 6 conquistas, 4 quizzes com correção no backend, 3 desafios diários, painel personalizado e perfil com estatísticas |
| Qualidade | Mobile-first, acessível (WCAG AA, teclado, `prefers-reduced-motion`), loading/empty/error em todas as telas; 285 testes no backend, 139 no frontend e E2E no Cypress |

## Stack

| Camada | Tecnologias |
|---|---|
| Frontend | React 19, TypeScript, Vite, React Router, Axios, TanStack Query, CSS Modules + design tokens, lucide-react, oxlint + Prettier |
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core, JWT, FluentValidation |
| Banco | PostgreSQL (schema por migrations do EF Core) |
| Testes | xUnit, Vitest + React Testing Library, Cypress |
| Infra | Docker Compose (PostgreSQL), opcional |

## Arquitetura em resumo

```text
frontend (React SPA) ──HTTP/JSON + JWT──► AvatarCodex.Api ──► Application ──► Domain
                                                 │                  ▲
                                                 └──► Infrastructure ┘ (EF Core, PostgreSQL)
```

Controllers finos; toda regra de negócio fica na camada **Application** (com destaque para o `ProgressionService`, que concentra XP, nível, desafios e conquistas). O frontend só exibe o que a API decide. Detalhes em [docs/architecture.md](docs/architecture.md).

```text
avatar-codex/
├── backend/                   # AvatarCodex.sln + src/ (Api, Application, Domain, Infrastructure)
├── frontend/avatar-codex-web/ # SPA React (src/, cypress/, public/)
├── tests/                     # testes .NET (unitários e de integração)
├── docs/                      # requisitos, arquitetura, banco, API, UI/UX, testes, fases
├── docker-compose.yml         # PostgreSQL (opcional)
└── .env.example               # variáveis do docker-compose
```

## Pré-requisitos

| Ferramenta | Versão | Para quê |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10 | Backend e testes .NET |
| [Node.js](https://nodejs.org/) | 22 ou superior | Frontend |
| PostgreSQL | 16 ou superior | Banco. Via [Docker](https://www.docker.com/) **ou** instalação local |
| Google Chrome | atual | Só para os testes E2E |
| `dotnet-ef` | 10 | Só para **criar** migrations (`dotnet tool install -g dotnet-ef`) |

Para apenas rodar o projeto não é preciso o `dotnet-ef`: em Development a API aplica as migrations sozinha.

## Como executar

São três passos: banco, backend e frontend. Comandos em Bash (Git Bash no Windows); onde muda no PowerShell, há uma nota.

### 1. Banco de dados

Escolha **uma** das opções.

**A) Com Docker**

```bash
cp .env.example .env          # troque POSTGRES_PASSWORD
docker compose up -d
```

O PostgreSQL fica em `localhost:5433` (porta `POSTGRES_PORT`, para não conflitar com uma instalação local na 5432), com o usuário e o banco do `.env`.

**B) PostgreSQL instalado na máquina**

Use o usuário e a senha definidos na instalação (normalmente `postgres`, porta `5432`). Não é preciso criar o banco: na primeira subida a API cria `avatar_codex`, aplica as migrations e o seed.

### 2. Backend

A connection string e a chave do JWT não ficam no repositório. Configure-as com `user-secrets` (ficam no seu perfil de usuário, fora do projeto):

```bash
cd backend

# Opção A (Docker): use os valores do .env
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5433;Database=avatar_codex;Username=avatar;Password=<senha-do-.env>" \
  --project src/AvatarCodex.Api

# Opção B (local): troque porta, usuário e senha
# "Host=localhost;Port=5432;Database=avatar_codex;Username=postgres;Password=<sua-senha>"

# Chave de assinatura dos tokens (mínimo de 32 bytes, aleatória)
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)" --project src/AvatarCodex.Api

dotnet run --project src/AvatarCodex.Api --launch-profile http
```

> **PowerShell:** escreva cada comando numa linha só (sem `\`) e gere a chave com
> `$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)`.

Na primeira subida em Development a API aplica as migrations, o seed de conteúdo (personagens, nações, elementos, habilidades, timeline, atributos de comparação, quizzes, conquistas e desafios) e cria um **usuário de demonstração**. O seed é idempotente: roda a cada subida e só altera o que mudou nos JSON.

| Endereço | |
|---|---|
| API | http://localhost:5080 |
| Health check | http://localhost:5080/api/health (`"status": "Healthy"` quando o banco responde) |
| Swagger | http://localhost:5080/swagger |

### 3. Frontend

Em outro terminal:

```bash
cd frontend/avatar-codex-web
cp .env.example .env.local    # VITE_API_URL=http://localhost:5080
npm install
npm run dev
```

Abra **http://localhost:5173** (use `localhost`, não `127.0.0.1`: só essa origem está liberada no CORS).

### Usuário de demonstração

Criado só em Development:

| Email | Senha |
|---|---|
| `demo@avatarcodex.dev` | `Avatar123` |

Também dá para criar uma conta nova pela tela de cadastro.

## Testes e qualidade

```bash
# Backend: unitários (os de banco são ignorados sem a variável abaixo)
cd backend && dotnet test

# Backend completo, com PostgreSQL real (connection string do servidor, SEM Database)
AVATARCODEX_TEST_DB="Host=localhost;Port=5432;Username=postgres;Password=<senha>" dotnet test

# Frontend
cd frontend/avatar-codex-web
npm run test           # Vitest
npm run test:coverage  # com cobertura (coverage/)
npm run lint           # oxlint
npm run format:check   # Prettier
npm run build          # tsc + vite build
```

Os testes E2E (Cypress no Chrome) usam um banco próprio e o build do frontend. O passo a passo, a organização das suítes e o que cada uma cobre estão em [docs/testing.md](docs/testing.md).

## Variáveis de ambiente

| Variável | Onde | Descrição |
|---|---|---|
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_PORT` | `.env` (raiz) | Usadas apenas pelo `docker-compose.yml` |
| `ConnectionStrings:Default` | user-secrets ou env `ConnectionStrings__Default` | Conexão do backend com o PostgreSQL (**obrigatória**) |
| `Jwt:Secret` | user-secrets ou env `Jwt__Secret` | Chave de assinatura dos access tokens, ≥ 32 bytes (**obrigatória**) |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays` | `appsettings.json` ou env | Emissor/público e duração do access token (15 min) e do refresh token (7 dias) |
| `Cors:AllowedOrigins` | `appsettings.Development.json` ou env | Origens do frontend liberadas (`http://localhost:5173`) |
| `Database:MigrateAndSeedOnStartup` | `appsettings.Development.json` | Aplica migrations, seed e usuário demo no startup (`true` só em Development) |
| `AVATARCODEX_TEST_DB` | variável de ambiente (opcional) | Servidor PostgreSQL para os testes de integração |
| `VITE_API_URL` | `frontend/avatar-codex-web/.env.local` | URL base da API |

Nenhum secret é versionado; os arquivos `.env.example` servem de modelo.

## Problemas comuns

| Sintoma | Causa provável |
|---|---|
| A API não sobe e fala de `Jwt:Secret` ou de connection string | Faltou um dos `user-secrets` do passo 2 (confira com `dotnet user-secrets list --project src/AvatarCodex.Api`) |
| `/api/health` responde `Unhealthy` ou a API não conecta | PostgreSQL parado, porta errada (Docker usa **5433**, instalação local costuma usar **5432**) ou senha diferente |
| O frontend mostra erro de rede em tudo | API parada, `VITE_API_URL` errado no `.env.local`, ou o site foi aberto por `127.0.0.1` em vez de `localhost` (CORS) |
| O login funciona, mas ao recarregar a página a sessão some | Frontend e API em hosts diferentes: o cookie de refresh é `SameSite=Strict`; use `localhost` nos dois |
| `npm run e2e` não encontra o app | O `npm run preview` precisa estar rodando na porta 5173 (e o `npm run dev`, parado). Ver [docs/testing.md](docs/testing.md) |

## Documentação

| Documento | Conteúdo |
|---|---|
| [Requisitos](docs/requirements.md) | Requisitos funcionais, regras de negócio, não funcionais, status de implementação |
| [Arquitetura](docs/architecture.md) | Camadas, fluxos (progressão, autenticação, erros), frontend, decisões técnicas, riscos |
| [Banco de dados](docs/database.md) | Entidades, relacionamentos, constraints, seed e migrations |
| [API](docs/api.md) | Endpoints, payloads, autenticação, erros, paginação |
| [UI/UX](docs/ui-ux.md) | Identidade visual, rotas, páginas, componentes, estados, acessibilidade |
| [Testes](docs/testing.md) | Como rodar cada suíte, organização, cobertura e E2E |
| [Fases](docs/development-phases.md) | Fases 0–17 e o que foi entregue em cada uma |

## Aviso legal

Projeto **educacional e de portfólio**, sem fins comerciais. *Avatar: The Last Airbender*, *The Legend of Korra* e seus personagens, nomes e imagens pertencem aos seus respectivos detentores de direitos (Nickelodeon / Paramount). As imagens dos personagens são usadas apenas para ilustração e ficam em `frontend/avatar-codex-web/public/images/characters/`, com o crédito registrado no seed. A identidade visual do projeto é inspirada nos elementos da obra, não uma reprodução oficial.
