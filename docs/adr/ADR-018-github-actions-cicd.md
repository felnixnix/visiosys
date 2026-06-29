# ADR-018: GitHub Actions para CI/CD

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF07

---

## Contexto

O RNF07 exige pipeline de CI/CD implementado via GitHub Actions. O código já está no GitHub, o que elimina a necessidade de integrar uma ferramenta externa. CI (build + testes) deve rodar em todo push/PR; CD (deploy) deve ser controlado e auditável.

---

## Decisão

Dois workflows separados com responsabilidades distintas:

**`ci.yml`** — Integração Contínua (todo push e PR para `main`):
1. Restaurar dependências.
2. Build Release completo (todos os projetos).
3. Testes de domínio (unitários, rápidos, sem Docker).
4. Testes de integração (Testcontainers — requer Docker, disponível nos runners `ubuntu-latest`).

**`deploy.yml`** — Entrega Contínua (push para `main` + `workflow_dispatch` manual):
1. Build + testes (não reutiliza artefatos do CI para garantir consistência).
2. Publicação self-contained `linux-arm64` (API + Worker).
3. Build do frontend React.
4. SSH deploy para EC2 com rollback automático.
5. Health check pós-deploy (`GET /health`).
6. Ambiente `production` com aprovação obrigatória configurável no GitHub.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Jenkins** | Self-hosted, custo operacional; GitHub Actions é suficiente e zero infraestrutura |
| **CircleCI / TravisCI** | Serviços externos pagos; GitHub Actions é gratuito para repositórios e tem integração nativa |
| **GitLab CI** | Exigiria migrar o repositório do GitHub para GitLab |
| **AWS CodePipeline** | Nativo AWS, mas acoplado ao ecossistema; configuração mais verbosa; GitHub Actions integra diretamente com o repositório |

---

## Consequências

**Positivas:**
- Feedback imediato: qualquer push com testes quebrando é bloqueado antes do merge.
- Deploy auditável: cada deploy tem log completo no GitHub com quem triggerou e quando.
- `workflow_dispatch` com choice de componente: deploy de apenas API ou Worker sem reiniciar o outro.
- Artefatos publicados como `upload-artifact`: disponíveis para inspeção ou re-deploy manual.

**Negativas / Trade-offs:**
- Testes de integração com Testcontainers adicionam ~2–3 minutos ao CI (tempo de pull das imagens Docker no runner).
- O deploy via SSH direto é simples mas não oferece zero-downtime nativo — há alguns segundos de indisponibilidade durante o restart do serviço. Para zero-downtime, Blue/Green deploy ou rolling update seriam necessários no futuro.
