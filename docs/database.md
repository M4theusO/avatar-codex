# Banco de Dados

## 1. Convenções

- **PostgreSQL**, esquema gerenciado **exclusivamente por migrations do EF Core** (nada manual).
- Tabelas e colunas em `snake_case` (via `EFCore.NamingConventions`); entidades em PascalCase no C#.
- PK `int` identity para conteúdo; `Guid` para `User` (D12).
- Todas as FKs com índice; `ON DELETE` restritivo para conteúdo e `CASCADE` para dados do usuário.
- Timestamps em UTC (`timestamptz`); `CreatedAt`/`UpdatedAt` preenchidos automaticamente.
- Enums persistidos como `string` (legibilidade no banco).
- Comparações de unicidade de texto em minúsculas (índice único em `lower(email)`/`lower(username)` ou coluna normalizada).

### Tipo owned `ImageInfo` (D14)

Usado em `Character`, `Nation`, `Ability`, `Quiz`, `TimelineEvent`. Vira colunas na tabela dona:

| Campo | Tipo | Obs. |
|---|---|---|
| ImageUrl | varchar(500) | obrigatório quando houver imagem |
| ImageSourceUrl | varchar(500) | origem da imagem |
| ImageCredit | varchar(200) | crédito/autoria |

## 2. Diagrama ER (textual)

```text
                          ┌──────────┐
             ┌───────────►│ Element  │◄──────────────┬───────────────┐
             │            └────┬─────┘               │               │
             │                 │1:N                  │               │
             │         ┌───────▼────────┐            │               │
             │         │ BendingSubtype │            │               │
             │         └───────┬────────┘            │               │
             │                 │N:N                  │               │
┌────────┐   │N:1     ┌────────▼──────────┐   N:1    │          ┌────┴─────┐
│ Nation ├───┘◄───────┤    Character      ├──────────┘          │ Ability  │
└───┬────┘     N:1    └─┬───┬───┬───┬───┬─┘                     └────┬─────┘
    │                   │   │   │   │   └──N:N── CharacterAbility ───┘
    │                   │   │   │   └──N:N── CharacterSeries ── Series
    │                   │   │   └──N:N (self)── CharacterRelationship
    │                   │   └──N:N── CharacterAttributeValue ── ComparisonAttribute
    │                   └──N:N── TimelineEventCharacter ── TimelineEvent ──N:1── Era
    │                   │
    │      ┌────────────┼──────────────────────────────┐
    │      │            │                              │
    │  UserNation   UserCharacter             UserFavoriteCharacter
    │  Discovery    Discovery                          │
    │      │            │                              │
    │      └────────────┴──────────────┬───────────────┘
    │                                  │N:1
    │                            ┌─────▼─────┐ N:1  ┌─────────┐
    └───(N:1 via discovery)      │   User    ├─────►│ Element │ (elemento do perfil, opcional)
                                 └─┬───┬───┬─┘      └─────────┘
              ┌────────────────────┘   │   └──────────────────────┐
              │1:N                     │1:N                       │1:N
   ┌──────────▼──────────┐  ┌──────────▼─────────┐     ┌──────────▼─────────┐
   │ExperienceTransaction│  │  UserAchievement   │     │   RefreshToken     │
   └─────────────────────┘  └──────────┬─────────┘     └────────────────────┘
                                       │N:1
                                 ┌─────▼──────┐
                                 │Achievement │
                                 └────────────┘

 User 1:N UserQuizAttempt N:1 Quiz 1:N Question 1:N QuestionOption
          UserQuizAttempt 1:N UserQuizAnswer N:1 Question / QuestionOption

 User 1:N UserDailyChallenge N:1 DailyChallenge N:1 ChallengeTemplate
```

## 3. Entidades

### 3.1 Conteúdo

**Element**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(50) | obrigatório, único |
| Description | text | |
| Color | varchar(7) | hex, ex. `#2AA7C9` |
| Icon | varchar(50) | nome do ícone |

**BendingSubtype** (metal, sangue, lava, relâmpago, combustão… podem crescer)
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(80) | único |
| Description | text | |
| ElementId | int | FK → Element |

**Nation**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(100) | único |
| Description | text | |
| SymbolImage | ImageInfo | owned |
| ElementId | int? | FK → Element (nulo p/ ex. República Unida) |

**Series**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Code | varchar(10) | único (`ATLA`, `LOK`) |
| Name | varchar(100) | |

**Character**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(100) | obrigatório, índice |
| Aliases | text[] | apelidos |
| Description | varchar(500) | resumo para cards |
| Biography | text | |
| Image | ImageInfo | owned |
| NationId | int? | FK → Nation, índice |
| ElementId | int? | FK → Element (principal/nativo), índice; nulo p/ não-dobradores |
| IsBender | bool | |
| IsAvatar | bool | |
| Status | enum | `Alive`, `Deceased`, `Unknown` |
| FirstAppearance | varchar(150) | ex. "ATLA 1x01 – O Garoto no Iceberg" |
| CreatedAt / UpdatedAt | timestamptz | |

> O `BendingType` da especificação foi substituído pela N:N com `BendingSubtype`, que permite crescer (D6). O filtro "Tipo" (Avatar / dobrador / não-dobrador) é derivado de `IsAvatar`/`IsBender`.

**CharacterBendingSubtype** — PK (`CharacterId`, `BendingSubtypeId`)

**CharacterSeries** — PK (`CharacterId`, `SeriesId`)

**CharacterRelationship**
| Campo | Tipo | Restrições |
|---|---|---|
| CharacterId | int | FK, PK composta |
| RelatedCharacterId | int | FK, PK composta |
| Type | enum | `Family`, `Friend`, `Mentor`, `Student`, `Romantic`, `Ally`, `Rival`, `Enemy`; PK composta |
| Description | varchar(200) | ex. "Tio" |
| | | CHECK `CharacterId <> RelatedCharacterId` |

**Ability**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(100) | único |
| Description | text | |
| ElementId | int? | FK (nulo p/ habilidades sem dobra, ex. bloqueio de chi) |
| Image | ImageInfo | owned |

**CharacterAbility** — PK (`CharacterId`, `AbilityId`)

**ComparisonAttribute** (ex.: Força, Agilidade, Estratégia, Domínio elemental)
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(50) | único |
| Description | varchar(200) | |

**CharacterAttributeValue** — PK (`CharacterId`, `ComparisonAttributeId`), `Value` int com CHECK 0–100.

**Era**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(100) | ex. "Era de Aang", "Era de Korra" |
| Description | text | |
| Order | int | único |

**TimelineEvent**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Title | varchar(150) | |
| Description | text | |
| DateLabel | varchar(50) | ex. "100 AG" |
| Image | ImageInfo | owned |
| Order | int | único |
| EraId | int | FK → Era |

**TimelineEventCharacter** — PK (`TimelineEventId`, `CharacterId`)

### 3.2 Usuário e autenticação

**User**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | uuid | PK |
| Username | varchar(30) | único (case-insensitive) |
| Email | varchar(254) | único (case-insensitive) |
| PasswordHash | text | nunca exposto |
| AvatarUrl | varchar(500) | |
| ElementId | int? | FK → Element |
| ExperiencePoints | int | default 0, CHECK ≥ 0 (total desnormalizado, D5) |
| CreatedAt / UpdatedAt | timestamptz | |

> `Level` **não é coluna**: é calculado por `LevelCalculator` (RN-07).

**RefreshToken**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| UserId | uuid | FK, CASCADE |
| TokenHash | varchar(128) | único |
| ExpiresAt | timestamptz | |
| CreatedAt | timestamptz | |
| RevokedAt | timestamptz? | |

### 3.3 Exploração e favoritos

**UserCharacterDiscovery** — PK (`UserId`, `CharacterId`), `DiscoveredAt`; índice (`UserId`, `DiscoveredAt DESC`).

**UserNationDiscovery** — PK (`UserId`, `NationId`), `DiscoveredAt`. A 1ª descoberta concede +10 XP (`ExperienceTransaction.Type = NationDiscovered`, `SourceId = NationId`; DA-01).

**UserFavoriteCharacter** — PK (`UserId`, `CharacterId`), `CreatedAt`.

> As PKs compostas garantem por si só que não há descoberta nem favorito duplicado. A descoberta e a `ExperienceTransaction` são gravadas **na mesma transação**: se uma requisição concorrente já registrou, a violação de unicidade é tratada como "já descoberto" (RN-12).

### 3.4 Gamificação

**ExperienceTransaction**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | bigint | PK |
| UserId | uuid | FK, CASCADE, índice |
| Amount | int | CHECK > 0 |
| Type | enum | `CharacterDiscovered`, `NationDiscovered`, `QuizCompleted`, `QuizPerfect`, `ChallengeCompleted`, `AchievementUnlocked` |
| SourceId | int | id do personagem/nação/quiz/desafio/conquista que gerou o XP |
| Description | varchar(200) | |
| CreatedAt | timestamptz | |
| | | **UNIQUE (`UserId`, `Type`, `SourceId`)**: garante a idempotência (RN-05) |

**Achievement**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Code | varchar(50) | único, chave estável p/ seed (ex. `FIRST_STEP`) |
| Name | varchar(100) | |
| Description | varchar(300) | |
| Icon | varchar(50) | |
| RequirementType | enum | `AccountCreated`, `CharactersDiscovered`, `NationsDiscovered`, `AllNationsDiscovered`, `QuizzesCompleted`, `PerfectQuizzes`, `CorrectAnswers`, `ChallengesCompleted`, `LevelReached` |
| RequirementValue | int | |
| ExperienceReward | int | ≥ 0 |

**UserAchievement** — PK (`UserId`, `AchievementId`), `UnlockedAt`.

**ChallengeTemplate** (pool definido no seed, D8)
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Title | varchar(100) | |
| Description | varchar(300) | |
| ChallengeType | enum | `DiscoverCharacters`, `DiscoverNations`, `CompleteQuizzes`, `QuizScoreAtLeast` |
| TargetValue | int | quantidade, ou % para `QuizScoreAtLeast` |
| ExperienceReward | int | |
| IsActive | bool | |

**DailyChallenge**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| ChallengeTemplateId | int | FK |
| Date | date | índice; **UNIQUE (`ChallengeTemplateId`, `Date`)** |

> Os campos da especificação (`Title`, `ChallengeType`, `TargetValue`, `ExperienceReward`) ficam no template, e o `DailyChallenge` apenas o instancia em uma data.

**UserDailyChallenge**
| Campo | Tipo | Restrições |
|---|---|---|
| UserId | uuid | PK composta, FK |
| DailyChallengeId | int | PK composta, FK |
| Progress | int | ≥ 0 |
| IsCompleted | bool | |
| CompletedAt | timestamptz? | |

### 3.5 Quiz

**Quiz**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Title | varchar(150) | |
| Description | text | |
| Category | varchar(50) | texto livre (conteúdo no banco) |
| Difficulty | enum | `Easy`, `Medium`, `Hard` |
| Image | ImageInfo | owned |

**Question**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| QuizId | int | FK, CASCADE |
| QuestionText | varchar(500) | |
| ImageUrl | varchar(500)? | |
| Explanation | varchar(500)? | exibida no resultado |
| Order | int | UNIQUE (`QuizId`, `Order`) |

**QuestionOption**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| QuestionId | int | FK, CASCADE |
| Text | varchar(300) | |
| IsCorrect | bool | **nunca exposto antes da submissão**; índice único parcial em `QuestionId WHERE is_correct` (no máximo 1 correta) |
| Order | int | |

**UserQuizAttempt**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| UserId | uuid | FK, CASCADE |
| QuizId | int | FK |
| Score | int | acertos |
| TotalQuestions | int | |
| ExperienceEarned | int | |
| CompletedAt | timestamptz | índice (`UserId`, `CompletedAt DESC`) |

**UserQuizAnswer** (necessária para correção detalhada e para a conquista "100 respostas corretas")
| Campo | Tipo | Restrições |
|---|---|---|
| UserQuizAttemptId | int | PK composta, FK, CASCADE |
| QuestionId | int | PK composta, FK |
| SelectedOptionId | int | FK |
| IsCorrect | bool | |

## 4. Seed

- Dados em arquivos JSON versionados em `Infrastructure/Data/Seed/`, aplicados por um seeder idempotente (upsert por chave natural/`Code`) em Development.
- Conteúdo mínimo para demonstração:

| Tabela | Quantidade alvo |
|---|---|
| Element | 4 |
| BendingSubtype | ~8 |
| Nation | 5 (4 nações + República Unida) |
| Series | 2 |
| Character | ~40 (ATLA + LOK) |
| Ability | ~25 |
| ComparisonAttribute | 5 |
| Era / TimelineEvent | 3–4 / ~20 |
| Quiz / Question | 3–5 / ~10 por quiz |
| Achievement | ~12 |
| ChallengeTemplate | ~8 |

- Usuário de demonstração apenas em Development (credenciais documentadas no README).

## 5. Migrations

```bash
dotnet ef migrations add <Nome> -p backend/src/AvatarCodex.Infrastructure -s backend/src/AvatarCodex.Api
dotnet ef database update      -p backend/src/AvatarCodex.Infrastructure -s backend/src/AvatarCodex.Api
```

Uma migration por mudança coesa de esquema, com nome descritivo (ex.: `AddQuizTables`).
