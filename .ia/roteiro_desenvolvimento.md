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

* Solução .NET 8 estruturada em camadas (DDD / Clean Architecture).
* Entidade rica `Precatorio` com regras de negócio (criação, atualização de valor, cálculo de deságio, transição de status).
* Enums de domínio: `EsferaPrecatorio`, `NaturezaPrecatorio`, `StatusPrecatorio`.
* Suíte de 11 testes unitários no xUnit (ciclo TDD verde).
* `.gitignore` padrão .NET e governança de IA em `.ia/`.

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
| 13 | **SPA em React** consumindo a API REST, com portal de ajuda e link para o Swagger. | RF07, RNF02 |

### Fase 5 — Produção e Infraestrutura como Código
| Passo | Entrega | Requisitos |
| :--- | :--- | :--- |
| 14 | **Terraform** (EC2 ARM `t4g.medium`, S3, Route 53), deploy via GitHub Actions e rotinas de backup/DR. `terraform apply` apenas com aprovação humana explícita. | RNF06, RNF11, RNF14, RNF16 |

---

## 4. Oportunidades de Paralelização

* O **Frontend (Fase 4)** pode iniciar assim que o Swagger das Fases 0/1 existir.
* Os scripts **Terraform (Fase 5)** podem ser escritos em paralelo a partir da Fase 2.

---

## 5. Decisões Estruturais que Não Podem Ser Adiadas

* **Concorrência Otimista (`RowVersion`):** deve entrar no mapeamento do EF Core já na primeira migration. Adicionar depois exige nova migration e revisão de todos os fluxos de escrita.
* **Autenticação (JWT):** introduzir cedo (Fase 1) para evitar securizar dezenas de endpoints retroativamente.
* **Auditoria LGPD (MongoDB):** o registo de acessos a dados sensíveis deve ser desenhado junto com o primeiro fluxo de leitura de dados financeiros.
