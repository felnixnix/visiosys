# ADR-021: Deploy via AWS Systems Manager (SSM) com Autenticação OIDC

**Status:** Aceito  
**Data:** 2026-06-30  
**Requisitos:** RNF07, RNF11, RNF16  
**Relaciona-se com:** substitui o mecanismo de deploy do [ADR-018](ADR-018-github-actions-cicd.md) e a estratégia de credenciais de CI do [ADR-019](ADR-019-gestao-segredos.md).

---

## Contexto

O ADR-018 definiu o deploy via **SSH direto** do runner do GitHub Actions para a EC2 (`scp` dos artefatos + `ssh` para reiniciar os serviços). Ao provisionar o ambiente real, esse modelo se mostrou inviável por uma razão de segurança concreta:

- O Security Group da EC2 restringe a porta 22 (SSH) **apenas ao IP do operador** (`<ip>/32`), conforme a postura de menor exposição.
- Os runners hospedados do GitHub Actions usam **IPs dinâmicos** da Microsoft/Azure, diferentes a cada execução. Para o deploy via SSH funcionar, seria preciso **abrir a porta 22 para `0.0.0.0/0`** — expondo o SSH de um sistema com dados financeiros/jurídicos a toda a internet.

As alternativas para destravar o deploy mantendo a segurança foram avaliadas e a escolha recaiu sobre o AWS Systems Manager.

Durante o provisionamento, também ficou evidente que o schema do banco precisava ser criado no RDS de forma automática a cada deploy, já que o RDS fica em sub-redes privadas (sem acesso externo direto para rodar `dotnet ef`).

---

## Decisão

**1. Deploy via AWS SSM `send-command`** — sem porta de entrada SSH para o CI:
- O job de deploy publica os artefatos (`api.tar.gz`, `worker.tar.gz`, `deploy.sh`, `ssm_deploy.sh`) num bucket S3 dedicado (`visiosys-deploy-<env>-<account>`, com expiração automática em 14 dias).
- Aciona `aws ssm send-command` (documento `AWS-RunShellScript`) na instância, que executa o `ssm_deploy.sh`: baixa os artefatos do S3, extrai e chama o `deploy.sh` existente (troca de binários com rollback automático).
- A porta 22 permanece restrita ao IP do operador (apenas para diagnóstico manual).

**2. Autenticação CI → AWS via OIDC** — sem chaves de longa duração:
- Um *OIDC provider* (`token.actions.githubusercontent.com`) e uma IAM Role (`visiosys-github-deploy-<env>`) assumível somente pelo repositório (`repo:felnixnix/visiosys:*`) substituem qualquer `AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY` estático.
- A Role concede o mínimo: `s3:PutObject` no bucket de deploy, `ssm:SendCommand` na instância alvo + documento `AWS-RunShellScript`, e leitura do resultado do comando.
- Único secret no GitHub: `AWS_DEPLOY_ROLE_ARN` (não sensível — é um identificador). O `EC2_SSH_KEY` foi **removido**.

**3. Migrations aplicadas no startup da API** — `db.Database.Migrate()` (idempotente) cria/atualiza o schema no RDS automaticamente a cada deploy, viabilizando deploys self-contained sem acesso direto ao banco privado.

**Pré-requisitos de infraestrutura** (adicionados ao Terraform):
- Policy `AmazonSSMManagedInstanceCore` anexada à IAM Role da EC2 (instance profile efetivamente associado à instância).
- AWS CLI instalado no bootstrap (necessário para o `ssm_deploy.sh` baixar do S3).

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Abrir porta 22 para `0.0.0.0/0`** | Solução de 1 linha, mas expõe o SSH de produção à internet inteira. Inaceitável para um sistema com dados sensíveis, mesmo com autenticação só por chave |
| **Restringir a 22 aos ranges de IP do GitHub** | Os ranges dos runners são enormes, mudam com frequência e excedem o limite prático de regras de um Security Group |
| **Self-hosted runner na EC2** | Agente de longa duração executando código do GitHub dentro do host de produção (risco em repositório público); consome recursos da `t4g.medium` que já roda API + Worker + Mongo + nginx |
| **AWS CodeDeploy** | Mais "AWS-nativo", porém adiciona um agente e um modelo de aplicação/deployment-group próprios; o SSM `send-command` é suficiente e reaproveita o `deploy.sh` já existente |

---

## Consequências

**Positivas:**
- **Zero porta de entrada nova:** o CI faz deploy sem que a porta 22 (ou qualquer outra) seja aberta para a internet. A superfície de ataque não cresce.
- **Sem credenciais AWS estáticas no GitHub:** o OIDC emite credenciais temporárias por execução; nada para rotacionar ou vazar.
- **Auditoria nativa:** cada `send-command` fica registrado no CloudTrail e no histórico do SSM (quem, quando, qual comando, saída completa).
- **Deploy self-contained:** migrate-on-startup cria o schema automaticamente; um deploy limpo deixa a aplicação funcional sem passos manuais no banco.
- Reaproveita o `deploy.sh` (troca de binários + rollback) sem reescrevê-lo.

**Negativas / Trade-offs:**
- **Migrate-on-startup** acopla a aplicação da schema ao boot. Aceitável para o cenário **single-instance** atual; com múltiplas instâncias seria preciso coordenar migrations (job dedicado) para evitar corrida.
- O fluxo de deploy ganhou uma dependência do **AWS CLI na instância** e do **agente SSM** registrado — pontos a monitorar.
- Artefatos trafegam por um bucket S3 intermediário (custo desprezível, mitigado por lifecycle de 14 dias) em vez de irem direto do runner para a EC2.
