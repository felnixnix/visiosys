# ADR-003: Persistência Poliglota (PostgreSQL + MongoDB + S3)

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF03, RNF04, RNF05

---

## Contexto

O sistema lida com três categorias distintas de dados, cada uma com características de acesso e consistência diferentes:

1. **Dados financeiros e relacionais** (precatórios, clientes, pagamentos): exigem transações ACID, integridade referencial e concorrência otimista.
2. **Trilhas de auditoria LGPD** (registros de quem leu ou alterou dados sensíveis): schema dinâmico, escrita intensiva, nunca atualizados, apenas acrescidos.
3. **Arquivos físicos** (PDFs de procurações, certidões, contratos): binários grandes que não pertencem em banco relacional nem em disco local de servidor stateless.

Nenhum banco de dados único atende esses três perfis de forma igualmente eficiente.

---

## Decisão

Usar três tecnologias de persistência, cada uma responsável por uma categoria:

| Categoria | Tecnologia | Justificativa |
|-----------|-----------|---------------|
| Dados financeiros e relacionais | **PostgreSQL** | ACID, FK, joins, EF Core, xmin para concorrência otimista |
| Trilhas de auditoria LGPD | **MongoDB** | Schema-free para metadados variáveis, append-only natural, sem joins necessários |
| Arquivos físicos (PDFs) | **AWS S3** | Durabilidade 99,999999999%, custo por GB, URL pré-assinada para acesso controlado |

A referência cruzada é feita por chave: PostgreSQL guarda apenas a `chave` e `url` do S3; MongoDB guarda o `entidadeId` e `usuarioLogin` como strings — sem FKs entre bancos.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Apenas PostgreSQL** (JSONB para auditoria, bytea para arquivos) | Arquivos em banco degradam performance de queries; JSONB funciona mas perde a flexibilidade e isolamento operacional do MongoDB |
| **Apenas MongoDB** para tudo | Falta de transações multi-documento completas para dados financeiros; sem FK nativa para integridade |
| **PostgreSQL + disco local** (sem S3) | Viola RNF05; disco local em EC2 não é durável nem escalável; perda de dados em falha da instância |

---

## Consequências

**Positivas:**
- Cada banco opera em seu ponto forte; nenhum é forçado a um papel inadequado.
- Falha no MongoDB (auditoria) não bloqueia operações financeiras (PostgreSQL) — serviço degradado graciosamente.
- S3 isola os binários do ciclo de vida da EC2.

**Negativas / Trade-offs:**
- Três tecnologias de persistência para operar, monitorar e fazer backup separadamente.
- Testes de integração exigem Testcontainers para PostgreSQL e MongoDB (maior custo de CI).
- Sem transações distribuídas entre os bancos — eventual consistency na auditoria é aceitável pelo domínio.
