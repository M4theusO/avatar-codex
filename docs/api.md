# API

## 1. Convenções

- Base: `/api`. JSON em `camelCase`. Datas em ISO 8601 UTC.
- Recursos no plural; `/users/me/...` para dados do usuário autenticado (nunca se aceita `userId` do cliente).
- Códigos: `200` OK · `201` Created · `204` No Content · `400` validação · `401` não autenticado · `403` sem permissão · `404` não encontrado · `409` conflito · `422` regra de negócio · `500` erro interno.
- Documentação interativa via Swagger em Development.

## 2. Autenticação

- `Authorization: Bearer <accessToken>` em endpoints 🔒.
- Refresh token em cookie `httpOnly` + `Secure` + `SameSite=Strict` (`refresh_token`, `Path=/api/auth`), rotacionado a cada `/refresh` (ver `architecture.md` §3.4).
- Sem token ou com token inválido/expirado, endpoints 🔒 respondem `401` no formato padrão de erro (`"Você precisa estar autenticado."`).
- Endpoints públicos que mudam conforme o usuário (ex.: listagem de personagens com `isFavorite`/`isDiscovered`) aceitam token **opcional**.

## 3. Formato de erro

```json
{
  "status": 400,
  "message": "Os dados enviados são inválidos.",
  "errors": [
    { "field": "email", "message": "Email inválido." }
  ]
}
```

Nunca inclui stack trace nem detalhes internos.

## 4. Paginação, busca e ordenação

Query: `?page=1&pageSize=12&search=zuko&sort=name&order=asc` (`pageSize` máx. 50).

- `search`: busca no nome **e nos apelidos**, sem diferenciar maiúsculas/minúsculas; `%` e `_` são tratados como texto literal. Máx. 100 caracteres.
- `sort`: `name` (padrão), `nation`, `element`; `order`: `asc` (padrão) ou `desc`. Valores de texto não diferenciam maiúsculas/minúsculas.
- Na ordenação por nação/elemento, personagens sem nação/elemento ficam **sempre no fim**. O desempate é por nome e id, para que as páginas nunca repitam nem pulem itens.
- Parâmetros inválidos retornam `400` com um item em `errors` por campo (ex.: `{ "field": "pageSize", "message": "O tamanho da página deve estar entre 1 e 50." }`), inclusive erros de tipo como `?page=abc`.
- Enums são serializados como texto (`"status": "Alive"`, `"type": "Family"`).

```json
{
  "items": [],
  "page": 1,
  "pageSize": 12,
  "totalItems": 40,
  "totalPages": 4
}
```

## 5. Envelope de recompensa (`RewardResult`)

Endpoints que geram progresso retornam um objeto `reward`, que o frontend usa nos toasts:

```json
{
  "reward": {
    "xpGained": 60,
    "totalXp": 1040,
    "level": 6,
    "leveledUp": true,
    "currentLevelXp": 1000,
    "nextLevelXp": 1350,
    "levelProgress": 11,
    "achievementsUnlocked": [ { "id": 2, "name": "Curioso", "icon": "eye", "experienceReward": 50 } ],
    "challengesCompleted": [ { "id": 31, "title": "Descubra 3 personagens", "experienceReward": 30 } ]
  }
}
```

`currentLevelXp`/`nextLevelXp` são o XP mínimo do nível atual e do próximo, e `levelProgress` (0–99) é a porcentagem da barra. O frontend não conhece a fórmula de nível (RN-07): exibe esses valores, que também vêm no `UserDto` (login, refresh, `/auth/me`). ✅ Fase 7

## 6. Endpoints

🔓 público · 🔒 autenticado · 🔓* público com token opcional

### Auth
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| POST | `/api/auth/register` | 🔓 | Cadastro (`username`, `email`, `password`, `confirmPassword`) que já inicia a sessão (mesmo corpo do login + cookie). ✅ Fase 5. A resposta traz `reward` com a conquista "Primeiro passo" (nas demais respostas de auth, `reward` é `null`) ✅ Fase 8 | 201, 400, 409 |
| POST | `/api/auth/login` | 🔓 | Login (`email`, `password`) → `accessToken`, `user` + cookie de refresh. Email inexistente e senha errada têm a mesma mensagem. ✅ Fase 5 | 200, 400, 401 |
| POST | `/api/auth/refresh` | 🔓 (cookie) | Novo access token, refresh rotacionado. Cookie inválido/expirado/revogado → 401 e o cookie é apagado. ✅ Fase 5 | 200, 401 |
| POST | `/api/auth/logout` | 🔓 (cookie) | Revoga o refresh token e limpa o cookie. Idempotente e funciona com o access token expirado. ✅ Fase 5 | 204 |
| GET | `/api/auth/me` | 🔒 | Usuário autenticado (`UserDto`) ✅ Fase 5 | 200, 401 |

Validação do cadastro (mensagens em português, um item em `errors` por campo): `username` com 3–30 caracteres (`A-Z a-z 0-9 . _ -`), `email` válido (máx. 254), `password` com 8–128 caracteres, ao menos uma letra e um número (RN-02), `confirmPassword` igual à senha. Email é normalizado (trim + minúsculas) e email/username são únicos sem diferenciar maiúsculas (RN-01, `citext`).

Exemplo de resposta de login:

```json
{
  "accessToken": "eyJ...",
  "expiresIn": 900,
  "user": { "id": "4f1c…", "username": "matheus", "email": "m@ex.com", "avatarUrl": null, "element": null, "level": 1, "experiencePoints": 0, "currentLevelXp": 0, "nextLevelXp": 100, "levelProgress": 0, "createdAt": "2026-10-26T12:00:00Z" }
}
```

### Conteúdo
| Método | Rota | | Descrição | Status |
|---|---|---|---|---|
| GET | `/api/characters` | 🔓* | Lista paginada (`PagedResult<CharacterListItemDto>`). Filtros: `search`, `nationId`, `elementId`, `type` (`avatar`/`bender`/`nonBender`), `status` (`Alive`/`Deceased`/`Unknown`), `seriesId`; `sort` = `name`/`nation`/`element`; `order` = `asc`/`desc`. 400 em parâmetros inválidos. | ✅ Fase 3 |
| GET | `/api/characters/{id}` | 🔓* | Detalhe (`CharacterDetailsDto`): apelidos, biografia, nação, elemento, subtipos, habilidades, séries, primeira aparição e relações (com o personagem relacionado resumido). 404 se não existir. | ✅ Fase 3 |
| GET | `/api/characters/{id}/abilities` | 🔓 | Habilidades do personagem, ordenadas por nome. 404 se o personagem não existir. | ✅ Fase 3 |
| GET | `/api/characters/compare?ids=1,2` | 🔓 | Dados lado a lado + atributos (P2) | Fase 13 |
| GET | `/api/nations` | 🔓 | Nações com elemento e `characterCount`, na ordem dos elementos (Água, Terra, Fogo, Ar) e sem elemento por último | ✅ Fase 3 |
| GET | `/api/nations/{id}` | 🔓* | Detalhe + personagens. 404 se não existir. | ✅ Fase 3 |
| GET | `/api/elements` | 🔓 | Elementos (com cor e ícone) e seus subtipos de dobra | ✅ Fase 3 |
| GET | `/api/abilities` | 🔓 | Lista ordenada por nome (filtro opcional `elementId`) | ✅ Fase 3 |
| GET | `/api/timeline` | 🔓 | Eventos ordenados com era e personagens. Filtros: `eraId`, `characterId` (P2) | Fase 12 |
| GET | `/api/eras` | 🔓 | Eras (P2) | Fase 12 |
| GET | `/api/series` | 🔓 | Séries (para filtro) | ✅ Fase 3 |

> Com token, os endpoints 🔓* preenchem `isFavorite` e `isDiscovered` (✅ Fase 6). Sem token, ou com token inválido, respondem como para um visitante (`false`).

Exemplo de item da listagem:

```json
{
  "id": 9,
  "name": "Aang",
  "description": "O último dobrador de ar e o Avatar que encerrou a Guerra dos Cem Anos.",
  "image": null,
  "nation": { "id": 5, "name": "Nômades do Ar" },
  "element": { "id": 4, "name": "Ar", "color": "#D5C7A2", "icon": "wind" },
  "isBender": true,
  "isAvatar": true,
  "status": "Deceased"
}
```

### Descoberta e favoritos
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| POST | `/api/characters/{id}/discovery` | 🔒 | Registra a descoberta (idempotente): **+10 XP** na primeira vez. Retorna `{ isNew, reward }`. ✅ Fase 6 | 200, 401, 404 |
| POST | `/api/nations/{id}/discovery` | 🔒 | Registra a descoberta da nação (idempotente): **+10 XP** na primeira vez (DA-01). Retorna `{ isNew, reward }`. ✅ Fase 6 | 200, 401, 404 |
| POST | `/api/characters/{id}/favorite` | 🔒 | Adiciona favorito (idempotente, inclusive em paralelo) ✅ Fase 6 | 204, 401, 404 |
| DELETE | `/api/characters/{id}/favorite` | 🔒 | Remove favorito (idempotente) ✅ Fase 6 | 204, 401, 404 |

Regras de descoberta (RN-12):
- O frontend chama o endpoint **somente depois que o conteúdo principal da página foi carregado e exibido**, e uma única vez por montagem da página.
- Chamadas repetidas ou simultâneas retornam `200` com `{ "isNew": false, "reward": null }`, sem XP adicional.
- Recurso inexistente → `404`; sem token → `401`.

```json
{ "isNew": true, "reward": { "xpGained": 10, "totalXp": 100, "level": 2, "leveledUp": true, "currentLevelXp": 100, "nextLevelXp": 250, "levelProgress": 0, "achievementsUnlocked": [], "challengesCompleted": [] } }
```

### Usuário
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/users/me` | 🔒 | Perfil completo (nível, XP atual, XP do próximo nível, % de progresso) |
| PATCH | `/api/users/me` | 🔒 | Atualiza `avatarUrl`, `elementId` |
| GET | `/api/users/me/statistics` | 🔒 | Descobertos x/total, favoritos, quizzes, taxa de acerto, conquistas x/total. ✅ Fase 6 (parcial): `{ discoveredCharacters, totalCharacters, discoveredNations, totalNations, favorites }` + `unlockedAchievements`, `totalAchievements` ✅ Fase 8 + `completedQuizzes`, `totalQuizzes`, `quizAttempts`, `accuracyPercent` (acertos / respostas de todas as tentativas, `null` sem tentativas) ✅ Fase 9 |
| GET | `/api/users/me/dashboard` | 🔒 | Agregado do dashboard numa chamada (`DashboardDto`, ver abaixo) ✅ Fase 10 |
| GET | `/api/users/me/favorites` | 🔒 | Favoritos (`PagedResult<CharacterListItemDto>`, `?page`/`?pageSize`), mais recentes primeiro ✅ Fase 6 |
| GET | `/api/users/me/discoveries` | 🔒 | Descobertas de personagens e nações (`PagedResult<DiscoveryDto>`: `kind` = `Character`/`Nation`, `id`, `name`, `image`, `element`, `discoveredAt`), mais recentes primeiro ✅ Fase 6 |
| GET | `/api/users/me/experience` | 🔒 | Histórico de transações de XP (`PagedResult<ExperienceTransactionDto>`: `id`, `amount`, `type`, `description`, `createdAt`), mais recente primeiro ✅ Fase 7 |
| GET | `/api/users/me/quiz-history` | 🔒 | Todas as tentativas, mais recentes primeiro (`PagedResult<QuizAttemptDto>`: `id`, `quizId`, `quizTitle`, `score`, `totalQuestions`, `percentage`, `experienceEarned`, `completedAt`). O melhor resultado por quiz vem em `GET /api/quizzes` ✅ Fase 9 |
| GET | `/api/users/me/achievements` | 🔒 | Catálogo completo do ponto de vista do usuário (`UserAchievementDto[]`: `id`, `name`, `description`, `icon`, `experienceReward`, `isUnlocked`, `unlockedAt`, `progress`, `target`). Bloqueadas trazem o progresso (limitado à meta); desbloqueadas, a data ✅ Fase 8 |

Resposta de `GET /api/users/me/dashboard` (as escolhas são do backend; o frontend só exibe):

| Campo | Conteúdo |
|---|---|
| `user` | `UserDto` (nível, XP e progresso já calculados) |
| `statistics` | Mesmo formato de `GET /api/users/me/statistics` |
| `dailyChallenges` | Mesmo formato de `GET /api/challenges/daily` ✅ Fase 11 |
| `recentAchievements` | Até 3 `UserAchievementDto` desbloqueadas, mais recentes primeiro |
| `nextAchievement` | A bloqueada com maior fração da meta cumprida, ou `null` |
| `recentDiscoveries` | Até 5 `DiscoveryDto`, mais recentes primeiro |
| `recommendations` | 4 `CharacterListItemDto` ainda não descobertos; rotação diária estável por usuário |
| `featuredQuiz` | `QuizListItemDto` (com `bestResult`): primeiro não concluído, senão o mais perto do gabarito; `null` se todos foram gabaritados |
| `favorites` | Até 4 `CharacterListItemDto`, mais recentes primeiro |

### Quiz
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| GET | `/api/quizzes` | 🔓* | Lista (filtros `category` sem diferenciar maiúsculas e `difficulty` = `Easy`/`Medium`/`Hard`), sem paginação: `id`, `title`, `description`, `category`, `difficulty`, `image`, `questionCount` e `bestResult` (`score`, `totalQuestions`, `percentage`, `attempts`) do usuário autenticado ✅ Fase 9 | 200, 400 |
| GET | `/api/quizzes/{id}` | 🔒 | Quiz com perguntas e opções na ordem de exibição, **sem `isCorrect` e sem `explanation`** (opção = `id`, `text`) ✅ Fase 9 | 200, 401, 404 |
| POST | `/api/quizzes/{id}/attempts` | 🔒 | Submete as respostas e devolve o resultado. Toda tentativa é gravada; XP só na 1ª conclusão (+20) e no 1º gabarito (+50) de cada quiz (RN-06) ✅ Fase 9 | 201, 400, 401, 404 |

Requisição de submissão:

```json
{ "answers": [ { "questionId": 10, "optionId": 42 }, { "questionId": 11, "optionId": 47 } ] }
```

Resposta:

```json
{
  "attemptId": 77,
  "score": 8,
  "totalQuestions": 10,
  "percentage": 80,
  "experienceEarned": 20,
  "results": [
    { "questionId": 10, "selectedOptionId": 42, "correctOptionId": 42, "isCorrect": true, "explanation": "..." }
  ],
  "reward": { "xpGained": 20, "...": "..." }
}
```

Validações: todas as perguntas do quiz respondidas uma única vez, e cada opção deve pertencer à sua pergunta (senão 400, com as mensagens no campo `answers`). `experienceEarned` é o XP do quiz nesta tentativa (sem o das conquistas e dos desafios concluídos no mesmo evento); `reward` é `null` quando a tentativa não rendeu XP nem conquista.

> A especificação usava `POST /api/quizzes/{id}/attempt`; aqui foi pluralizado (`attempts`) por consistência REST.

### Conquistas
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/achievements` | 🔓 | Catálogo de conquistas (`AchievementDto[]`: `id`, `name`, `description`, `icon`, `experienceReward`), do cadastro às metas maiores ✅ Fase 8 |

Regras (RN-05, RN-08): as conquistas são avaliadas pelo backend depois de cada evento que gera XP e no cadastro. Cada uma é desbloqueada uma única vez (PK `UserId + AchievementId`, inclusive com requisições simultâneas), e o XP dela entra no mesmo `reward` (`xpGained` soma evento + conquistas), sem disparar nova avaliação. `icon` é o nome de um ícone do lucide-react.

### Desafios diários (P2)
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/challenges/daily` | 🔒 | Desafios de hoje (UTC) com progresso do usuário; gera os do dia se ainda não existirem. `DailyChallengesDto`: `date`, `resetsAt` (meia-noite UTC seguinte), `challenges[]` (`id`, `title`, `description`, `type`, `experienceReward`, `progress`, `target`, `isCompleted`, `completedAt`) ✅ Fase 11 |
| GET | `/api/users/me/challenges` | 🔒 | Histórico de concluídos, mais recentes primeiro (`PagedResult<CompletedChallengeDto>`: `id`, `title`, `description`, `experienceReward`, `date`, `completedAt`) ✅ Fase 11 |

Regras (RN-10, RN-05): 3 desafios por dia (UTC), sorteados de forma determinística pela data entre os modelos ativos, com tipos diferentes. `type` = `DiscoverCharacters` | `DiscoverNations` | `CompleteQuizzes` (quizzes diferentes no dia) | `QuizScoreAtLeast` (melhor % do dia; `progress`/`target` em porcentagem). O progresso é calculado pelas atividades do dia; quando a meta é atingida, o próprio evento conclui o desafio (uma única vez, PK `UserId + DailyChallengeId`) e o devolve em `reward.challengesCompleted`, com o XP somado em `xpGained`. Concluído aparece com `progress = target`.

> `POST /api/challenges/{id}/complete` da especificação **foi removido** (D9): a conclusão é automática no backend quando o progresso atinge a meta, e a recompensa volta no `reward` do evento que a causou.

### Futuro (P3)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/element-test` | Perguntas do teste de elemento |
| POST | `/api/element-test/result` | Calcula e salva o elemento no perfil |
| GET | `/api/ranking?scope=global\|monthly\|element` | Ranking por XP |
