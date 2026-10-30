# Requisitos

## 1. Visão do produto

Enciclopédia interativa do universo de Avatar (*The Last Airbender* e *The Legend of Korra*) com exploração e progressão do usuário. O usuário explora personagens, nações, elementos, habilidades e eventos, e ao mesmo tempo constrói uma jornada própria: XP, níveis, conquistas, favoritos, quizzes e desafios.

O projeto é peça de portfólio e deve demonstrar: frontend moderno, backend REST, banco relacional, autenticação/autorização, arquitetura, modelagem de dados, integração front/back, testes automatizados, UX/UI, gamificação e documentação.

## 2. Objetivos do usuário

1. Criar conta e fazer login.
2. Explorar o universo (personagens, nações, elementos, habilidades, linha do tempo).
3. Descobrir e favoritar personagens.
4. Fazer quizzes.
5. Ganhar XP, subir de nível e desbloquear conquistas.
6. Participar de desafios diários.
7. Acompanhar progresso e estatísticas pessoais.
8. Comparar personagens.
9. Escolher/descobrir um elemento associado ao perfil.

## 3. Perfis de usuário

| Perfil | Acesso |
|---|---|
| **Visitante** | Home, listagem/detalhe de personagens, nações, elementos, timeline, comparação, listagem de quizzes. Não ganha XP nem registra descobertas. |
| **Usuário autenticado** | Tudo do visitante + dashboard, perfil, favoritos, descobertas, XP/níveis, conquistas, jogar quizzes, histórico de quizzes, desafios. |

> Não há perfil administrador na v1: o conteúdo é mantido via seed/migrations (ver `architecture.md`, decisão D11).

## 4. Requisitos funcionais

Prioridades: **P0** essencial · **P1** núcleo de gamificação · **P2** complementar · **P3** futuro.

### 4.1 Autenticação — P0
| ID | Requisito |
|---|---|
| RF-AUTH-01 | Cadastro com nome de usuário, email, senha e confirmação de senha. |
| RF-AUTH-02 | Login com email e senha. |
| RF-AUTH-03 | Logout. |
| RF-AUTH-04 | Recuperação de sessão ao recarregar a página. |
| RF-AUTH-05 | Proteção de rotas privadas: perfil, favoritos, progresso, conquistas, histórico de quizzes, desafios, dashboard. |

### 4.2 Conteúdo — P0
| ID | Requisito |
|---|---|
| RF-CHAR-01 | Listagem de personagens em grid de cards com busca, filtros (nação, elemento, tipo, status, série), ordenação e paginação. |
| RF-CHAR-02 | Card exibe imagem, nome, elemento, nação, indicador de descoberta e de favorito (estes dois apenas para usuários logados). |
| RF-CHAR-03 | Página de detalhe: imagem grande, nome, apelidos, nação, elemento, dobramento (subtipos), descrição, biografia, habilidades, aparições, relações e personagens relacionados. |
| RF-NAT-01 | Listagem e detalhe de nações (descrição, símbolo, elemento, personagens). |
| RF-ELEM-01 | Consulta de elementos e seus subtipos de dobramento (metal, sangue, lava, relâmpago, combustão…). |
| RF-ABIL-01 | Consulta de habilidades e personagens que as possuem. |

### 4.3 Perfil e favoritos — P0
| ID | Requisito |
|---|---|
| RF-FAV-01 | Adicionar, remover e listar personagens favoritos. |
| RF-PROF-01 | Perfil com avatar, nome, elemento, nível, XP, barra de progresso, personagens descobertos (x / total), favoritos, conquistas (x / total), quizzes realizados, taxa de acerto e estatísticas. |

### 4.4 Descoberta — P1
| ID | Requisito |
|---|---|
| RF-DISC-01 | Ao visitar um personagem pela primeira vez, registrar descoberta, conceder XP e mostrar feedback visual ("✨ Nova descoberta — Você descobriu Zuko! +10 XP"). |
| RF-DISC-02 | Registrar também descoberta de nações (necessário para a conquista "Historiador" e o desafio "Explore uma nação"), com **+10 XP** na primeira descoberta de cada nação (DA-01). |
| RF-DISC-03 | Histórico de descobertas (recentes no dashboard e perfil). |

### 4.5 XP e níveis — P1
| ID | Requisito |
|---|---|
| RF-XP-01 | Sistema centralizado de XP com registro auditável de cada movimentação. |
| RF-XP-02 | Nível calculado a partir do XP total. |
| RF-XP-03 | Notificação de subida de nível ("🎉 Novo nível! Você alcançou o nível 10."). |

### 4.6 Conquistas — P1
| ID | Requisito |
|---|---|
| RF-ACH-01 | Catálogo de conquistas (desbloqueadas e bloqueadas) com progresso. |
| RF-ACH-02 | Desbloqueio automático verificado pelo backend, com notificação. |

Conquistas iniciais: **Primeiro passo** (criar conta), **Curioso** (10 personagens descobertos), **Explorador** (50 personagens na especificação; no seed, 30, porque há 37 personagens), **Historiador** (todas as nações), **Mestre dos elementos** (N desafios concluídos), **Conhecedor de Avatar** (100 respostas corretas).

### 4.7 Quiz — P1
| ID | Requisito |
|---|---|
| RF-QUIZ-01 | Listagem de quizzes (título, descrição, categoria, dificuldade, imagem). |
| RF-QUIZ-02 | Execução de quiz sem expor as respostas corretas. |
| RF-QUIZ-03 | Submissão validada pelo backend; resultado com pontuação, porcentagem, correção por pergunta e XP ganho. |
| RF-QUIZ-04 | Histórico: quizzes realizados, pontuação, porcentagem, data e melhor resultado. |

### 4.8 Dashboard — P1/P2
| ID | Requisito |
|---|---|
| RF-DASH-01 | Área inicial personalizada após login: saudação, nível, XP, progresso, desafios do dia, últimas conquistas, descobertas recentes, recomendação de conteúdo, quiz em destaque, favoritos. |

### 4.9 Desafios diários — P2
| ID | Requisito |
|---|---|
| RF-CHAL-01 | Desafios do dia com progresso individual (ex.: "Descubra 3 personagens", "Complete 1 quiz", "Acerte 80% de um quiz", "Explore uma nação"). |
| RF-CHAL-02 | Conclusão e recompensa de XP determinadas pelo backend; histórico de desafios concluídos. |

### 4.10 Linha do tempo — P2
| ID | Requisito |
|---|---|
| RF-TIME-01 | Timeline visual de eventos ordenados, agrupados por era, com personagens relacionados e filtros (era, personagem). |

### 4.11 Comparação — P2
| ID | Requisito |
|---|---|
| RF-COMP-01 | Usuário escolhe dois personagens e vê elemento, habilidades, informações e atributos lado a lado, com comparação visual. |
| RF-COMP-02 | Atributos vêm do banco e são exibidos com aviso de que são estimativas não canônicas. |

### 4.12 Futuro — P3
| ID | Requisito |
|---|---|
| RF-ELTEST-01 | Teste de descoberta do elemento do usuário (perguntas com pontuação por elemento); resultado salvo no perfil. A arquitetura já deve permitir (`User.ElementId`). |
| RF-RANK-01 | Ranking por XP (global, mensal, por elemento). Não implementar antes de P0–P2. |
| RF-SOCIAL-01 | Funcionalidades sociais. |

## 5. Regras de negócio

### 5.1 Gerais

| ID | Regra |
|---|---|
| RN-01 | Email e nome de usuário são únicos (comparação sem diferenciar maiúsculas/minúsculas). |
| RN-02 | Senha: mínimo de 8 caracteres, com letra e número. Confirmação deve ser igual. |
| RN-11 | Dados subjetivos de comparação nunca são apresentados como canônicos. |

### 5.2 Gamificação

**Tabela oficial de XP**

| Evento | XP | Quando | `ExperienceTransaction.Type` |
|---|---|---|---|
| Descobrir personagem | +10 | 1ª descoberta de cada personagem por usuário | `CharacterDiscovered` |
| Descobrir nação | +10 | 1ª descoberta de cada nação por usuário (**DA-01**) | `NationDiscovered` |
| Completar quiz | +20 | 1ª conclusão de cada quiz | `QuizCompleted` |
| Quiz perfeito | +50 (bônus) | 1ª vez que gabarita aquele quiz | `QuizPerfect` |
| Completar desafio | valor do desafio (padrão +30) | conclusão automática | `ChallengeCompleted` |
| Conquista | variável (`ExperienceReward`) | desbloqueio | `AchievementUnlocked` |

| ID | Regra |
|---|---|
| RN-03 | XP só é concedido pelo backend, a partir de eventos de domínio. O cliente nunca informa valores de XP. |
| RN-04 | Os valores de XP são os da tabela oficial acima, definidos no backend em um único lugar (`ProgressionService` / configuração de recompensas). |
| RN-05 | Uma mesma recompensa nunca é concedida duas vezes (descoberta repetida, quiz refeito, conquista, desafio). Garantido por constraint única no banco, não apenas por verificação na aplicação. |
| RN-06 | XP de quiz é concedido apenas na **primeira conclusão** de cada quiz; o bônus de perfeito, na **primeira vez** em que o usuário gabarita aquele quiz. Novas tentativas são registradas no histórico sem XP. *(decisão D7)* |
| RN-07 | Nível derivado do XP: `XP mínimo do nível n = 25 × (n − 1) × (n + 2)` → 1: 0 · 2: 100 · 3: 250 · 4: 450 · 5: 700 · … · 10: 2.700. Fórmula isolada em um serviço para poder mudar. |
| RN-08 | Conquistas são avaliadas pelo backend após cada evento de progresso; o XP da conquista não dispara nova avaliação em cascata. |
| RN-09 | Respostas de quiz são validadas apenas no backend; `IsCorrect` só aparece no resultado após a submissão. Cada pergunta tem exatamente uma opção correta. |
| RN-10 | Desafios diários são definidos por dia em UTC; o progresso é atualizado pelo backend a partir dos eventos e a conclusão concede XP automaticamente. |
| RN-12 | **Descoberta (personagem e nação, DA-01):** (a) só para usuário autenticado; (b) o frontend registra a descoberta **somente depois que o conteúdo principal da página foi carregado com sucesso e exibido**, nunca em erro, 404 ou loading; (c) o backend valida autenticação e existência do recurso (404 caso contrário); (d) o registro é idempotente: a PK composta (`UserId`, `NationId`/`CharacterId`) e a unique de `ExperienceTransaction` impedem duplicidade mesmo com requisições simultâneas, e uma violação de unicidade é tratada como "já descoberto" (`isNew: false`, sem XP, sem erro); (e) o frontend evita disparos duplicados (um envio por montagem da página, seguro em StrictMode), mas a garantia é do backend. |

## 6. Requisitos não funcionais

| Categoria | Requisito |
|---|---|
| Segurança | Hash de senha (PBKDF2 via `PasswordHasher`), JWT, autorização em endpoints privados, validação de entrada no backend, CORS restrito à origem do frontend, sem dados sensíveis nas respostas, sem stack trace para o cliente, secrets fora do Git (`.env.example`). |
| Validação | Backend valida campos obrigatórios, formato de email, senha, IDs, regras de negócio, duplicidades e permissões. Frontend dá feedback amigável, mas nunca é a única validação. |
| Erros | Tratamento global de exceções com formato padronizado (`api.md`). |
| Responsividade | Desktop, tablet e mobile, planejada mobile-first desde o início. |
| Acessibilidade | Contraste WCAG AA (regras de uso da paleta em `ui-ux.md` §2), labels, navegação por teclado, `alt` em imagens, botões semânticos, foco visível, `aria-label` quando necessário. |
| Performance | Paginação, lazy loading de rotas e imagens, imagens otimizadas, cache de requisições, sem chamadas duplicadas. Sem otimização prematura. |
| Testabilidade | Testes unitários, de integração, de frontend e E2E nas partes importantes (ver `architecture.md` §8). |
| Manutenibilidade | Conteúdo no banco; novos personagens, habilidades, eventos, quizzes e conquistas sem alterar código. |
| Setup | Ambiente sobe do zero (PostgreSQL local ou via Docker) com migrations + seed aplicados pela API, já com dados de demonstração. |
| Documentação | README e `docs/` permitem que outro desenvolvedor entenda e execute o projeto. |

## 7. Direitos autorais

- Projeto educacional/de portfólio, sem uso comercial.
- Imagens e textos devem ser fáceis de substituir: cada imagem guarda URL, URL de origem e crédito (ver `database.md`, tipo `ImageInfo`).
- README deve conter aviso de que Avatar é propriedade de seus detentores.
- A identidade visual (paleta e tipografia, DA-02/DA-03) é **inspirada** nos elementos do anime, não uma reprodução oficial.

## 8. Fora do escopo da v1

- Painel administrativo de conteúdo.
- Ranking e funcionalidades sociais (P3).
- Recuperação de senha por email.
- Upload de avatar (v1 usa URL ou avatares pré-definidos).
- Internacionalização (UI em português).

## 9. Status de implementação (v1)

Todos os requisitos P0, P1 e P2 estão implementados, com as exceções e ajustes abaixo. Os P3 (§4.12) não foram iniciados.

| Requisito | Situação |
|---|---|
| RF-CHAR-02 / RF-CHAR-03 (imagem do personagem) | 12 dos 37 personagens têm retrato; os demais usam o placeholder próprio (DA-04). Adicionar imagens não exige código (`database.md` §4). |
| RF-ELEM-01 (elementos e subtipos) | Disponível na API (`GET /api/elements`) e usado na Home, nos filtros e nos badges; não há uma página só de elementos. |
| RF-ABIL-01 (habilidades e quem as possui) | Disponível na API (`GET /api/abilities`, `GET /api/characters/{id}/abilities`); na interface, as habilidades aparecem no detalhe e na comparação de personagens. Não há página de habilidades com a lista de quem as possui. |
| RF-PROF-01 (avatar e elemento no perfil) | O perfil mostra avatar e elemento quando existem, mas não há edição de perfil na v1 (`avatarUrl` e `elementId` ficam nulos). A escolha do elemento fica para o teste de elemento (RF-ELTEST-01, P3). |
| RF-CHAL-01 ("Explore uma nação") | Coberto pelo tipo de desafio `DiscoverNations`. |
| `POST /api/challenges/{id}/complete` da especificação | Removido de propósito (D9): a conclusão é automática no backend. |
