# ADR-004: Entity Framework Core com Mapeamento Fluente

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF03

---

## Contexto

O domínio usa modelos ricos com `private set`, value objects como `DadosBancarios`, e entidades com regras de criação encapsuladas em factory methods. O ORM precisa ser capaz de materializar essas entidades sem violar o encapsulamento (sem exigir setters públicos ou construtores paramétricos públicos).

---

## Decisão

Usar **Entity Framework Core 8** com **Mapeamento Fluente** via `IEntityTypeConfiguration<T>` em classes de configuração separadas por entidade (`PrecatorioConfiguration`, `ClienteConfiguration`, etc.).

Decisões específicas de mapeamento:

- **`OwnsOne()`** para value objects (`DadosBancarios`) — mantém o objeto na mesma tabela sem tabela auxiliar.
- **`UseXminAsConcurrencyToken()`** via Npgsql — mapeia a coluna de sistema `xmin` do PostgreSQL como token de concorrência (ver ADR-005).
- **Convenção de nomenclatura snake_case** para colunas — padrão PostgreSQL, sem depender de convenção automática do EF.
- **`HasColumnType("numeric(18,2)")`** explícito para valores monetários — sem arredondamento inesperado de `float`/`double`.
- **Migrations versionadas** no repositório — histórico auditável de alterações de schema.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Data Annotations** (`[Column]`, `[Required]`) nas entidades | Polui o domínio com dependências de infraestrutura; viola a separação Domain/Infrastructure |
| **Dapper** (micro-ORM, SQL puro) | Mais controle de SQL, mas perde migrations automáticas, change tracking e concorrência otimista integrada |
| **NHibernate** | Maduro mas verboso; ecossistema .NET moderno convergiu para EF Core; menor comunidade ativa |

---

## Consequências

**Positivas:**
- Migrations como código-fonte: qualquer alteração de schema é rastreável no Git.
- Change tracking nativo: EF Core detecta alterações sem código extra.
- Value objects mapeados nativamente com `OwnsOne` sem tabela extra.

**Negativas / Trade-offs:**
- EF Core gera SQL subótimo em queries complexas; nesses casos, `FromSqlRaw` ou Dapper pode ser usado pontualmente.
- Migrations automáticas podem gerar scripts destrutivos em alterações de schema — revisão obrigatória antes de aplicar em produção.
