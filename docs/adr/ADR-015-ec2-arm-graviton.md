# ADR-015: EC2 ARM Graviton2 t4g.medium para Hospedagem

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF06

---

## Contexto

O RNF06 especifica EC2 ARM `t4g.medium` explicitamente. Este ADR documenta o racional que levou a essa especificação. A aplicação é um sistema de backoffice para uma assessoria de precatórios — equipe de 5 a 30 usuários simultâneos, predominantemente CRUD com picos em horário comercial.

---

## Decisão

Usar **EC2 `t4g.medium`** (arquitetura ARM, processador AWS Graviton2):

| Especificação | Valor |
|---------------|-------|
| vCPUs | 2 (Graviton2 ARM) |
| RAM | 4 GB |
| Rede | Até 5 Gbps (bursting) |
| Custo (sa-east-1) | ~$25/mês (On-Demand) |

A instância hospeda: nginx (reverse proxy + SSL), Visiosys.Api (.NET 8), Visiosys.Worker (.NET 8), MongoDB (auditoria), Seq (logs).

**Região:** `sa-east-1` (São Paulo) — menor latência para usuários brasileiros.

**Capacidade estimada para o perfil de uso:**
- ~40–60 usuários simultâneos ativos (fazendo requisições).
- ~150–200 usuários online (maioria em leitura/idle).
- Gargalo real: RDS db.t4g.micro (ver ADR-016).

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **`t3.medium`** (x86) | ~20% mais caro que `t4g.medium` para especificações equivalentes; Graviton2 oferece melhor custo-benefício para cargas de trabalho sustentadas |
| **`t4g.small`** (1 GB RAM) | Insuficiente: .NET 8 API + Worker + MongoDB consomem ~1.5–2 GB em repouso |
| **`t4g.large`** (8 GB RAM) | Desnecessário para o volume atual; escalar verticalmente é simples se necessário |
| **Fargate (containers serverless)** | Elimina gerenciamento de servidor, mas custo por hora de CPU/RAM é maior; complexidade de deploy maior para equipe pequena |
| **EC2 x86 (`t3a.medium`)** | Graviton2 oferece melhor throughput por watt e melhor custo; .NET 8 tem suporte nativo a linux-arm64 |

---

## Consequências

**Positivas:**
- ~20% de economia vs. equivalentes x86 (Graviton2 pricing advantage).
- .NET 8 compila self-contained para `linux-arm64` nativo — sem emulação.
- Escalabilidade vertical simples: mudar de `t4g.medium` para `t4g.large` requer apenas stop/change/start na console.

**Negativas / Trade-offs:**
- ARM: algumas ferramentas de monitoramento ou agentes de APM podem não ter builds ARM64.
- Build de deploy deve especificar `--runtime linux-arm64` (já configurado no `deploy.yml`).
- Modo burst: `t4g` usa créditos de CPU para bursts — carga sustentada alta por horas pode esgotar créditos.
