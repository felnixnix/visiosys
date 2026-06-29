# ADR-017: Terraform para Infraestrutura como Código

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF11

---

## Contexto

O RNF11 exige que todo provisionamento da AWS seja declarativo via IaC. Sem IaC, a infraestrutura é criada manualmente na console AWS — sem rastreabilidade de mudanças, sem reprodutibilidade em caso de disaster recovery, e sem revisão de código antes de alterações.

---

## Decisão

Usar **Terraform** (HashiCorp) para declarar toda a infraestrutura AWS como código HCL versionado no repositório, em `infra/terraform/`.

Decisões específicas:
- **Estado remoto em S3** (`visiosys-terraform-state`): compartilhado, durável, com versionamento habilitado. Sem estado local que se perde com a máquina do desenvolvedor.
- **Módulos sem abstração excessiva:** recursos declarados diretamente em arquivos por responsabilidade (`ec2.tf`, `rds.tf`, `s3.tf`, `vpc.tf`) — sem módulos Terraform customizados desnecessários para a escala atual.
- **`terraform.tfvars` no `.gitignore`:** valores sensíveis (senha RDS, CIDR SSH) nunca commitados.
- **`terraform.tfvars.example`** commitado: template documentando todas as variáveis necessárias.
- **`terraform apply` requer aprovação humana explícita** (governança de IA — ver ADR-020).
- **`deletion_protection = true` no RDS** e **`lifecycle { ignore_changes }` na EC2**: proteção contra destruição acidental.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **AWS CDK (CloudFormation)** | Ligado ao ecossistema AWS; Terraform é cloud-agnostic e mais portável se houver migração futura |
| **AWS CloudFormation direto** | YAML/JSON verboso; ecossistema de providers menor; Terraform tem melhor DX |
| **Pulumi** | IaC em código real (.NET, Python); interessante mas menos maduro que Terraform; menor comunidade |
| **Console AWS manual** | Sem rastreabilidade, sem revisão, sem reprodutibilidade — inaceitável para RNF11 |

---

## Consequências

**Positivas:**
- Infraestrutura como Pull Request: mudanças de infra são revisadas antes de aplicadas.
- `terraform plan` mostra exatamente o que será criado/alterado/destruído antes de qualquer ação.
- Disaster recovery reproduzível: `terraform apply` recria toda a infraestrutura a partir do estado declarado.
- Estado remoto em S3: múltiplos operadores trabalham no mesmo estado sem conflitos.

**Negativas / Trade-offs:**
- Terraform não gerencia estado in-band de sistemas (ex: dados do banco, configurações do MongoDB). Esses precisam de automação separada (scripts de bootstrap).
- Drift: se alguém alterar um recurso manualmente na console, o estado Terraform fica desatualizado — `terraform refresh` ou `terraform import` necessários.
- `terraform apply` é destrutivo em alguns cenários (ex: mudar nome de bucket S3 destroi e recria) — sempre revisar o `plan` com atenção.
