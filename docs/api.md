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

> Os campos dependentes do usuário (`isFavorite`, `isDiscovered`) dos endpoints 🔓* entram na Fase 6, junto com a autenticação opcional.

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
