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
| 1 | Setup | P0 | Repositório, solução .NET (4 projetos + 2 de teste), projeto React/TS/Vite, Docker com PostgreSQL, configuração de ambiente, `.gitignore`, `.env.example`, README inicial | Frontend e backend executando separadamente; API conecta no banco |
| 2 | Banco e domínio | P0 | Element, BendingSubtype, Nation, Series, Character, Ability e tabelas de junção, User, RefreshToken; configurações EF, DbContext, migrations, seed | Banco sobe do zero com dados de demonstração |
| 3 | API de conteúdo | P0 | Characters, Nations, Elements, Abilities, Series; busca, filtros, paginação, ordenação; middleware de erros; Swagger | Endpoints funcionando e com testes de integração básicos |
| 4 | Frontend base | P0 | Tokens/tema (paleta DA-02, fontes Cinzel/Inter DA-03, validação de contraste), layout, navbar, rotas, componentes base, cards, loading/error/empty, responsividade; páginas Home, Characters, CharacterDetails, Nations, Timeline (placeholder) | Navegação completa sobre dados reais, mobile-first; foco visual |
| 5 | Autenticação | P0 | Cadastro, login, logout, JWT + refresh, recuperação de sessão, rotas protegidas, perfil básico | Usuário real cria conta, loga, recarrega a página e continua logado |
| 6 | Favoritos e descoberta | P0/P1 | Favoritos, descoberta de personagens e nações, histórico de descobertas, XP inicial | Experiência personalizada; descoberta de personagem e de nação (+10 XP, DA-01) registrada só após o conteúdo ser exibido, sem XP duplicado mesmo com requisições concorrentes (testado) |
| 7 | XP e níveis | P1 | ExperienceTransaction, ProgressionService, LevelCalculator, XpBar, notificações de nível, `RewardResult` | Regras centralizadas no backend, com testes unitários |
| 8 | Conquistas | P1 | Achievement, UserAchievement, regras, desbloqueio, página de conquistas, notificações | Conquistas desbloqueiam automaticamente e uma única vez |
| 9 | Quiz | P1 | Quizzes, perguntas, opções, submissão, validação, resultado, XP, histórico; UI atraente | `isCorrect` nunca exposto antes da submissão; testes cobrindo isso |
| 10 | Dashboard | P1/P2 | Endpoint agregado + página: XP, nível, progresso, conquistas, desafios, descobertas, quiz recomendado, favoritos | Destino padrão após login |
| 11 | Desafios diários | P2 | ChallengeTemplate, geração diária, progresso, conclusão automática, recompensa, histórico | Testes com `IClock` simulando dias |
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
- [ ] Conexão real com o banco validada *(pendente: precisa da connection string do ambiente do usuário)*

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

**Resultado esperado:** frontend e backend executando separadamente, com a API conectada ao PostgreSQL. ✅ Executando separadamente; conexão real com o banco aguardando a configuração local.
