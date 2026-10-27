# UI/UX

## 1. Direção visual

Identidade própria **inspirada** no universo de Avatar, sem copiar a interface de nenhuma obra. Deve parecer um produto profissional.

- Elegante, moderna, imersiva e responsiva.
- **Dark mode como tema principal** (light mode opcional no futuro).
- Cores dos quatro elementos usadas como **acento**, não como fundo dominante. Não exagerar.
- Animações sutis, cards com hover, transições suaves, backgrounds atmosféricos (gradientes/névoa discretos), efeitos de profundidade (sombras, camadas, blur leve).
- A paleta é **inspirada** nos elementos do anime, não uma reprodução oficial.

## 2. Paleta (DA-02 — aprovada)

Definida em `src/styles/tokens.css` como CSS custom properties.

### 2.1 Cores oficiais

| Token | Valor | Uso |
|---|---|---|
| `--color-bg` | `#101820` | fundo principal |
| `--color-surface` | `#1B2835` | cards, painéis, navbar |
| `--color-water` | `#3186B5` | Água: azul |
| `--color-fire` | `#B86B35` | Fogo: laranja queimado |
| `--color-earth` | `#638B58` | Terra: verde |
| `--color-air` | `#D5C7A2` | Ar: areia clara |

### 2.2 Cores de apoio (aprovadas em 2026-10-26)

Neutros derivados do fundo oficial, para completar a interface.

| Token | Valor | Uso | Contraste sobre `surface` |
|---|---|---|---|
| `--color-surface-raised` | `#243445` | hover, modais, dropdowns | — |
| `--color-border` | `#2E4052` | bordas e divisores | — |
| `--color-text` | `#E8EDF2` | texto principal | 12,7:1 |
| `--color-text-muted` | `#9FB0C0` | texto secundário | 6,7:1 |
| `--color-success` | `#5FC28A` | feedback positivo | ≥ 4,5:1 |
| `--color-danger` | `#F07167` | erros | ≥ 4,5:1 |
| `--color-warning` | `#F0A35E` | avisos | ≥ 4,5:1 |

> Todos os pares texto/fundo são verificados automaticamente por `src/styles/tokens.test.ts`.

> O antigo acento dourado (`#E0B354`) **foi removido**: ficaria visualmente próximo do Ar (`#D5C7A2`) e criaria um quinto acento competindo com os elementos. Botões primários usam fundo neutro claro (`--color-text`) com texto em `--color-bg`. A barra de XP usa a cor do elemento do usuário, ou o neutro claro se ele ainda não tiver elemento.

### 2.3 Regras de uso e contraste (WCAG AA)

Contraste medido para as cores oficiais:

| Cor | sobre `bg` `#101820` | sobre `surface` `#1B2835` |
|---|---|---|
| Água `#3186B5` | 4,44:1 | 3,72:1 |
| Fogo `#B86B35` | 4,42:1 | 3,70:1 |
| Terra `#638B58` | 4,57:1 | 3,83:1 |
| Ar `#D5C7A2` | 10,67:1 | 8,93:1 |

Consequências:

- **Água, Fogo e Terra não são usadas como cor de texto pequeno** (< 18,66px bold / 24px regular): ficam abaixo de 4,5:1. Elas servem para **bordas, ícones, indicadores, barras de progresso, foco, fundos translúcidos e títulos grandes**, onde o mínimo é 3:1 e todas passam.
- **Ar** pode ser usada como texto em qualquer tamanho.
- Badges de elemento: fundo com a cor em baixa opacidade (~15–20%), borda na cor sólida e **texto em `--color-text`**, sempre acompanhado de ícone/nome do elemento (nunca só cor).
- Texto branco sobre as cores sólidas também fica abaixo de 4,5:1 (≈ 3,9–4,0:1), por isso evitar botões com fundo sólido de elemento e texto pequeno.
- Os componentes recebem a cor do elemento que vem da API (campo `color`) na variável `--element-color`. Cards, badges e bordas se adaptam sem lógica extra. Sem elemento (não dobradores), o acento é o neutro `--color-text-muted`.
- **Imagens de personagem (DA-04):** até a curadoria da Fase 15, o `CharacterAvatar` mostra as iniciais sobre um círculo com borda e brilho na cor do elemento. Quando houver `image`, a imagem real substitui o placeholder sem mudança nas páginas.

## 3. Tipografia (DA-03 — aprovada)

| Fonte | Uso | Pesos |
|---|---|---|
| **Cinzel** | Títulos de página (H1/H2), hero da Home, nome do personagem no detalhe, títulos de eras/eventos na timeline e destaques narrativos (modal de novo nível, conquista desbloqueada) | 600, 700 |
| **Inter** | Todo o resto: textos corridos, navegação, formulários, botões, labels, badges, tabelas, cards, toasts, números/estatísticas | 400, 500, 600 |

- **Cinzel com moderação**: no máximo um ou dois elementos por tela. Nunca em botões, inputs, menus, parágrafos ou textos pequenos (< 20px).
- Escala modular: 12 / 14 / 16 / 20 / 24 / 32 / 40px; corpo em 16px; altura de linha 1,5 (texto) e 1,2 (títulos).
- Fontes servidas localmente via `@fontsource` (sem dependência de CDN), com `font-display: swap` e fallback `serif` (Cinzel) / `system-ui, sans-serif` (Inter).

## 4. Outros tokens

- Espaçamento em escala de 4px; raio de borda 8–16px; sombras em 3 níveis (escurecidas sobre `--color-bg`).

## 5. Responsividade (mobile-first)

| Breakpoint | Largura | Layout |
|---|---|---|
| base | < 640px | 1 coluna, navegação inferior ou menu hambúrguer, cards em lista/grid de 2 |
| `sm` | ≥ 640px | grid de 2–3 colunas |
| `md` | ≥ 768px | tablet, grid de 3, filtros em drawer |
| `lg` | ≥ 1024px | desktop, filtros em sidebar, grid de 4 |
| `xl` | ≥ 1280px | navbar completa (abaixo disso, menu recolhido; ✅ Fase 9), conteúdo com largura máxima (~1200px) |

As telas são desenhadas primeiro para mobile e expandidas. Não vale fazer o desktop e só "quebrar" para mobile.

## 6. Rotas do frontend

| Rota | Página | Acesso |
|---|---|---|
| `/` | Home (landing: proposta, destaques, CTA de cadastro) | público |
| `/login` | Login | apenas deslogado |
| `/register` | Cadastro | apenas deslogado |
| `/dashboard` | Dashboard personalizado (destino após login) | 🔒 |
| `/characters` | Listagem com busca/filtros/ordenação/paginação (estado na URL) | público |
| `/characters/:id` | Detalhe do personagem | público (descoberta/favorito se logado) |
| `/nations` | Nações | público |
| `/nations/:id` | Detalhe da nação | público |
| `/timeline` | Linha do tempo | público |
| `/compare` | Comparação (`?a=1&b=2`) | público |
| `/quizzes` | Lista de quizzes | público |
| `/quizzes/:id` | Jogar quiz + resultado | 🔒 |
| `/quizzes/history` | Histórico de quizzes | 🔒 |
| `/profile` | Perfil e estatísticas (progresso) | 🔒 |
| `/favorites` | Favoritos | 🔒 |
| `/achievements` | Conquistas | 🔒 |
| `/challenges` | Desafios diários e histórico | 🔒 |
| `/element-test` | Teste de elemento (P3) | 🔒 |
| `*` | NotFound | público |

Rotas 🔒 redirecionam para `/login?redirect=<rota>` e voltam após o login.

## 7. Páginas

### Home
Hero atmosférico com os quatro elementos, proposta de valor, personagens em destaque, chamada para explorar e para criar conta.

### Dashboard
```text
Olá, Matheus 👋
Continue sua jornada.

Nível 12   ███████████░░░  1.240 / 1.500 XP

DESAFIOS DE HOJE            ÚLTIMAS CONQUISTAS
[ ] Descubra 3 personagens  🏅 Curioso
[ ] Complete um quiz

CONTINUE EXPLORANDO         QUIZ EM DESTAQUE
Zuko · Toph · Korra         [card]
```
Também mostra recomendações (personagens ainda não descobertos) e favoritos.

**Implementado (Fase 10):** hero com saudação e XpBar; 4 indicadores clicáveis (personagens, nações, conquistas, quizzes); em ≥ 1024px, "Quiz em destaque" e "Conquistas" (recentes + próxima) lado a lado; depois "Continue explorando", "Descobertas recentes" e "Favoritos". Seções vazias usam uma mensagem curta com borda tracejada. Na navbar, o link "Início" vira "Painel" para quem está logado. ✅ Fase 11: seção "Desafios de hoje" logo depois dos indicadores.

### Listagem de personagens
Grid de `CharacterCard` (imagem, nome, badge de elemento, nação, indicador "descoberto", botão de favorito). Busca com debounce, filtros (nação, elemento, tipo, status, série), ordenação e paginação numérica sincronizada com a URL. Em mobile/tablet, os filtros ficam num painel recolhível aberto pelo botão "Filtros" e os cards viram itens de lista; a partir de 1024px, os filtros ficam numa sidebar fixa.

### Detalhe do personagem
Imagem grande sobre um gradiente atmosférico sutil na cor do elemento, nome, apelidos, nação, elemento, subtipos de dobra, descrição, biografia, habilidades (cards), aparições/séries, relações e relacionados. Botão `☆ Favoritar` que vira `★ Favoritado` (atualização otimista com rollback em erro). Na primeira visita de um usuário logado aparece o toast de descoberta (registrada só depois que o conteúdo principal foi exibido; ver RN-12).

### Nações
Cards com símbolo e cor do elemento como acento; detalhe com descrição e personagens. Na primeira visita de um usuário logado ao detalhe, depois que o conteúdo principal foi exibido, a descoberta é registrada: `✨ NOVA DESCOBERTA — Você descobriu a Nação do Fogo! +10 XP` (DA-01, RN-12).

### Timeline
Linha vertical (horizontal em telas largas) agrupada por era, com marcadores, cards de evento, avatares dos personagens relacionados e filtros por era/personagem. A animação de entrada ao rolar deve ser sutil e respeitar `prefers-reduced-motion`.

### Comparação
Dois seletores com busca. Colunas lado a lado: imagem, elemento, nação, habilidades, informações, e **gráfico de barras ou radar** com os atributos. Aviso fixo: *"Atributos são estimativas da comunidade do projeto, não informações canônicas."*

### Quiz
Uma pergunta por vez, com barra de progresso, opções como botões grandes e acessíveis, e possibilidade de voltar antes de enviar. Ao submeter: tela de resultado com pontuação, %, correção pergunta a pergunta (certa/errada + explicação), XP ganho e eventuais conquistas.

✅ Fase 9:
- **Lista** (`/quizzes`): cards com categoria, dificuldade (texto + barrinha de 1 a 3 níveis), nº de perguntas e "Melhor: 8/10 (80%)" para quem já jogou; filtros de categoria e dificuldade na URL; "Meu histórico" para quem está logado.
- **Jogo** (`/quizzes/:id`): cartão com "Pergunta n de 10" + barra, enunciado e opções A–D. As opções são rádios nativos estilizados (setas do teclado, leitor de tela, foco visível); ao trocar de pergunta o foco vai para o enunciado. "Próxima" só com a pergunta respondida; "Enviar respostas" só com todas. Se o envio falhar, as respostas ficam.
- **Resultado**: título conforme o desempenho ("Gabaritou!", "Mandou bem!"…), "9 / 10" + %, XP do quiz ou o aviso de que a tentativa não rende XP, conquistas desbloqueadas, e a correção com borda verde/vermelha, "Sua resposta", "Resposta correta" e a explicação. Ações: jogar novamente, ver histórico, outros quizzes.
- **Histórico** (`/quizzes/history`): lista com quiz, data, pontuação, % e XP ("Sem XP" nas tentativas repetidas).

### Perfil
```text
MATHEUS
🌪️ Ar            Nível 12
1.240 / 1.500 XP  ████████████████░░░░

Personagens descobertos  82 / 150
Quizzes                  47
Taxa de acerto           87%
Conquistas               14 / 30
```
Inclui abas/seções para descobertas, favoritos, conquistas e histórico de XP.

### Conquistas
Grid com conquistas desbloqueadas (coloridas, com data) e bloqueadas (esmaecidas, com barra de progresso).

✅ Fase 8: resumo "x de y conquistas desbloqueadas" com barra; cards na ordem do catálogo (cadastro → descobertas → quizzes → desafios). Desbloqueada: ícone sobre círculo com o acento do Ar, borda clara e "Desbloqueada em dd/mm/aaaa". Bloqueada: só o ícone fica esmaecido (com cadeado), os textos mantêm o contraste, e o progresso aparece como "3 / 10" + barra. Acesso pelo troféu na navbar e por "Ver conquistas" no perfil.

### Desafios
Cards dos desafios do dia com progresso, recompensa e horário do próximo reset (UTC convertido para o horário local).

✅ Fase 11: `/challenges` com "Desafios de hoje" ("x de 3 concluídos" + "Novos desafios às HH:mm") e "Concluídos" (histórico paginado). Card com ícone do tipo, título, XP, descrição e barra de progresso ("2 / 3", ou "60% de 80%" no desafio de porcentagem); concluído tem borda e ícone verdes e "Concluído" no lugar da barra. Atalho por um ícone de alvo na área do usuário da navbar e por "Ver desafios" no painel. A conclusão é anunciada por um toast "Desafio concluído — título! +XP" vindo do `reward` de qualquer evento.

## 8. Componentes base (`components/common`)

`Button`, `IconButton`, `Card`, `Badge`/`ElementBadge`, `Avatar`, `Input`, `Select`, `SearchInput`, `Modal`, `Drawer`, `Tabs`, `Toast`, `Spinner`, `Skeleton`, `ProgressBar`/`XpBar`, `EmptyState`, `ErrorState`, `Pagination`, `Tooltip`.

## 9. Estados obrigatórios em toda tela

| Estado | Padrão |
|---|---|
| **Loading** | Skeletons no formato do conteúdo (spinner só para ações pontuais) |
| **Empty** | Ilustração/ícone + texto + ação. Ex.: *"Você ainda não possui favoritos. Explore os personagens e adicione seus favoritos."* |
| **Error** | Mensagem amigável + botão "Tentar novamente"; nunca mensagem técnica |
| **Sucesso** | Toast curto. Ex.: *"✓ Personagem adicionado aos favoritos."* |

## 10. Feedback de gamificação

Disparados a partir do `reward` retornado pela API (nunca calculados no frontend):

- **Descoberta** (personagem ou nação): `✨ NOVA DESCOBERTA — Você descobriu Zuko! +10 XP` (toast). Só aparece quando a API retorna `isNew: true`.
- **XP**: animação curta na `XpBar` (a barra cresce e um brilho a atravessa). A `XpBar` aparece completa no perfil e compacta na navbar, sob o nome do usuário; usa a cor do elemento do usuário ou o neutro claro (DA-02). ✅ Fase 7
- **Nível**: modal `Novo nível! Você alcançou o nível 10. Continue sua jornada!` com `<dialog>` nativo (foco preso, Esc fecha, fundo inerte), foco inicial em "Continuar". Vários níveis entram numa fila e aparecem um de cada vez. ✅ Fase 7
- **Quiz**: toast `🎯 Quiz concluído — 9/10! +20 XP` só quando a tentativa rende XP. ✅ Fase 9
- **Conquista**: toast com troféu, `Conquista desbloqueada — Curioso! +50 XP`, um por conquista, logo depois do toast do evento que a causou (que mostra só o XP do próprio evento). No cadastro, aparece junto da boas-vindas ("Primeiro passo"). ✅ Fase 8
- **Desafio concluído**: toast com recompensa.

Quando chegam vários eventos de uma vez, eles entram numa fila e aparecem em sequência.

## 11. Acessibilidade

- Contraste AA conforme as regras da §2.3; não depender só de cor (ícone/texto junto do elemento).
- Labels em todos os inputs; mensagens de erro associadas (`aria-describedby`).
- Navegação completa por teclado; foco visível e consistente; trap de foco em modais.
- `alt` descritivo em imagens de conteúdo; `alt=""` em decorativas.
- Botões para ações (`<button>`) e links para navegação (`<a>`).
- `aria-label` em botões só com ícone (ex.: favoritar); `aria-pressed` no favorito.
- Toasts em região `aria-live="polite"`.
- Respeitar `prefers-reduced-motion`.

## 12. Animações

Transições de 150–300ms em hover/foco, entrada de cards com fade/slide leve e transição entre rotas discreta. Nada de animações longas, em loop ou chamativas.
