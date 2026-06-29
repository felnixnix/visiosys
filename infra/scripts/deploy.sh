#!/bin/bash
# Script de deploy remoto — executado VIA SSH pelo GitHub Actions.
# Argumentos: $1 = componente ("api" | "worker" | "all")
set -euo pipefail

COMPONENTE="${1:-all}"
APP_DIR="/opt/visiosys"
LOG_PREFIX="[deploy]"

log() { echo "${LOG_PREFIX} $*"; }

deploy_api() {
    log "Parando visiosys-api..."
    sudo systemctl stop visiosys-api || true

    log "Substituindo binarios da API..."
    rm -rf "${APP_DIR}/api.bak"
    mv "${APP_DIR}/api" "${APP_DIR}/api.bak" 2>/dev/null || true
    mv "${APP_DIR}/api.new" "${APP_DIR}/api"

    log "Iniciando visiosys-api..."
    sudo systemctl start visiosys-api
    sudo systemctl is-active --quiet visiosys-api && log "API iniciada com sucesso." || {
        log "ERRO: API falhou ao iniciar. Revertendo..."
        sudo systemctl stop visiosys-api || true
        mv "${APP_DIR}/api" "${APP_DIR}/api.failed"
        mv "${APP_DIR}/api.bak" "${APP_DIR}/api"
        sudo systemctl start visiosys-api
        exit 1
    }
}

deploy_worker() {
    log "Parando visiosys-worker..."
    sudo systemctl stop visiosys-worker || true

    log "Substituindo binarios do Worker..."
    rm -rf "${APP_DIR}/worker.bak"
    mv "${APP_DIR}/worker" "${APP_DIR}/worker.bak" 2>/dev/null || true
    mv "${APP_DIR}/worker.new" "${APP_DIR}/worker"

    log "Iniciando visiosys-worker..."
    sudo systemctl start visiosys-worker
    sudo systemctl is-active --quiet visiosys-worker && log "Worker iniciado com sucesso." || {
        log "ERRO: Worker falhou ao iniciar. Revertendo..."
        sudo systemctl stop visiosys-worker || true
        mv "${APP_DIR}/worker" "${APP_DIR}/worker.failed"
        mv "${APP_DIR}/worker.bak" "${APP_DIR}/worker"
        sudo systemctl start visiosys-worker
        exit 1
    }
}

case "${COMPONENTE}" in
    api)    deploy_api ;;
    worker) deploy_worker ;;
    all)    deploy_api; deploy_worker ;;
    *)
        echo "Uso: $0 [api|worker|all]"
        exit 1
        ;;
esac

log "Deploy concluido."
