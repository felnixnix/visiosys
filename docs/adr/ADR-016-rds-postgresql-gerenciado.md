# ADR-016: Amazon RDS PostgreSQL como Banco Gerenciado

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF03, RNF14

---

## Contexto

O PostgreSQL precisa rodar em produção de forma durável e com backups automatizados. Há duas opções: PostgreSQL autogerenciado na própria EC2, ou PostgreSQL gerenciado via Amazon RDS. O RNF14 exige backups automáticos e disaster recovery — implementar isso manualmente na EC2 é possível mas operacionalmente custoso.

---

## Decisão

Usar **Amazon RDS para PostgreSQL 16** com instância `db.t4g.micro` em subnet privada (sem acesso público).

Configurações críticas:
- **Criptografia em repouso:** habilitada (storage encrypted).
- **Backup automático:** 7 dias de retenção, janela 03h–04h UTC.
- **`deletion_protection = true`:** impede exclusão acidental via Terraform.
- **`skip_final_snapshot = false`:** snapshot final obrigatório antes de qualquer destroy.
- **Multi-AZ:** desabilitado no MVP (economia de custo); habilitar antes de operação crítica 24/7.
- **Subnet group privado:** RDS acessível apenas pela EC2 (security group restrito a porta 5432 da EC2).

**Custo:** ~$15/mês (db.t4g.micro, sa-east-1, On-Demand).

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **PostgreSQL na EC2** (autogerenciado) | Backup, HA, patches e monitoramento são responsabilidade manual; RNF14 exige backup automático |
| **Aurora PostgreSQL Serverless v2** | Compatível com PostgreSQL, escala a zero, mas custo mínimo é maior que `db.t4g.micro` para cargas previsíveis |
| **PlanetScale / Supabase** | SaaS externos; sai do ecossistema AWS definido; latência adicional de rede sa-east-1 |

---

## Consequências

**Positivas:**
- Backups automáticos diários com retenção de 7 dias — RNF14 atendido sem scripts manuais.
- Patches automáticos de segurança do PostgreSQL aplicados pela AWS.
- Point-in-time recovery: restauração para qualquer segundo nos últimos 7 dias.
- Monitoramento nativo via CloudWatch (CPU, conexões, IOPS) sem agentes extras.

**Negativas / Trade-offs:**
- `db.t4g.micro` (1 GB RAM) é o gargalo de concorrência do sistema: ~20–30 queries simultâneas confortáveis. Migrar para `db.t4g.small` (2 GB, +~$13/mês) quando o volume crescer.
- Single-AZ no MVP: falha da AZ interrompe o banco. Mitigação: RDS restaura automaticamente, mas com downtime de alguns minutos.
- Custo ligeiramente maior que PostgreSQL na EC2 — justificado pela eliminação de trabalho operacional.
