# ADR-007: AWS S3 para Armazenamento de Documentos

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF05, RF04

---

## Contexto

O sistema precisa armazenar PDFs de procurações, certidões, contratos e petições associados a precatórios e clientes. Esses arquivos podem ter dezenas de megabytes cada e o volume cresce continuamente. Armazená-los em disco local da EC2 criaria três problemas: (1) o disco local não é durável em caso de falha da instância; (2) impossibilita escalabilidade horizontal (múltiplas instâncias não compartilham disco local); (3) o RNF05 proíbe explicitamente armazenamento local.

---

## Decisão

Usar **AWS S3** como único repositório de arquivos físicos. O PostgreSQL armazena apenas a **chave** (path no bucket) e a **URL** para referência — nunca o conteúdo binário.

Implementação:
- **`S3ArmazenamentoService`** em produção: faz `PutObjectAsync` no bucket e retorna URL pré-assinada com expiração de 1 hora.
- **`LocalArmazenamentoService`** em desenvolvimento: stub que gera chave/URL sem subir arquivo — sem dependência de credenciais AWS em dev.
- A seleção é automática: se `Storage:S3Bucket` estiver configurado, a API usa S3; caso contrário, usa o stub local.
- A EC2 acessa o S3 via **IAM Role** (instance profile) — sem credenciais explícitas no código ou em arquivos de configuração.

Chave de armazenamento: `documentos/{ano}/{mês}/{uuid}-{nome-original}`.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Disco local da EC2** | Proibido por RNF05; não durável; não escalável horizontalmente |
| **PostgreSQL BYTEA** | Degrada performance de queries; backups enormes; inadequado para BLOBs |
| **EFS (Elastic File System)** | Durável e compartilhável, mas custo mais alto que S3 para armazenamento de arquivos estáticos; sem URL direta |
| **Cloudflare R2** | Compatível com S3 API, sem egress fees, mas sai do ecossistema AWS definido no RNF06 |

---

## Consequências

**Positivas:**
- Durabilidade 99,999999999% (S3 SLA).
- Custo baixo: ~$0,023/GB/mês com lifecycle rule para IA após 90 dias.
- URL pré-assinada garante acesso temporário e controlado sem tornar o bucket público.
- IAM Role elimina o risco de credenciais AWS vazadas no código.

**Negativas / Trade-offs:**
- URL pré-assinada expira em 1 hora — o frontend não pode cachear links de download indefinidamente.
- Em desenvolvimento, o `LocalArmazenamentoService` não persiste arquivos de fato; testes de upload não testam o S3 real (isolamento intencional).
