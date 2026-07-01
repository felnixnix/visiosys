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

Adotar uma estratégia em **duas camadas**:

1. **Hook `pre-push` (rápido, ~30s):** executa `dotnet build --configuration Release` +
   `dotnet test` nos testes de domínio (unit), sem Docker. Bloqueia pushes com erro de
   compilação C# ou falha nos testes unitários — os erros mais comuns no dia a dia.

2. **`scripts/validate-ci.sh` (completo, ~3-5 min):** simula o workflow `ci.yml` completo
   via **act** (nektos/act v0.2.89), incluindo build, testes de integração com
   Testcontainers e qualquer etapa do GitHub Actions. Requer Docker. Executado
   manualmente antes de pushes com mudanças significativas na infraestrutura ou testes
   de integração.

A tentativa de rodar act no hook `pre-push` foi descartada após verificar que o
tempo de execução (5-15 minutos por push, mesmo com imagem em cache) é incompatível
com o uso no dia a dia.

## Estrutura adotada

```
.actrc                   # plataforma e opções do act
.githooks/
  pre-push               # hook rápido: dotnet build + testes unitários (~30s)
scripts/
  setup-dev.sh           # configura core.hooksPath e verifica pré-requisitos
  validate-ci.sh         # simulação completa com act (execução manual)
```

O hook pode ser pulado com `git push --no-verify` quando necessário.

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| act no hook `pre-push` | 5-15 min por push; inviável no dia a dia |
| Apenas `dotnet build` sem testes | Não pega regressões em regras de domínio |
| Confiar apenas no CI do GitHub | Ciclo de feedback lento (2-5 min por iteração) e histórico poluído |

## Consequências

- Todo `git push` roda build + testes unitários em ~30s (sem Docker).
- Simulação completa disponível sob demanda via `bash scripts/validate-ci.sh`.
- `git push --no-verify` mantém a saída de emergência sem remover a proteção padrão.
- O `.actrc` e `.githooks/` são commitados; a configuração de `core.hooksPath` é local
  e ativada via `bash scripts/setup-dev.sh`.
