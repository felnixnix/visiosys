# ADR-011: BackgroundService para Captura Automática de Andamentos

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RF08, RNF01

---

## Contexto

O sistema precisa capturar automaticamente andamentos processuais de portais de tribunais (fontes externas instáveis). Esse processamento não pode bloquear requisições de usuários da API: é assíncrono, de longa duração, e tolerante a falhas parciais (um precatório com erro não deve parar os demais).

---

## Decisão

Implementar um **projeto separado** (`Visiosys.Worker`) usando o `BackgroundService` nativo do .NET (`IHostedService`), com separação em duas classes:

- **`ConsultaTribunaisProcessor`**: lógica pura de consulta (injetável, testável unitariamente sem timer).
- **`ConsultaTribunalWorker`**: timer loop com `Task.Delay(30min)` que cria um escopo DI por ciclo via `IServiceScopeFactory`.

O Worker é um processo separado da API:
- Pode ser deployado, escalado e reiniciado independentemente.
- Falhas no Worker não derrubam a API.
- Usa o mesmo PostgreSQL e MongoDB, mas sem conexão HTTP entre eles.

Tratamento de erro por precatório: cada precatório é processado em `try/catch` individual — um erro é logado com `LogError` e o processamento continua para os próximos.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Hangfire** | Framework completo para jobs; viola ADR-002 (.NET Native); requer banco para persistência de jobs |
| **Quartz.NET** | Similar ao Hangfire; desnecessário para um único job recorrente simples |
| **Azure Functions / AWS Lambda** | Serverless é elegante para este caso mas sai do ecossistema EC2 definido (RNF06); adiciona complexidade de deploy |
| **Job na mesma API (`IHostedService` no Visiosys.Api)** | Acoplaria o Worker ao ciclo de vida da API; qualquer restart da API interromperia jobs em andamento |

---

## Consequências

**Positivas:**
- Processo separado: falhas no scraping nunca afetam a disponibilidade da API.
- `IServiceScopeFactory` por ciclo: repositórios com escopo `Scoped` (EF Core DbContext) são criados e destruídos a cada execução — sem leaks de conexão.
- `ConsultaTribunaisProcessor` é 100% testável unitariamente com fakes (sem timer, sem DI framework).

**Negativas / Trade-offs:**
- Dois processos a operar, monitorar e deployar (dois serviços systemd na EC2).
- Intervalo de 30 minutos é fixo (configurável via `Worker:IntervalMinutos`); sem mecanismo de trigger externo para forçar execução imediata.
