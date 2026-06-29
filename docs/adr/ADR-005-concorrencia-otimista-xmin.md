# ADR-005: Concorrência Otimista via xmin do PostgreSQL

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF15

---

## Contexto

Dois operadores podem abrir o mesmo precatório simultaneamente, editar valores diferentes e tentar salvar. Sem controle de concorrência, a última gravação silenciosamente sobrescreve a anterior (Lost Update), corrompendo dados financeiros — um risco inaceitável para um sistema de gestão de ativos judiciais.

---

## Decisão

Usar **Concorrência Otimista** via **coluna de sistema `xmin` do PostgreSQL**, mapeada pelo Npgsql como token de concorrência no EF Core.

Como funciona:
- O PostgreSQL incrementa automaticamente o `xmin` de uma linha a cada `UPDATE`, sem necessidade de coluna extra.
- O EF Core inclui o valor de `xmin` lido no `WHERE` do `UPDATE`: `WHERE id = @id AND xmin = @xmin_original`.
- Se outro processo já tiver alterado a linha, o `xmin` não bate, o `UPDATE` afeta 0 linhas, e o EF Core lança `DbUpdateConcurrencyException`.
- A aplicação captura essa exceção e retorna HTTP 409 Conflict ao cliente.

Implementação: `builder.UseXminAsConcurrencyToken()` nas configurações de `Precatorio` e `Cliente`.

**Nota técnica:** `UseXminAsConcurrencyToken()` foi marcada obsoleta pelo Npgsql em favor de shadow property com `IsRowVersion()`. No entanto, a abordagem alternativa geraria uma migration que tentaria criar a coluna `xmin` via DDL — o que é inválido, pois `xmin` é uma coluna de sistema do PostgreSQL e não pode ser criada ou removida. A supressão de `CS0618` com `#pragma warning` é a solução correta neste caso específico.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Coluna `RowVersion` / `timestamp`** (padrão SQL Server) | Exige coluna extra em cada tabela; não é idiomático no PostgreSQL |
| **Concorrência Pessimista** (`SELECT FOR UPDATE`) | Bloqueia a linha durante toda a operação; reduz paralelismo; inadequado para um sistema web com múltiplos usuários |
| **Last-Write-Wins** (sem controle) | Silenciosamente perde dados; inaceitável para dados financeiros |

---

## Consequências

**Positivas:**
- Sem coluna extra nas tabelas — o `xmin` é nativo do PostgreSQL.
- Proteção automática: qualquer `SaveChanges()` em entidade com `xmin` está protegida.
- Detectável em teste de integração com Testcontainers (cenário implementado em `PrecatorioRepositoryTests`).

**Negativas / Trade-offs:**
- Específico do PostgreSQL — não portável para SQL Server ou SQLite sem adaptação.
- API precisa tratar `DbUpdateConcurrencyException` e retornar 409 com mensagem clara para o frontend.
