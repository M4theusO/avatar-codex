# Deploy

## 1. Visão geral

Tudo em planos gratuitos, com deploy automático a cada push na `main`.

| | URL |
|---|---|
| Aplicação | https://avatar-codex.vercel.app |
| API (direto no Render) | https://avatar-codex-api.onrender.com/api/health |

```text
navegador ──HTTPS──► Vercel ── arquivos do build (React + imagens)
                        │
                        │  /api/*  (rewrite = proxy reverso)
                        ▼
                     Render ── API .NET em container Docker (backend/Dockerfile)
                        │
                        ▼
                     Neon ── PostgreSQL gerenciado
```

| Peça | Serviço | Configuração versionada |
|---|---|---|
| Frontend | Vercel (projeto com raiz em `frontend/avatar-codex-web`) | `frontend/avatar-codex-web/vercel.json` |
| API | Render, web service Docker no plano free, região Virginia | `render.yaml` (Blueprint) + `backend/Dockerfile` |
| Banco | Neon, plano free, região AWS US East 1 | — (connection string em variável de ambiente do Render) |

## 2. Decisões

| Decisão | Escolha | Por quê |
|---|---|---|
| Frontend e API no mesmo domínio | A Vercel repassa `/api/*` para o Render (rewrite em `vercel.json`) | Para o navegador tudo vem de `*.vercel.app`: o cookie de refresh continua `SameSite=Strict`, não há cookie de terceiros (bloqueado cada vez mais por Safari e Chrome) e o CORS não é necessário em produção. A alternativa (`SameSite=None` + CORS) é mais frágil e enfraquece a proteção contra CSRF |
| Banco no Neon, não no Render | Neon | O PostgreSQL gratuito do Render expira em 30 dias; o do Neon não tem prazo |
| Conexão com o Neon | Host **direto** (sem `-pooler`) para tudo | O Neon recomenda o direto para migrations (o PgBouncer em modo transação não aceita o *advisory lock* do EF Core) e o pooler para cargas serverless ou de uma conexão por requisição. A API é um processo contínuo, de instância única, com o pool do próprio Npgsql, então uma única string direta atende as duas coisas |
| Migrations | No startup da API (`Database:MigrateAndSeedOnStartup = true` em `appsettings.Production.json`) | Uma única instância, então não há corrida entre réplicas; é o mesmo caminho testado em dev e nos testes de integração. O seed é idempotente |
| Usuário demo em produção | Ligado (`Database:SeedDemoUser = true`) | Quem visita o portfólio entra sem se cadastrar. Para desligar: variável `Database__SeedDemoUser=false` no Render |
| HTTPS | Termina na Vercel e no Render; a API lê `X-Forwarded-Proto`/`For` (`ReverseProxyExtensions`) e não usa `UseHttpsRedirection` | O health check do Render chama o container direto em HTTP; um redirecionamento quebraria o health check |
| Cache do proxy | Desligado para `/api/*` (`x-vercel-enable-rewrite-caching: 0`) | Respostas da API dependem do usuário e nunca podem ficar em cache na CDN |
| Regiões | Render em Virginia e Neon em AWS US East 1 | A API faz várias consultas por requisição; API e banco perto um do outro pesa mais que a distância até o usuário (a Vercel já serve o frontend de perto) |
| CI | Fica para depois | Por enquanto, testes, lint e build rodam localmente antes do push (`docs/testing.md`) |

## 3. Configuração de produção

**API (variáveis no Render):**

| Variável | Valor | Origem |
|---|---|---|
| `ConnectionStrings__Default` | connection string do Neon no formato Npgsql (ver §4.1) | informada por você ao criar o Blueprint |
| `Jwt__Secret` | 256 bits aleatórios em base64 | gerada pelo Render (`generateValue`), nunca versionada |
| `PORT` | definida pelo Render | o `Dockerfile` repassa para `ASPNETCORE_HTTP_PORTS` |
| `ASPNETCORE_ENVIRONMENT` | `Production` | fixada no `Dockerfile` |

Em Production: Swagger desligado, erros sem detalhes internos (como em dev), log do EF só com avisos, CORS sem nenhuma origem liberada (não é necessário com o proxy).

**Frontend (Vercel):** nenhuma variável. Sem `VITE_API_URL`, o Axios faz chamadas relativas (`/api/...`), que o rewrite leva até a API.

## 4. Passo a passo (primeira vez)

### 4.1 Banco no Neon

1. Crie uma conta em https://neon.com e um projeto: nome `avatar-codex`, versão mais recente do PostgreSQL, região **AWS US East 1 (N. Virginia)**.
2. Abra **Connect** no painel do projeto e:
   - **desligue "Connection pooling"**: use o host direto, sem `-pooler` no nome. As migrations do EF Core usam um *advisory lock*, que não funciona pelo PgBouncer em modo transação;
   - escolha o formato **.NET** (ou monte à mão): `Host=ep-xxxx.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=<senha>;SSL Mode=Require`.

   O formato de URI (`postgresql://...`) **não** é aceito pelo Npgsql.
3. Guarde essa connection string; ela vai para o Render no próximo passo. Não precisa criar tabelas: a API aplica as migrations e o seed na primeira subida.

### 4.2 API no Render

1. Crie uma conta em https://render.com entrando com o GitHub e autorize o acesso ao repositório `avatar-codex`.
2. **New → Blueprint**, escolha o repositório. O Render lê o `render.yaml` e mostra o serviço `avatar-codex-api`.
3. Cole a connection string do Neon em `ConnectionStrings__Default` e confirme (**Apply**). `Jwt__Secret` é gerada sozinha.
4. O primeiro build leva alguns minutos. Quando o serviço ficar **Live**, abra `https://<serviço>.onrender.com/api/health`: deve responder `{"status":"Healthy","checks":{"database":"Healthy"}}`.
5. Se o Render der ao serviço uma URL diferente de `https://avatar-codex-api.onrender.com` (nome já usado), troque o `destination` em `frontend/avatar-codex-web/vercel.json` e faça push.

### 4.3 Frontend na Vercel

1. Crie uma conta em https://vercel.com entrando com o GitHub.
2. **Add New → Project**, importe o repositório `avatar-codex`.
3. Em **Root Directory**, escolha `frontend/avatar-codex-web`. O preset **Vite** é detectado (build `npm run build`, saída `dist`). Não adicione variáveis de ambiente.
4. **Deploy**. A URL fica algo como `https://avatar-codex.vercel.app` (dá para trocar o nome do projeto em Settings → Domains).

### 4.4 Conferir

- `https://<frontend>/api/health` responde `Healthy` (prova que o proxy chega na API).
- Entrar com o usuário demo, **recarregar a página** e continuar logado (prova que o cookie de refresh passa pelo proxy).
- Abrir um personagem (toast de +10 XP), favoritar, fazer um quiz.
- Abrir um link direto, como `/characters/9`, e recarregar (fallback da SPA).
- `https://<frontend>/swagger` não abre a documentação da API.

## 5. Deploys seguintes

Push na `main`:
- a Vercel reconstrói o frontend (e cria *previews* para outras branches);
- o Render reconstrói a API só quando algo em `backend/` muda (`buildFilter` do `render.yaml`). Migrations novas são aplicadas no startup.

Antes do push, rode a bateria local (`docs/testing.md`), já que ainda não há CI.

**Voltar uma versão:** Render → serviço → **Events** → *Rollback*; Vercel → **Deployments** → *Promote to Production* numa versão anterior. Migrations não são desfeitas no rollback; uma migration que remove dados precisa de cuidado extra.

## 6. Limitações do plano gratuito

| Limitação | Efeito | O que o projeto faz |
|---|---|---|
| Render dorme após 15 min sem requisições | A primeira requisição depois disso leva por volta de 1 minuto | As páginas mostram skeletons enquanto carregam; a Vercel espera até 120 s pela origem, então a requisição não cai |
| Render: 750 horas/mês por conta | Com um único serviço, cobre o mês inteiro | — |
| Neon: computação limitada por mês e suspensão após inatividade | Primeira consulta depois de suspenso leva alguns centésimos de segundo a mais | O consumo do projeto fica muito abaixo do limite |
| Neon: armazenamento limitado por projeto | O banco do projeto tem poucos MB | — |

## 7. Melhorias futuras

- **CI no GitHub Actions** rodando testes, lint e build a cada push (e, se quiser, deploy só com tudo verde: `autoDeployTrigger: checksPass` no `render.yaml`).
- **Restringir a API ao tráfego da Vercel:** hoje `*.onrender.com` também responde direto. A Vercel pode mandar um cabeçalho secreto em cada rewrite, e a API recusaria requisições sem ele.
- **Domínio próprio** apontando para a Vercel.
