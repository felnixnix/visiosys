# ADR-010: Serilog + Seq para Observabilidade

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF08, RNF13

---

## Contexto

Em um sistema financeiro-jurídico, rastrear o que aconteceu e quando é crítico — tanto para debugging quanto para auditoria operacional. Logs de texto simples (`Console.WriteLine`) são difíceis de filtrar e correlacionar em produção. O RNF08 exige logs estruturados em JSON centralizados.

---

## Decisão

Usar **Serilog** como biblioteca de logging estruturado com dois sinks:
- **Console** (sempre ativo): útil em dev local e para captura pelo systemd/Docker.
- **Seq** (em dev e produção): plataforma de logs estruturados com interface de busca por propriedades JSON.

Configuração via `appsettings.json` com `ReadFrom.Configuration()` — sem código de configuração hardcoded. O `ReadFrom.Services()` permite que sinks injetem dependências via DI (ex: enriquecedores customizados futuros).

Health checks (`/health`) expostos via `AspNetCore.HealthChecks.Npgsql` verificam a conectividade com o PostgreSQL — separado dos logs mas parte da estratégia de observabilidade (RNF13).

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **`Microsoft.Extensions.Logging` padrão (sem Serilog)** | Não suporta JSON estruturado nativamente sem providers adicionais; Serilog é a extensão padrão de mercado |
| **NLog** | Maduro mas ecossistema .NET moderno converge para Serilog; configuração mais verbosa |
| **AWS CloudWatch Logs** | Boa opção para logs de sistema; interface menos amigável para desenvolvimento e debugging local |
| **Elasticsearch + Kibana (ELK)** | Poderoso mas custo e complexidade operacional desproporcionais ao volume atual |

---

## Consequências

**Positivas:**
- Logs JSON consultáveis por qualquer propriedade no Seq (ex: `UserId`, `PrecatorioId`, `StatusCode`).
- Correlação de requisições via `SourceContext` e `RequestId` automáticos.
- Configuração 100% via `appsettings` — sem rebuild para alterar níveis de log.
- Seq tem tier gratuito para usuário único (adequado para dev e pequenas equipes).

**Negativas / Trade-offs:**
- Serilog é um pacote de terceiro (viola parcialmente ADR-002) — aceito porque `Microsoft.Extensions.Logging` sozinho não entrega JSON estruturado com sinks configuráveis.
- Seq precisa estar rodando; se cair, logs vão apenas para Console (degradação aceitável).
