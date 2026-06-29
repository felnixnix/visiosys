# ADR-006: MongoDB para Trilha de Auditoria LGPD

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF04, RNF12

---

## Contexto

A LGPD (Lei Geral de Proteção de Dados — Lei nº 13.709/2018) exige que sistemas que tratam dados pessoais financeiros mantenham registro de quem acessou ou modificou esses dados, quando, e com qual finalidade. A trilha de auditoria tem características distintas dos dados transacionais:

- **Append-only:** registros de auditoria nunca são editados ou deletados.
- **Schema variável:** metadados de cada ação (campos alterados, valores anteriores/novos) variam por tipo de operação.
- **Volume crescente:** cresce indefinidamente; consultas são raras e analíticas.
- **Falha tolerável:** se o registro de auditoria falhar, a operação principal não deve ser interrompida.

---

## Decisão

Usar **MongoDB** como repositório exclusivo da trilha de auditoria, com a coleção `visiosys_auditoria.audit_logs`.

Cada documento registra: `acao`, `usuarioLogin`, `entidadeTipo`, `entidadeId`, `ocorridoEm` e um dicionário livre de `metadados`. O schema-free do MongoDB permite que cada tipo de ação carregue os metadados que fazem sentido para ela sem alterar schema do banco.

O `MongoAuditLogService` é registrado como **Singleton** e todas as chamadas são assíncronas e envolvidas em `try/catch` — uma falha no MongoDB não propaga exceção para a camada de aplicação. O erro é apenas logado via `ILogger`.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Tabela PostgreSQL** para auditoria | Funciona, mas schema fixo dificulta metadados variáveis; tabela grande impacta performance do banco transacional |
| **Amazon CloudWatch Logs** | Bom para logs de sistema; não oferece consultas estruturadas por entidade/usuário necessárias para compliance LGPD |
| **Elasticsearch** | Poderoso para analytics, mas custo e complexidade operacional excessivos para o volume atual |

---

## Consequências

**Positivas:**
- Isolamento total: problemas na auditoria não afetam operações financeiras.
- Schema flexível: novos tipos de auditoria não exigem migrations.
- Append-only por design: documentos MongoDB inseridos nunca são atualizados (imutabilidade da trilha).

**Negativas / Trade-offs:**
- Terceiro banco a operar e monitorar (além de PostgreSQL e S3).
- Sem garantia de entrega se MongoDB estiver indisponível no momento do evento — a auditoria pode ter lacunas. Para compliance mais rígido, considerar uma fila (SQS) como intermediário no futuro.
