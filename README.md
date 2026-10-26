# Avatar Codex

Enciclopédia interativa + plataforma de exploração + sistema de evolução do usuário, inspirada em *Avatar: The Last Airbender* e *The Legend of Korra*.

O usuário explora personagens, nações, elementos, habilidades e a linha do tempo do universo, e constrói sua própria jornada: descobertas, favoritos, quizzes, XP, níveis, conquistas e desafios diários.

> **Status:** Fase 3 concluída (API de conteúdo); próxima: Fase 4 (frontend base). Veja o plano completo em [docs/development-phases.md](docs/development-phases.md).

## Stack

| Camada | Tecnologias |
|---|---|
| Frontend | React 19, TypeScript, Vite, React Router, Axios, TanStack Query, CSS Modules, lucide-react |
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core, JWT, FluentValidation |
| Banco | PostgreSQL |
| Testes | xUnit, Vitest + React Testing Library, Cypress |
| Infra | Docker Compose |

## Estrutura

```text
avatar-codex/
├── backend/                  # AvatarCodex.sln + src/ (Api, Application, Domain, Infrastructure)
├── frontend/avatar-codex-web # SPA React
├── tests/                    # testes .NET (unitários e de integração)
├── docs/                     # requisitos, arquitetura, banco, API, UI/UX, fases
├── docker-compose.yml        # PostgreSQL
└── .env.example
```

Arquitetura detalhada em [docs/architecture.md](docs/architecture.md).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- PostgreSQL: via [Docker](https://www.docker.com/) (recomendado) **ou** uma instalação local

## Como executar

### 1. Banco de dados

**Com Docker:**

```bash
cp .env.example .env          # ajuste POSTGRES_PASSWORD
docker compose up -d
```

O banco fica em `localhost:5433` (porta definida por `POSTGRES_PORT`).

**Sem Docker:** crie um banco `avatar_codex` no seu PostgreSQL local e use a porta dele (normalmente `5432`) na connection string abaixo.

### 2. Backend

A connection string não fica no repositório. Configure-a com `user-secrets`:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5433;Database=avatar_codex;Username=avatar;Password=<sua-senha>" \
  --project src/AvatarCodex.Api

dotnet run --project src/AvatarCodex.Api --launch-profile http
```

Em Development, o startup aplica as migrations e o seed de conteúdo automaticamente (personagens, nações, elementos, habilidades). O seed é idempotente e pode rodar quantas vezes for preciso.

- API: http://localhost:5080
- Health check: http://localhost:5080/api/health
- Swagger: http://localhost:5080/swagger

### 3. Frontend

```bash
cd frontend/avatar-codex-web
cp .env.example .env.local
npm install
npm run dev
```

Aplicação: http://localhost:5173. A página inicial mostra se a API e o banco estão conectados.

## Testes e qualidade

```bash
# Backend
cd backend && dotnet test

# Backend, incluindo os testes com PostgreSQL real
# (cada classe cria e remove um banco avatar_codex_test_<guid>)
AVATARCODEX_TEST_DB="Host=localhost;Port=5432;Username=postgres;Password=<senha>" dotnet test

# Frontend
cd frontend/avatar-codex-web
npm run test          # Vitest
npm run lint          # oxlint
npm run format:check  # Prettier
npm run build
```

## Variáveis de ambiente

| Variável | Onde | Descrição |
|---|---|---|
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_PORT` | `.env` (raiz) | Usadas pelo `docker-compose.yml` |
| `ConnectionStrings:Default` | user-secrets ou env `ConnectionStrings__Default` | Conexão do backend com o PostgreSQL |
| `Cors:AllowedOrigins` | `appsettings.Development.json` ou env | Origens permitidas do frontend |
| `AVATARCODEX_TEST_DB` | variável de ambiente (opcional) | Servidor PostgreSQL para os testes de integração de banco |
| `VITE_API_URL` | `frontend/avatar-codex-web/.env.local` | URL base da API |

Nenhum secret é versionado. Use sempre os arquivos `.env.example` como modelo.

## Documentação

- [Requisitos](docs/requirements.md)
- [Arquitetura](docs/architecture.md)
- [Banco de dados](docs/database.md)
- [API](docs/api.md)
- [UI/UX](docs/ui-ux.md)
- [Fases de desenvolvimento](docs/development-phases.md)

## Aviso legal

Projeto **educacional e de portfólio**, sem fins comerciais. *Avatar: The Last Airbender* e *The Legend of Korra* e seus personagens, nomes e imagens pertencem aos seus respectivos detentores de direitos. A identidade visual do projeto é inspirada nos elementos da obra, não uma reprodução oficial.
