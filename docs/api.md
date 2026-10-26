# API

## 1. Convenções

- Base: `/api`. JSON em `camelCase`. Datas em ISO 8601 UTC.
- Recursos no plural; `/users/me/...` para dados do usuário autenticado (nunca se aceita `userId` do cliente).
- Códigos: `200` OK · `201` Created · `204` No Content · `400` validação · `401` não autenticado · `403` sem permissão · `404` não encontrado · `409` conflito · `422` regra de negócio · `500` erro interno.
- Documentação interativa via Swagger em Development.

## 2. Autenticação

- `Authorization: Bearer <accessToken>` em endpoints 🔒.
- Refresh token em cookie `httpOnly` (`refresh_token`, `Path=/api/auth`), rotacionado a cada `/refresh` (ver `architecture.md` §3.4).
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
    "totalXp": 1240,
    "level": 12,
    "leveledUp": true,
    "achievementsUnlocked": [ { "id": 2, "name": "Curioso", "icon": "eye", "experienceReward": 50 } ],
    "challengesCompleted": [ { "id": 31, "title": "Descubra 3 personagens", "experienceReward": 30 } ]
  }
}
```

## 6. Endpoints

🔓 público · 🔒 autenticado · 🔓* público com token opcional

### Auth
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| POST | `/api/auth/register` | 🔓 | Cadastro (`username`, `email`, `password`, `confirmPassword`). Desbloqueia "Primeiro passo". | 201, 400, 409 |
| POST | `/api/auth/login` | 🔓 | Login (`email`, `password`) → `accessToken`, `user` + cookie de refresh | 200, 400, 401 |
| POST | `/api/auth/refresh` | 🔓 (cookie) | Novo access token, refresh rotacionado | 200, 401 |
| POST | `/api/auth/logout` | 🔒 | Revoga o refresh token e limpa o cookie | 204 |
| GET | `/api/auth/me` | 🔒 | Usuário autenticado (resumo) | 200, 401 |

Exemplo de resposta de login:

```json
{
  "accessToken": "eyJ...",
  "expiresIn": 900,
  "user": { "id": "4f1c…", "username": "matheus", "email": "m@ex.com", "avatarUrl": null, "element": null, "level": 1, "experiencePoints": 0 }
}
```

### Conteúdo
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/characters` | 🔓* | Lista paginada. Filtros: `search`, `nationId`, `elementId`, `type` (`avatar`/`bender`/`nonBender`), `status`, `seriesId`; `sort` = `name`/`nation`/`element`. |
| GET | `/api/characters/{id}` | 🔓* | Detalhe com nação, elemento, subtipos, habilidades, séries, relações e relacionados. 404 se não existir. |
| GET | `/api/characters/{id}/abilities` | 🔓 | Habilidades do personagem |
| GET | `/api/characters/compare?ids=1,2` | 🔓 | Dados lado a lado + atributos (P2) |
| GET | `/api/nations` | 🔓 | Lista de nações |
| GET | `/api/nations/{id}` | 🔓* | Detalhe + personagens |
| GET | `/api/elements` | 🔓 | Elementos com subtipos |
| GET | `/api/abilities` | 🔓 | Lista (filtro `elementId`) |
| GET | `/api/timeline` | 🔓 | Eventos ordenados com era e personagens. Filtros: `eraId`, `characterId` (P2) |
| GET | `/api/eras` | 🔓 | Eras (P2) |
| GET | `/api/series` | 🔓 | Séries (para filtro) |

### Descoberta e favoritos
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| POST | `/api/characters/{id}/discovery` | 🔒 | Registra a descoberta (idempotente). Retorna `{ isNew, reward }`. | 200, 404 |
| POST | `/api/nations/{id}/discovery` | 🔒 | Registra a descoberta da nação (idempotente): **+10 XP** na primeira vez (DA-01). Retorna `{ isNew, reward }`. | 200, 404 |
| POST | `/api/characters/{id}/favorite` | 🔒 | Adiciona favorito (idempotente) | 204, 404 |
| DELETE | `/api/characters/{id}/favorite` | 🔒 | Remove favorito | 204, 404 |

Regras de descoberta (RN-12):
- O frontend chama o endpoint **somente depois que o conteúdo principal da página foi carregado e exibido**, e uma única vez por montagem da página.
- Chamadas repetidas ou simultâneas retornam `200` com `{ "isNew": false, "reward": null }`, sem XP adicional.
- Recurso inexistente → `404`; sem token → `401`.

```json
{ "isNew": true, "reward": { "xpGained": 10, "totalXp": 130, "level": 2, "leveledUp": true, "achievementsUnlocked": [], "challengesCompleted": [] } }
```

### Usuário
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/users/me` | 🔒 | Perfil completo (nível, XP atual, XP do próximo nível, % de progresso) |
| PATCH | `/api/users/me` | 🔒 | Atualiza `avatarUrl`, `elementId` |
| GET | `/api/users/me/statistics` | 🔒 | Descobertos x/total, favoritos, quizzes, taxa de acerto, conquistas x/total |
| GET | `/api/users/me/dashboard` | 🔒 | Agregado do dashboard (evita várias chamadas) |
| GET | `/api/users/me/favorites` | 🔒 | Favoritos (paginado) |
| GET | `/api/users/me/discoveries` | 🔒 | Descobertas (paginado, mais recentes primeiro) |
| GET | `/api/users/me/experience` | 🔒 | Histórico de transações de XP (paginado) |
| GET | `/api/users/me/quiz-history` | 🔒 | Tentativas: pontuação, %, data e melhor resultado por quiz |
| GET | `/api/users/me/achievements` | 🔒 | Conquistas desbloqueadas + progresso das bloqueadas |

### Quiz
| Método | Rota | | Descrição | Respostas |
|---|---|---|---|---|
| GET | `/api/quizzes` | 🔓* | Lista (filtros `category`, `difficulty`), com melhor resultado do usuário se autenticado | 200 |
| GET | `/api/quizzes/{id}` | 🔒 | Quiz com perguntas e opções **sem `isCorrect`** | 200, 404 |
| POST | `/api/quizzes/{id}/attempts` | 🔒 | Submete as respostas e devolve o resultado | 201, 400, 404 |

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
  "results": [
    { "questionId": 10, "selectedOptionId": 42, "correctOptionId": 42, "isCorrect": true, "explanation": "..." }
  ],
  "reward": { "xpGained": 20, "...": "..." }
}
```

Validações: todas as perguntas do quiz respondidas uma única vez, e cada opção deve pertencer à sua pergunta (senão 400).

> A especificação usava `POST /api/quizzes/{id}/attempt`; aqui foi pluralizado (`attempts`) por consistência REST.

### Conquistas
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/achievements` | 🔓 | Catálogo de conquistas |

### Desafios diários (P2)
| Método | Rota | | Descrição |
|---|---|---|---|
| GET | `/api/challenges/daily` | 🔒 | Desafios de hoje (UTC) com progresso do usuário; gera os do dia se ainda não existirem |
| GET | `/api/users/me/challenges` | 🔒 | Histórico de desafios concluídos |

> `POST /api/challenges/{id}/complete` da especificação **foi removido** (D9): a conclusão é automática no backend quando o progresso atinge a meta, e a recompensa volta no `reward` do evento que a causou.

### Futuro (P3)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/element-test` | Perguntas do teste de elemento |
| POST | `/api/element-test/result` | Calcula e salva o elemento no perfil |
| GET | `/api/ranking?scope=global\|monthly\|element` | Ranking por XP |
