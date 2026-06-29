# ADR-012: Microsoft.Extensions.Http.Resilience para Resiliência HTTP

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RF08

---

## Contexto

O Worker consulta portais de transparência de tribunais — fontes externas governamentais historicamente instáveis: lentas, com timeout frequente, fora do ar em horários de pico. Sem resiliência, uma falha temporária de rede derrubaria o ciclo inteiro de captura. O sistema precisa tentar novamente de forma inteligente sem sobrecarregar o servidor remoto.

---

## Decisão

Usar **`Microsoft.Extensions.Http.Resilience`** com `AddStandardResilienceHandler()` no `HttpClient` do `TribunalHttpClient`.

O handler padrão inclui automaticamente:
- **Retry com backoff exponencial:** até 3 tentativas com espera crescente entre elas.
- **Circuit Breaker:** após N falhas consecutivas, o circuito abre e falhas futuras são retornadas imediatamente (sem aguardar timeout) por um período de recuperação.
- **Timeout por tentativa e total:** evita que uma requisição lenta bloqueie o processamento indefinidamente.

Configuração aplicada no `Program.cs` do Worker:
```csharp
builder.Services
    .AddHttpClient<IConsultaTribunalService, TribunalHttpClient>()
    .AddStandardResilienceHandler();
```

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Polly diretamente** | `Microsoft.Extensions.Http.Resilience` é construído sobre Polly v8; usar Polly direto seria reinventar o que o pacote já oferece de forma integrada ao `IHttpClientFactory` |
| **Sem resiliência (try/catch simples)** | Uma falha de rede derrubaria o ciclo inteiro; inaceitável para fontes governamentais reconhecidamente instáveis |
| **`HttpClient.Timeout` apenas** | Apenas timeout não fornece retry nem circuit breaker |

---

## Consequências

**Positivas:**
- Resiliência com configuração de uma linha — sem política manual de retry.
- Circuit breaker protege os servidores externos de sobrecarga em caso de degradação.
- Integrado ao `IHttpClientFactory`: gerenciamento correto de `HttpMessageHandler` (sem socket exhaustion).

**Negativas / Trade-offs:**
- `AddStandardResilienceHandler()` usa configurações padrão que podem não ser ideais para todos os tribunais (ex: timeout total pode ser muito curto para APIs lentas). Configuração customizada via `ResiliencePipelineBuilder` pode ser necessária no futuro.
