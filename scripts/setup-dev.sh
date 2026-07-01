#!/usr/bin/env bash
#
# setup-dev.sh — configure o ambiente de desenvolvimento local.
# Execute uma vez após clonar o repositório:
#   bash scripts/setup-dev.sh
#

set -euo pipefail

log()  { echo "[setup] $*"; }
ok()   { echo "[setup] OK: $*"; }
warn() { echo "[setup] AVISO: $*"; }

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# 1. Configura git para usar os hooks do repositório
log "Configurando git hooks..."
git -C "$REPO_ROOT" config core.hooksPath .githooks
chmod +x "$REPO_ROOT/.githooks/pre-push"
ok "core.hooksPath = .githooks"

# 2. Verifica .NET SDK
log "Verificando .NET SDK..."
if command -v dotnet > /dev/null 2>&1; then
  ok "dotnet $(dotnet --version) encontrado."
else
  warn "dotnet SDK nao encontrado. Instale o .NET 8 SDK em https://dotnet.microsoft.com/download"
fi

# 3. Docker (opcional — necessario apenas para testes de integracao e validate-ci.sh)
log "Verificando Docker..."
if docker info > /dev/null 2>&1; then
  ok "Docker $(docker --version | awk '{print $3}' | tr -d ',') rodando."
else
  warn "Docker nao esta rodando. Necessario para testes de integracao e bash scripts/validate-ci.sh"
fi

echo ""
echo "Ambiente configurado."
echo "  - Todo 'git push' executa build Release + testes unitarios (~30s)."
echo "  - Para simulacao CI completa com act: bash scripts/validate-ci.sh"
echo "  - Para pular o hook em um push especifico: git push --no-verify"
