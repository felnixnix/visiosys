#!/bin/bash
# Orquestrador de deploy executado NA INSTANCIA via AWS SSM (como root).
# Baixa os artefatos publicados no S3 pelo CI, extrai e aciona deploy.sh.
# Argumentos: $1 = bucket S3, $2 = prefixo (ex: deploy/<run_id>), $3 = componente
set -euo pipefail

BUCKET="${1:?bucket obrigatorio}"
PREFIX="${2:?prefixo obrigatorio}"
COMPONENTE="${3:-all}"
APP_DIR="/opt/visiosys"
TMP="/tmp/visiosys-deploy"

log() { echo "[ssm_deploy] $*"; }

rm -rf "${TMP}"
mkdir -p "${TMP}"

log "Baixando deploy.sh..."
aws s3 cp "s3://${BUCKET}/${PREFIX}/deploy.sh" "${TMP}/deploy.sh"
chmod +x "${TMP}/deploy.sh"

if [[ "${COMPONENTE}" == "api" || "${COMPONENTE}" == "all" ]]; then
    log "Baixando e extraindo API..."
    aws s3 cp "s3://${BUCKET}/${PREFIX}/api.tar.gz" "${TMP}/api.tar.gz"
    rm -rf "${APP_DIR}/api.new"
    mkdir -p "${APP_DIR}/api.new"
    tar -xzf "${TMP}/api.tar.gz" -C "${APP_DIR}/api.new"
    chmod +x "${APP_DIR}/api.new/Visiosys.Api"
    chown -R ubuntu:ubuntu "${APP_DIR}/api.new"
fi

if [[ "${COMPONENTE}" == "worker" || "${COMPONENTE}" == "all" ]]; then
    log "Baixando e extraindo Worker..."
    aws s3 cp "s3://${BUCKET}/${PREFIX}/worker.tar.gz" "${TMP}/worker.tar.gz"
    rm -rf "${APP_DIR}/worker.new"
    mkdir -p "${APP_DIR}/worker.new"
    tar -xzf "${TMP}/worker.tar.gz" -C "${APP_DIR}/worker.new"
    chmod +x "${APP_DIR}/worker.new/Visiosys.Worker"
    chown -R ubuntu:ubuntu "${APP_DIR}/worker.new"
fi

log "Acionando troca de binarios (deploy.sh ${COMPONENTE})..."
"${TMP}/deploy.sh" "${COMPONENTE}"

rm -rf "${TMP}"
log "Deploy via SSM concluido."
