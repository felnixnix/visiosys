# ADR-014: Testcontainers para Testes de Integração (TDD)

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF07 (implícito — qualidade de código)

---

## Contexto

TDD é mandatório no projeto. Testes unitários cobrem a lógica de domínio (entidades, use cases com fakes) mas não provam que o repositório EF Core gera SQL correto, que as migrations são válidas, ou que a concorrência otimista via `xmin` funciona de fato. Bancos in-memory (SQLite in-memory, EF Core InMemory) têm comportamento diferente do PostgreSQL real — o que passou no teste pode falhar em produção.

---

## Decisão

Usar **Testcontainers** para subir instâncias reais de **PostgreSQL** e **MongoDB** em containers Docker durante os testes de integração, descartados ao final de cada suite.

Estrutura adotada:
- Projeto separado `Visiosys.Integration.Tests` (xUnit + Testcontainers).
- `IAsyncLifetime` para controlar o ciclo de vida dos containers por classe de teste.
- `PostgreSqlBuilder` e `MongoDbBuilder` com imagens fixas (`postgres:16`, `mongo:7`).
- Migration aplicada programaticamente antes dos testes via `dbContext.Database.MigrateAsync()`.

Testes cobertos:
- `PrecatorioRepositoryTests`: persistência, busca, concorrência otimista (`DbUpdateConcurrencyException`).
- `MongoAuditLogServiceTests`: inserção de documento de auditoria, tolerância a falha.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **EF Core InMemory Provider** | Não suporta SQL real, sem validação de FK, sem transações reais, sem `xmin` — falsos positivos frequentes |
| **SQLite in-memory** | Mais próximo do SQL real, mas divergências com PostgreSQL (tipos, concorrência, arrays) criam falsos negativos |
| **Banco de dados compartilhado de testes** | Estado compartilhado entre testes gera flakiness; ordem de execução importa; sem isolamento |
| **Mocks de repositório apenas** | Fakes funcionam para use cases, mas não provam que o repositório EF Core está correto |

---

## Consequências

**Positivas:**
- Confiança máxima: o teste roda contra o mesmo banco que a produção usa.
- Concorrência otimista testada de verdade (cenário com dois DbContext concorrentes).
- Migrations validadas automaticamente em cada execução do CI.

**Negativas / Trade-offs:**
- Testes de integração são mais lentos que unitários (~5–15s para subir containers).
- Requerem Docker instalado no ambiente de CI (runners GitHub Actions têm Docker — coberto em `ci.yml`).
- `IAsyncLifetime` adiciona boilerplate por classe de teste.
