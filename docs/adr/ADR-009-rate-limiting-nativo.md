# ADR-009: Rate Limiting Nativo .NET 8

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF17

---

## Contexto

A API é exposta publicamente na AWS. O endpoint de login é alvo natural de ataques de força bruta (credential stuffing, password spraying). Sem proteção, um atacante pode tentar milhares de combinações de senha por segundo sem custo. Um WAF (Web Application Firewall) da AWS resolveria o problema, mas adiciona custo e complexidade operacional desnecessários para o volume atual.

---

## Decisão

Usar o **Rate Limiting nativo do .NET 8** (`Microsoft.AspNetCore.RateLimiting`), configurado como **Fixed Window Limiter** no endpoint de login.

Política aplicada:
- **Janela:** 1 minuto
- **Limite:** 5 requisições por janela por IP de origem
- **Resposta ao exceder:** HTTP 429 Too Many Requests

O atributo `[EnableRateLimiting("login")]` é aplicado no `AuthController`. Outros endpoints podem receber políticas próprias conforme o sistema crescer.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **AWS WAF** | Eficaz mas custo adicional (~$5/mês mínimo + por request); desnecessário para o volume atual |
| **Nginx `limit_req_zone`** | Configuração no nginx resolve o problema, mas em camada diferente da aplicação; duplicação de responsabilidade |
| **AspNetCoreRateLimit** (terceiro) | Biblioteca madura mas viola a filosofia ".NET Native" do ADR-002; o nativo é suficiente |
| **Nenhum controle** | Inaceitável para sistema exposto publicamente (RNF17 explícito) |

---

## Consequências

**Positivas:**
- Proteção sem custo adicional de infraestrutura.
- Configurável por política nomeada — diferentes endpoints podem ter limites diferentes.
- Parte do middleware ASP.NET Core — sem dependência extra.

**Negativas / Trade-offs:**
- O Fixed Window Limiter é simples; ataques sofisticados que distribuem requisições entre múltiplos IPs não são bloqueados (nesses casos, WAF ou serviço dedicado seria necessário).
- Rate limit por IP: aplicações atrás de NAT compartilhado (escritórios) podem ter múltiplos usuários contando para o mesmo limite.
