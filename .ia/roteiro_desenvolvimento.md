# Roteiro de Desenvolvimento

**Projeto:** Sistema de Gestão e Assessoria de Precatórios  
**Autor:** Felipe de Araújo  
**Função:** Software developer  
**Data:** Junho de 2026  

---

## 1. Princípio Orientador

A sequência de desenvolvimento é **guiada por dependências técnicas e por risco**, e não pela ordem numérica dos requisitos funcionais.

A estratégia central é construir primeiro um **"esqueleto que anda" (walking skeleton)**: uma fatia vertical fina que atravessa toda a pilha tecnológica (HTTP → Controller → Caso de Uso → Repositório → PostgreSQL → resposta). Isto elimina cedo o risco de integração (EF Core, Testcontainers, Docker e Concorrência Otimista), em vez de o descobrir tardiamente com múltiplas entidades já construídas em isolamento.

Cada nova funcionalidade entra depois por um caminho já pavimentado e coberto por testes.

---

## 2. Estado Atual (Concluído)

**Todas as fases (0 a 5) do roteiro foram concluídas. A aplicação está provisionada e em execução na AWS.**

Backend e domínio:
* Solução .NET 8 em camadas (DDD / Clean Architecture): Domain, Application, Infrastructure, API e Worker.
* Domínio rico: `Precatorio` (criação, atualização de valor, cálculo de deságio, transição de status), `Cliente`, `Documento`, `Andamento`, `Pagamento` — com associação Cliente↔Precatório.
* Persistência poliglota: PostgreSQL (EF Core, concorrência otimista via `xmin`), MongoDB (auditoria LGPD) e S3 (documentos).
* Autenticação JWT + rate limiting nativo, Health Checks (`/health`), logging estruturado (Serilog) e Worker de captura com resiliência HTTP.
* Busca e filtros server-side nas listagens (precatórios por número/tribunal/esfera/status/natureza; clientes por nome/documento/tipo/inicial), com a contagem de paginação usando o mesmo filtro.
* Suíte de testes xUnit (domínio + integração com Testcontainers) verde no CI.

Frontend:
* SPA em React + Vite + TypeScript consumindo a API, servido pela própria API (`wwwroot`) atrás do nginx.

Infraestrutura e produção (provisionada via Terraform, em `sa-east-1`):
* VPC, EC2 ARM `t4g.medium` (Elastic IP), RDS PostgreSQL gerenciado, buckets S3 (documentos + artefatos de deploy), IAM e Security Groups.
* Bootstrap da instância (`infra/scripts/user_data.sh`): .NET runtime, MongoDB 8, nginx (reverse proxy), AWS CLI e serviços systemd (`visiosys-api`, `visiosys-worker`).
* **Deploy automatizado via AWS SSM + OIDC** (ver [ADR-021](../docs/adr/ADR-021-deploy-ssm-oidc.md)) — sem porta SSH aberta para o CI; migrations aplicadas no startup.
* **Domínio público com HTTPS:** `https://felipedearaujo.dev/visiosys`, roteamento por path com `UsePathBase` + certificado Let's Encrypt via certbot (ver [ADR-023](../docs/adr/ADR-023-dominio-felipedearaujo-dev.md)) — fecha a pendência de HTTPS da Fase 5.
* 28 ADRs documentando as decisões arquiteturais em `docs/adr/`.
* **Correções pós-deploy validadas em produção:** serialização de enums como string (`JsonStringEnumConverter`, alinhado ao contrato esperado pelo frontend) e normalização de datas para UTC em `RegistrarPagamentoUseCase` (PostgreSQL `timestamptz` exige `DateTimeKind.Utc`).
* **Seeder de dados mock** (`infra/scripts/seed.py`): popula clientes, precatórios, andamentos e pagamentos via API real (não INSERT direto), com CPF/CNPJ válidos e idempotência — usado para validar o comportamento real da plataforma no ambiente de estudos. Execução: `scp` para a instância + `sudo python3 seed.py` (lê credenciais admin de `/etc/visiosys/production.env`).

Governança:
* `.gitignore` padrão .NET, segredos fora do repositório e governança de IA em `.ia/`.
* **Documentação viva** (ver [ai_rules.md](ai_rules.md), seção 4): toda mudança de arquitetura, infraestrutura ou comportamento observável da API atualiza a documentação na mesma entrega.

---

## 3. Fases de Desenvolvimento

### Fase 0 — Esqueleto que Anda (de-risk da infraestrutura)
> Objetivo: provar que o fluxo `HTTP → Controller → Caso de Uso → Repositório → PostgreSQL → resposta` funciona, validado por teste de integração real.

| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 1 | **CI no GitHub Actions** (`build` + `test`) — impõe a disciplina TDD pela máquina desde o início. | RNF07 |
| 2 | **Docker Compose local** — PostgreSQL + MongoDB + Seq. | RNF03, RNF04, RNF08 |
| 3 | **EF Core na Infrastructure** — `DbContext`, mapeamento do `Precatorio` com `RowVersion` e migration inicial. | RNF03, RNF15 |
| 4 | **Teste de integração com Testcontainers** — persistência real + cenário que dispara `DbUpdateConcurrencyException`. | RNF15 |
| 5 | **Primeiro endpoint vertical na API** — `POST /precatorios` e `GET /precatorios/{id}` com Swagger ativo. Nasce a camada Application (caso de uso + DTOs). | RF02, RNF01, RNF10 |

### Fase 1 — Preocupações Transversais
> São cross-cutting: retrofitá-las com muitos endpoints prontos é doloroso. Adicionar com 1-2 endpoints.

| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 6 | **Autenticação JWT + Identity** e **Rate Limiting nativo** no fluxo de login. | RNF09, RNF17 |
| 7 | **Logging estruturado → Seq**, **Health Checks (`/health`)** e middleware global de tratamento de erros. | RNF08, RNF13 |

### Fase 2 — Largura do Domínio
> O padrão arquitetural já está provado; cada entidade entra por caminho pavimentado.

| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 8 | **Cliente** — credor, contatos e dados bancários. | RF01 |
| 9 | **Documentos + AWS S3** — apenas chave/URL persistida no PostgreSQL. | RF04, RNF05 |
| 10 | **Andamentos + Auditoria → MongoDB** — trilha LGPD de leitura/alteração de dados sensíveis. | RF05, RNF04, RNF12 |
| 11 | **Controle de Pagamentos** e refino do **Cálculo de Deságio**. | RF03, RF06 |

### Fase 3 — Worker e Integração Externa
| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 12 | **Worker de captura automática** com `Microsoft.Extensions.Http.Resilience` (retry, backoff exponencial, circuit breaker). | RF08 |

### Fase 4 — Frontend
| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 13 | **SPA em React** consumindo a API REST, com página de Ajuda (`/ajuda`) e link para o Swagger, protegido por Basic Auth em produção (ver [ADR-022](../docs/adr/ADR-022-swagger-producao-basic-auth.md)). | RF07, RNF02, RNF10 |

### Fase 5 — Produção e Infraestrutura como Código
| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 14 | **Terraform** (EC2 ARM `t4g.medium`, RDS, S3, Route 53) e **deploy via AWS SSM + OIDC** (ver [ADR-021](../docs/adr/ADR-021-deploy-ssm-oidc.md)). `terraform apply` apenas com aprovação humana explícita. **HTTPS via domínio próprio e certbot concluído** (ver [ADR-023](../docs/adr/ADR-023-dominio-felipedearaujo-dev.md)). Pendente: rotinas de backup/DR. | RNF06, RNF11, RNF14, RNF16 |

---

## 4. Oportunidades de Paralelização

* O **Frontend (Fase 4)** pode iniciar assim que o Swagger das Fases 0/1 existir.
* Os scripts **Terraform (Fase 5)** podem ser escritos em paralelo a partir da Fase 2.

---

## 5. Decisões Estruturais que Não Podem Ser Adiadas

* **Concorrência Otimista (`RowVersion`):** deve entrar no mapeamento do EF Core já na primeira migration. Adicionar depois exige nova migration e revisão de todos os fluxos de escrita.
* **Autenticação (JWT):** introduzir cedo (Fase 1) para evitar securizar dezenas de endpoints retroativamente.
* **Auditoria LGPD (MongoDB):** o registo de acessos a dados sensíveis deve ser desenhado junto com o primeiro fluxo de leitura de dados financeiros.
