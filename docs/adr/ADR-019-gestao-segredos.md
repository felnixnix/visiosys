# ADR-019: Gestão de Segredos via GitHub Secrets e Variáveis de Ambiente

**Status:** Aceito — a parte de credenciais do CI foi revista pelo [ADR-021](ADR-021-deploy-ssm-oidc.md): o `EC2_SSH_KEY` foi removido e a autenticação CI→AWS passou a usar OIDC (sem chaves estáticas). A gestão de segredos em produção (`/etc/visiosys/production.env` via systemd) permanece vigente.  
**Data:** 2026-06-29  
**Requisitos:** RNF16

---

## Contexto

O sistema lida com credenciais que, se expostas, comprometem dados financeiros de credores: senha do banco de dados, chave JWT, login do sistema, endpoint do MongoDB. O RNF16 proíbe explicitamente qualquer credencial no código-fonte. O risco é real: repositórios públicos com credenciais hardcoded são varridos automaticamente por bots em minutos após o commit.

---

## Decisão

Estratégia em camadas por ambiente:

**Desenvolvimento local:**
- Credenciais do ambiente Docker local (`appsettings.Development.json`) são valores conhecidamente fracos e de uso local. O arquivo é commitado porque as credenciais são placeholders de dev (ex: `visiosys_dev_pass`) — não funcionam fora do Docker Compose local.
- `appsettings.Production.json` está no `.gitignore` — nunca commitado.

**CI (GitHub Actions):**
- Secrets do deploy (`EC2_HOST`, `EC2_SSH_KEY`) armazenados nos **GitHub Secrets** do repositório.
- Injetados como variáveis de ambiente no workflow apenas durante a execução — nunca expostos nos logs.

**Produção (EC2):**
- Arquivo `/etc/visiosys/production.env` com permissão `640` (leitura apenas para o usuário da aplicação).
- Carregado pelo systemd via `EnvironmentFile=` — sem exposição via `ps aux` ou logs.
- A EC2 acessa o S3 via **IAM Role** (instance profile) — sem `AWS_ACCESS_KEY_ID` em nenhum arquivo.
- Rotação de credenciais: ao rotacionar, editar `production.env` e reiniciar os serviços — sem rebuild de imagem.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **AWS Secrets Manager** | Ideal para equipes maiores; custo por secret ($0.40/mês) e por 10k API calls; overhead desnecessário para o volume atual |
| **AWS Parameter Store** | Gratuito para parâmetros padrão; boa alternativa futura se o número de segredos crescer |
| **HashiCorp Vault** | Poderoso mas complexo de operar; overhead excessivo para equipe pequena |
| **`dotnet user-secrets`** | Ideal para dev local com múltiplos desenvolvedores; para dev solo com Docker, `appsettings.Development.json` é suficiente |
| **Credenciais no código** | Proibido por RNF16 |

---

## Consequências

**Positivas:**
- Nenhuma credencial de produção no repositório — auditoria do Git não revela segredos.
- IAM Role elimina chaves AWS estáticas — sem risco de rotação esquecida de `AWS_ACCESS_KEY_ID`.
- `EnvironmentFile` systemd: segredos não aparecem em `ps`, `journalctl` ou logs da aplicação.

**Negativas / Trade-offs:**
- `appsettings.Development.json` com credenciais de dev é commitado — aceitável porque são credenciais sem acesso ao ambiente de produção, mas gera alerta em scanners de segredos (ex: GitGuardian). Mitigação: adicionar exceção explícita no scanner.
- Rotação de segredos em produção exige SSH manual na EC2 para editar `production.env` — sem self-service. Para automação futura: Parameter Store com pull automático na startup.
