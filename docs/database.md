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
| Name | varchar(100) | obrigatório, único (chave natural do seed) |
| Aliases | text[] | apelidos |
| Description | varchar(500) | resumo para cards |
| Biography | text | |
| Image | ImageInfo | owned |
| NationId | int? | FK → Nation, índice |
| ElementId | int? | FK → Element (principal/nativo), índice; nulo p/ não-dobradores |
| IsBender | bool | |
| IsAvatar | bool | |
| Status | enum | `Alive`, `Deceased`, `Unknown` |
| FirstAppearance | varchar(150)? | ex. "ATLA – Livro 1, Ep. 1"; nulo quando não confirmado |
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

Migration `AddCharacterComparison`.

**ComparisonAttribute** (Domínio elemental, Combate, Agilidade, Estratégia, Liderança)
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(50) | único (chave do seed) |
| Description | varchar(200) | |
| Order | int | ordem de exibição (posição no JSON; não é único) |

**CharacterAttributeValue** — PK (`CharacterId`, `ComparisonAttributeId`), `Value` int com CHECK 0–100 (`ck_character_attribute_values_value_range`); tabela `character_attribute_values`, CASCADE pelo personagem e pelo atributo.

Migration `AddTimeline`. `Order` vem da posição no JSON; ao reordenar, o seed grava valores negativos temporários antes dos definitivos, para não violar a unicidade.

**Era**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Name | varchar(100) | único; ex. "A Guerra dos Cem Anos", "A Era de Korra" |
| Description | text | |
| Order | int | único |

**TimelineEvent**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Title | varchar(150) | único (chave do seed) |
| Description | text | |
| DateLabel | varchar(50) | ex. "100 AG" |
| Image | ImageInfo | owned |
| Order | int | único |
| EraId | int | FK → Era |

**TimelineEventCharacter** — PK (`TimelineEventId`, `CharacterId`); tabela `timeline_event_characters`, CASCADE pelo evento e RESTRICT pelo personagem.

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

> Migration `AddFavoritesDiscoveriesAndExperience`. Tabelas: `character_discoveries`, `nation_discoveries`, `favorite_characters`. FK do usuário em CASCADE; FK do conteúdo em RESTRICT.

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
| ExperienceReward | int | > 0 (todo desbloqueio gera uma `ExperienceTransaction`) |

**UserAchievement** — PK (`UserId`, `AchievementId`), `UnlockedAt`; índice (`UserId`, `UnlockedAt` desc). A PK garante o desbloqueio único (RN-05). Migration `AddAchievements`.

Migration `AddDailyChallenges`.

**ChallengeTemplate** (pool definido no seed, D8)
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| Code | varchar(50) | único; chave estável do seed |
| Title | varchar(100) | |
| Description | varchar(300) | |
| ChallengeType | enum | `DiscoverCharacters`, `DiscoverNations`, `CompleteQuizzes`, `QuizScoreAtLeast` |
| TargetValue | int | quantidade, ou % para `QuizScoreAtLeast`; CHECK > 0 |
| ExperienceReward | int | CHECK > 0 |
| IsActive | bool | |

**DailyChallenge**
| Campo | Tipo | Restrições |
|---|---|---|
| Id | int | PK |
| ChallengeTemplateId | int | FK |
| Date | date | índice; **UNIQUE (`ChallengeTemplateId`, `Date`)** |

> Os campos da especificação (`Title`, `ChallengeType`, `TargetValue`, `ExperienceReward`) ficam no template, e o `DailyChallenge` apenas o instancia em uma data.

**UserDailyChallenge** (registro da conclusão)
| Campo | Tipo | Restrições |
|---|---|---|
| UserId | uuid | PK composta, FK (CASCADE) |
| DailyChallengeId | int | PK composta, FK (RESTRICT) |
| CompletedAt | timestamptz | índice (`UserId`, `CompletedAt` desc) |

> Diferente do planejado, não há `Progress`/`IsCompleted`: o progresso é calculado a partir das descobertas e tentativas de quiz do dia (como nas conquistas), e a linha só existe quando o desafio foi concluído. A PK impede concluir (e ganhar XP) duas vezes (RN-05).

### 3.5 Quiz

Migration `AddQuizTables` (tabelas `quizzes`, `questions`, `question_options`, `quiz_attempts`, `quiz_answers`). `Quiz.Title` é único (chave do seed); `Question` e `QuestionOption` têm `Order` único dentro do pai; `UserQuizAttempt` tem CHECK `0 ≤ score ≤ total_questions` e `experience_earned ≥ 0`; o quiz é `RESTRICT` para as tentativas e as respostas apontam para pergunta e opção com `RESTRICT` (o seed nunca remove perguntas nem opções).

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

- Dados em arquivos JSON versionados em `Infrastructure/Data/Seed/Content/` (embutidos no assembly), aplicados pelo `ContentSeeder` no startup da API quando `Database:MigrateAndSeedOnStartup = true` (apenas `appsettings.Development.json`).
- O seeder é **idempotente**: faz upsert pela chave natural (nome; `Code` para séries), sincroniza as N:N e as relações, e **não remove** conteúdo que não esteja nos JSON. Rodar duas vezes não altera nenhuma linha.
- Antes de gravar, `ContentSeedData.Validate()` verifica nomes duplicados e referências inexistentes entre os arquivos. Um seed inválido impede o startup.
- Referências entre arquivos são feitas por nome (ex.: `"nation": "Nação do Fogo"`).
- **Imagens (DA-04):** `characters.json` aceita `image` (`url`, `sourceUrl`, `credit`), sincronizado no `ImageInfo` a cada subida. 12 dos 37 personagens têm retrato; os demais ficam com `image` nulo e aparecem com o placeholder. Os arquivos ficam no frontend, em `public/images/characters/<nome-em-kebab>.jpg` (retrato quadrado de 320×320, cabeça e ombros), e a `url` é relativa (`/images/characters/aang.jpg`). Para adicionar uma imagem: coloque o arquivo na pasta, preencha `image` no personagem e reinicie a API. Nações, habilidades, quizzes e eventos da timeline ainda não têm imagens.
- Conteúdo mínimo para demonstração:

| Tabela | Quantidade alvo | Atual |
|---|---|---|
| Element | 4 | 4 |
| BendingSubtype | ~8 | 9 |
| Nation | 5 (4 nações + República Unida) | 5 |
| Series | 2 | 2 |
| Character | ~40 (ATLA + LOK) | 37 |
| Ability | ~25 | 23 |
| CharacterRelationship | — | 79 |
| ComparisonAttribute / CharacterAttributeValue | 5 | 5 / 185 (`comparison-attributes.json`, `character-attributes.json`: todos os 37 personagens) |
| Era / TimelineEvent | 3–4 / ~20 | 4 / 17 (`eras.json`, `timeline.json`) |
| Quiz / Question | 3–5 / ~10 por quiz | 4 / 10 por quiz (`quizzes.json`) |
| Achievement | ~12 | 6 (`achievements.json`, upsert por `Code`) |
| ChallengeTemplate | ~8 | 8 (`challenges.json`, upsert por `Code`) |

- Usuário de demonstração apenas em Development (`DemoUserSeeder`, credenciais no README): criado no startup se ainda não existir, com a senha em hash PBKDF2.

## 5. Migrations

```bash
dotnet ef migrations add <Nome> -p backend/src/AvatarCodex.Infrastructure -s backend/src/AvatarCodex.Api
dotnet ef database update      -p backend/src/AvatarCodex.Infrastructure -s backend/src/AvatarCodex.Api
```

Uma migration por mudança coesa de esquema, com nome descritivo (ex.: `AddQuizTables`).

| Migration | Conteúdo |
|---|---|
| `InitialContentSchema` | Extensão `citext`; `elements`, `bending_subtypes`, `nations`, `series`, `abilities`, `characters`, `character_relationships`, `character_abilities`, `character_bending_subtypes`, `character_series`, `users`, `refresh_tokens` |
| `AddFavoritesDiscoveriesAndExperience` | `character_discoveries` (UserCharacterDiscovery), `nation_discoveries` (UserNationDiscovery), `favorite_characters` (UserFavoriteCharacter), `experience_transactions` com unique (`user_id`, `type`, `source_id`) e `CHECK amount > 0`. `type` gravado como texto. |
| `AddAchievements` | `achievements` (`code` único, `CHECK experience_reward > 0`), `user_achievements` com PK (`user_id`, `achievement_id`) |
| `AddQuizTables` | `quizzes` (`title` único), `questions` e `question_options` (`order` único dentro do pai; no máximo uma opção correta por pergunta, índice único parcial), `quiz_attempts` (CHECKs de pontuação e XP), `quiz_answers` |
| `AddDailyChallenges` | `challenge_templates` (`code` único, CHECKs de meta e XP), `daily_challenges` com unique (`challenge_template_id`, `date`), `user_daily_challenges` com PK (`user_id`, `daily_challenge_id`) |
| `AddTimeline` | `eras` (`name` e `order` únicos), `timeline_events` (`title` e `order` únicos, imagem owned, FK `era_id` RESTRICT), `timeline_event_characters` |
| `AddCharacterComparison` | `comparison_attributes` (`name` único, `order`), `character_attribute_values` (PK composta, `CHECK value` 0–100) |

## 6. Testes de banco

Os testes de integração em `tests/AvatarCodex.IntegrationTests/Database` criam um banco descartável por classe de teste (`avatar_codex_test_<guid>`), aplicam as migrations e o removem no final. O servidor vem da variável de ambiente `AVATARCODEX_TEST_DB` (connection string **sem** `Database`); sem ela, esses testes são ignorados.
