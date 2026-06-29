# ADR-002: .NET 8 e Filosofia ".NET Native"

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF01

---

## Contexto

Em projetos .NET é comum adicionar bibliotecas de terceiros para funcionalidades que o próprio ecossistema já oferece nativamente: MediatR para CQRS, Hangfire para background jobs, FluentValidation para validação, AutoMapper para mapeamento. Cada dependência adicional é um vetor de incompatibilidade, atualização e manutenção.

---

## Decisão

Adotar a filosofia **".NET Native"**: preferir recursos oficiais do ecossistema Microsoft em vez de pacotes de terceiros, salvo quando estes oferecerem funcionalidade sem equivalente nativo de qualidade comparável.

Escolhas concretas resultantes desta filosofia:

| Necessidade | Solução nativa adotada | Alternativa de terceiro descartada |
|-------------|----------------------|-----------------------------------|
| Background jobs | `BackgroundService` (.NET) | Hangfire, Quartz.NET |
| Mensagens/CQRS | Use cases simples (classes) | MediatR |
| Validação | Validação na entidade (DDD) | FluentValidation |
| Mapeamento DTO | Projeção manual ou record | AutoMapper |
| Rate Limiting | `AddRateLimiter()` (.NET 8) | AspNetCoreRateLimit |
| Resiliência HTTP | `Microsoft.Extensions.Http.Resilience` | Polly direto |
| Logging | `Microsoft.Extensions.Logging` + Serilog | NLog, log4net |
| Testes de integração | `WebApplicationFactory` + Testcontainers | TestServer customizado |

---

## Alternativas Consideradas

Adotar uma stack com mais bibliotecas de terceiros (MediatR + AutoMapper + Hangfire) é um padrão comum e documentado. Foi descartado porque:
- Aumenta a superfície de atualização e o risco de incompatibilidade entre versões.
- O .NET 8 já provê tudo que o projeto precisa com qualidade de produção.
- Reduz a dependência de decisões de terceiros (mudanças de licença, abandono de mantenedor).

---

## Consequências

**Positivas:**
- Menos dependências no `.csproj`; processo de atualização mais simples.
- Alinhamento com o roadmap oficial do .NET (funcionalidades nativas recebem suporte de longo prazo).
- Equipe aprende os fundamentos em vez de abstrações de terceiros.

**Negativas / Trade-offs:**
- Algumas abstrações de terceiros (ex: MediatR) oferecem convenções úteis; sem elas, o wiring é mais explícito.
- Colaboradores vindos de projetos com MediatR/AutoMapper podem estranhar a ausência dessas ferramentas.
