# ADR-027 — Trade-offs deliberados de MVP

**Status:** Aceito  
**Data:** 2026-07-01

## Contexto

O Visiosys é um projeto de estudos aplicados, rodando em infraestrutura AWS real
com custo saindo do bolso do autor. Nesse cenário, algumas escolhas de escopo foram
feitas **de forma consciente**, priorizando custo e simplicidade sobre robustez
máxima de produção crítica. Este ADR registra essas decisões para que fiquem
explícitas — são limitações conhecidas, não descuidos.

## Decisões

### 1. RDS PostgreSQL em Single-AZ

O banco roda em **uma única zona de disponibilidade** (`multi_az = false`), em vez de
Multi-AZ com failover automático.

- **Motivo:** Multi-AZ praticamente dobra o custo do RDS. Para um ambiente de estudos,
  o ganho de disponibilidade não justifica o custo recorrente.
- **Mitigações já em vigor** (ver [`infra/terraform/rds.tf`](../../infra/terraform/rds.tf)):
  backups automáticos com retenção de 7 dias, `deletion_protection = true` e
  `skip_final_snapshot = false` (snapshot final na destruição). Ou seja, a proteção
  contra **perda de dados** existe; o que se abre mão é do **failover automático**
  em caso de falha de uma AZ.
- **Como sair do trade-off:** trocar uma linha (`multi_az = true`) e aplicar — o
  restante da infraestrutura já suporta.

### 2. Worker de captura com fonte externa simulada

O Worker de captura de andamentos (`Visiosys.Worker`) integra com um endpoint de
tribunal **fictício** (`Worker__TribunalBaseUrl`), pois não há uma API pública única
e estável de andamentos processuais para consumir.

- **O que é real:** toda a arquitetura de integração resiliente — `HttpClient`
  tipado com `AddStandardResilienceHandler()` do .NET 8 (retry com backoff, circuit
  breaker, timeout), o `BackgroundService` com agendamento e o tratamento de falhas.
  Esse código funcionaria contra uma fonte real trocando apenas a URL e o parsing da
  resposta.
- **O que é simulado:** a fonte de dados em si. O objetivo aqui foi demonstrar o
  **padrão de resiliência a fontes externas instáveis**, não integrar com um tribunal
  específico.

## Fora de escopo (consciente)

Itens típicos de produção crítica deixados de fora por serem desproporcionais a um
projeto de portfólio: WAF, rotação automática de segredos, auto-scaling horizontal,
DR multi-região e observabilidade com APM/tracing distribuído. A observabilidade
atual (logs estruturados + dashboard, health checks) é suficiente para o escopo.

## Consequências

- As limitações ficam documentadas e rastreáveis, em vez de implícitas.
- Cada trade-off tem um caminho claro de evolução, caso o projeto saísse do escopo de
  estudos para produção crítica.
