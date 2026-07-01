#!/usr/bin/env bash
#
# validate-ci.sh — simula o CI completo localmente via act.
# Inclui build, testes de dominio E testes de integracao com Testcontainers.
# Requer Docker rodando e a imagem catthehacker/ubuntu:act-latest disponivel.
#
# Uso:
#   bash scripts/validate-ci.sh
#
# Para apenas build + testes unitarios rapidos, o hook pre-push ja faz isso
# automaticamente a cada "git push".
#

set -euo pipefail

log()  { echo "[validate-ci] $*"; }
fail() { echo "[validate-ci] FALHOU: $*"; exit 1; }

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# Verifica Docker
if ! docker info > /dev/null 2>&1; then
  fail "Docker nao esta rodando. Inicie o Docker Desktop."
fi

# Verifica act
ACT_CMD=""
if command -v act > /dev/null 2>&1; then
  ACT_CMD="act"
elif [ -f "$LOCALAPPDATA/Microsoft/WinGet/Links/act.exe" ]; then
  ACT_CMD="$LOCALAPPDATA/Microsoft/WinGet/Links/act.exe"
elif [ -f "$HOME/AppData/Local/Microsoft/WinGet/Links/act.exe" ]; then
  ACT_CMD="$HOME/AppData/Local/Microsoft/WinGet/Links/act.exe"
else
  fail "act nao encontrado. Instale com: winget install nektos.act"
fi

# Limpa containers testcontainers estagnados de execucoes anteriores
log "Limpando containers testcontainers estagnados..."
docker ps -aq --filter "name=testcontainers-" | xargs -r docker rm -f > /dev/null 2>&1 || true

log "Iniciando simulacao CI completa com act..."
log "(Primeira execucao pode levar mais tempo enquanto baixa dependencias)"
log ""

"$ACT_CMD" -j build-and-test \
  --workflows .github/workflows/ci.yml

EXIT_CODE=$?
if [ $EXIT_CODE -ne 0 ]; then
  fail "Simulacao CI falhou (exit $EXIT_CODE). Corrija antes de fazer push."
fi

log ""
log "Simulacao CI completa passou. Seguro para fazer push."
