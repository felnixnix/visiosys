# ADR-024 — Validação de CI Local com act (nektos/act)

**Status:** Aceito  
**Data:** 2026-07-01

## Contexto

O pipeline de CI no GitHub Actions (build + testes de domínio + testes de integração
com Testcontainers) falhou diversas vezes por erros que só eram detectados após o push:
erros de compilação C# (using directives ausentes, conflitos de versão de assembly) e
erros de tipagem TypeScript. Cada ciclo de fix-push-esperar-CI consumia entre 2 e 5 minutos
por iteração, além de poluir o histórico de commits com fixes sucessivos.

## Decisão

Adotar **act** (nektos/act v0.2.89) para executar o mesmo workflow `ci.yml` localmente
antes de cada push, usando um hook `pre-push` versionado no repositório.

O act roda o workflow em um container Docker (`catthehacker/ubuntu:act-latest`), que
reproduz o ambiente GitHub Actions ubuntu-latest com suporte a Docker-in-Docker,
permitindo que os testes de integração com Testcontainers funcionem corretamente.

## Estrutura adotada

```
.actrc                  # plataforma e opções do act
.githooks/
  pre-push              # hook: executa act -j build-and-test antes do push
scripts/
  setup-dev.sh          # configura core.hooksPath e verifica pré-requisitos
```

O hook é **não bloqueante por omissão de pré-requisitos**: se `act` ou Docker não estiverem
disponíveis, o push prossegue com um aviso. O desenvolvedor pode também pular
explicitamente com `git push --no-verify`.

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| Hooks locais executando `dotnet build` diretamente | Não reproduz o ambiente exato do CI (versão do .NET, variáveis, imagens) |
| Rodar só os testes de domínio (unit) | Não pega erros de compilação do frontend nem falhas de integração |
| Confiar apenas no CI do GitHub | Ciclo de feedback lento (2-5 min por iteração) e histórico poluído |

## Consequências

- **Primeira execução:** download da imagem `catthehacker/ubuntu:act-latest` (~1.5 GB), único.
- **Execuções seguintes:** ~2-3 minutos (build + testes), usando imagem em cache local.
- `git push --no-verify` mantém a saída de emergência sem remover a proteção padrão.
- O `.actrc` e `.githooks/` são commitados; a configuração de `core.hooksPath` é local
  e ativada via `bash scripts/setup-dev.sh`.
