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

# 2. Verifica act
log "Verificando act..."
if command -v act > /dev/null 2>&1; then
  ok "act $(act --version | awk '{print $3}') encontrado."
else
  warn "act nao encontrado. Instale com:"
  warn "  Windows : winget install nektos.act"
  warn "  macOS   : brew install act"
  warn "  Linux   : curl -sSf https://raw.githubusercontent.com/nektos/act/master/install.sh | bash"
fi

# 3. Verifica Docker
log "Verificando Docker..."
if docker info > /dev/null 2>&1; then
  ok "Docker $(docker --version | awk '{print $3}' | tr -d ',') rodando."
else
  warn "Docker nao esta rodando ou nao esta instalado."
  warn "Inicie o Docker Desktop antes de fazer push."
fi

# 4. Verifica imagem do act (pull apenas se necessário)
log "Verificando imagem catthehacker/ubuntu:act-latest..."
if docker image inspect catthehacker/ubuntu:act-latest > /dev/null 2>&1; then
  ok "Imagem ja disponivel localmente."
else
  warn "Imagem nao encontrada localmente (~1.5GB). Sera baixada no primeiro push."
  warn "Para baixar agora: docker pull catthehacker/ubuntu:act-latest"
fi

echo ""
echo "Ambiente configurado. O pre-push hook vai validar o CI local antes de cada push."
echo "Para pular a validacao em um push especifico: git push --no-verify"
