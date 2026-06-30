# Architecture Decision Records — Visiosys

Registro das decisões arquiteturais tomadas no Sistema de Gestão e Assessoria de Precatórios.

Cada ADR documenta: **contexto**, **decisão**, **alternativas consideradas** e **consequências**.

---

## Índice

| ADR | Título | Status | Requisitos |
|-----|--------|--------|-----------|
| [ADR-001](ADR-001-arquitetura-ddd-camadas.md) | Arquitetura em Camadas com DDD e Modelos Ricos | Aceito | RNF01 |
| [ADR-002](ADR-002-dotnet-native.md) | .NET 8 e Filosofia ".NET Native" | Aceito | RNF01 |
| [ADR-003](ADR-003-persistencia-poliglota.md) | Persistência Poliglota (PostgreSQL + MongoDB + S3) | Aceito | RNF03, RNF04, RNF05 |
| [ADR-004](ADR-004-ef-core-mapeamento-fluente.md) | Entity Framework Core com Mapeamento Fluente | Aceito | RNF03 |
| [ADR-005](ADR-005-concorrencia-otimista-xmin.md) | Concorrência Otimista via xmin do PostgreSQL | Aceito | RNF15 |
| [ADR-006](ADR-006-mongodb-auditoria-lgpd.md) | MongoDB para Trilha de Auditoria LGPD | Aceito | RNF04, RNF12 |
| [ADR-007](ADR-007-s3-armazenamento-documentos.md) | AWS S3 para Armazenamento de Documentos | Aceito | RNF05, RF04 |
| [ADR-008](ADR-008-autenticacao-jwt.md) | Autenticação JWT Stateless | Aceito | RNF09 |
| [ADR-009](ADR-009-rate-limiting-nativo.md) | Rate Limiting Nativo .NET 8 | Aceito | RNF17 |
| [ADR-010](ADR-010-serilog-seq-observabilidade.md) | Serilog + Seq para Observabilidade | Aceito | RNF08, RNF13 |
| [ADR-011](ADR-011-worker-service-captura.md) | BackgroundService para Captura Automática de Andamentos | Aceito | RF08, RNF01 |
| [ADR-012](ADR-012-http-resilience.md) | Microsoft.Extensions.Http.Resilience para Resiliência HTTP | Aceito | RF08 |
| [ADR-013](ADR-013-react-vite-typescript.md) | React + Vite + TypeScript para o SPA | Aceito | RNF02, RF07 |
| [ADR-014](ADR-014-testcontainers-tdd.md) | Testcontainers para Testes de Integração (TDD) | Aceito | RNF07 |
| [ADR-015](ADR-015-ec2-arm-graviton.md) | EC2 ARM Graviton2 t4g.medium para Hospedagem | Aceito | RNF06 |
| [ADR-016](ADR-016-rds-postgresql-gerenciado.md) | Amazon RDS PostgreSQL como Banco Gerenciado | Aceito | RNF03, RNF14 |
| [ADR-017](ADR-017-terraform-iac.md) | Terraform para Infraestrutura como Código | Aceito | RNF11 |
| [ADR-018](ADR-018-github-actions-cicd.md) | GitHub Actions para CI/CD | Aceito (deploy substituído por ADR-021) | RNF07 |
| [ADR-019](ADR-019-gestao-segredos.md) | Gestão de Segredos via GitHub Secrets e Variáveis de Ambiente | Aceito (credenciais de CI revistas por ADR-021) | RNF16 |
| [ADR-020](ADR-020-governanca-ia.md) | Governança de IA com Regras Declarativas | Aceito | RNF18 |
| [ADR-021](ADR-021-deploy-ssm-oidc.md) | Deploy via AWS SSM com Autenticação OIDC | Aceito | RNF07, RNF11, RNF16 |
| [ADR-022](ADR-022-swagger-producao-basic-auth.md) | Exposição do Swagger em Produção Protegida por Basic Auth | Aceito | RF07, RNF10 |

---

## Formato dos ADRs

```
Status: Proposto | Aceito | Depreciado | Substituído por ADR-XXX
```

Para propor alterações a uma decisão existente, crie um novo ADR referenciando o anterior em vez de editar o ADR original — o histórico de decisões é imutável.
