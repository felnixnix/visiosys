#!/bin/bash
# Bootstrap da EC2 Visiosys — Ubuntu 24.04 ARM64
# Executado UMA VEZ na criação da instância.
set -euo pipefail
exec > /var/log/user_data.log 2>&1

# -------------------------------------------------------------------
# 1. Sistema base
# -------------------------------------------------------------------
apt-get update -y
apt-get upgrade -y
apt-get install -y curl wget gnupg unzip nginx certbot python3-certbot-nginx \
    apt-transport-https software-properties-common

# AWS CLI v2 (ARM64) — necessario para o deploy via SSM baixar artefatos do S3.
curl -fsSL "https://awscli.amazonaws.com/awscli-exe-linux-aarch64.zip" -o /tmp/awscliv2.zip
unzip -q -o /tmp/awscliv2.zip -d /tmp
/tmp/aws/install --update
rm -rf /tmp/awscliv2.zip /tmp/aws

# -------------------------------------------------------------------
# 2. .NET 8 Runtime (ARM64)
# -------------------------------------------------------------------
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update -y
apt-get install -y aspnetcore-runtime-8.0

# -------------------------------------------------------------------
# 3. MongoDB 8 (Community) - 7.0 nao tem repositorio para Ubuntu 24.04 (noble)
# -------------------------------------------------------------------
curl -fsSL https://www.mongodb.org/static/pgp/server-8.0.asc \
    | gpg --batch --yes -o /usr/share/keyrings/mongodb-server-8.0.gpg --dearmor
echo "deb [ arch=arm64 signed-by=/usr/share/keyrings/mongodb-server-8.0.gpg ] \
  https://repo.mongodb.org/apt/ubuntu noble/mongodb-org/8.0 multiverse" \
    | tee /etc/apt/sources.list.d/mongodb-org-8.0.list
apt-get update -y
apt-get install -y mongodb-org
systemctl enable --now mongod

# Aguarda o mongod aceitar conexões antes de criar o usuário
for i in $(seq 1 30); do
    mongosh --eval "db.adminCommand('ping')" >/dev/null 2>&1 && break
    sleep 1
done

# Cria usuário admin do MongoDB para a aplicação
mongosh --eval "
  db = db.getSiblingDB('admin');
  db.createUser({ user: 'visiosys_prod', pwd: 'SUBSTITUIR_SENHA_MONGO', roles:[{role:'readWrite',db:'visiosys_auditoria'}] });
"

# -------------------------------------------------------------------
# 4. Diretórios da aplicação
# -------------------------------------------------------------------
mkdir -p /opt/visiosys/api
mkdir -p /opt/visiosys/worker
mkdir -p /etc/visiosys
mkdir -p /var/log/visiosys

chown -R ubuntu:ubuntu /opt/visiosys
chmod 750 /etc/visiosys

# -------------------------------------------------------------------
# 5. Arquivo de variáveis de ambiente (preencher após o deploy)
# -------------------------------------------------------------------
cat > /etc/visiosys/production.env << 'EOF'
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000

ConnectionStrings__Postgres=Host=ENDPOINT_RDS;Port=5432;Database=visiosys;Username=visiosys_prod;Password=SUBSTITUIR
ConnectionStrings__Mongo=mongodb://visiosys_prod:SUBSTITUIR@localhost:27017/visiosys_auditoria?authSource=admin

Jwt__Chave=SUBSTITUIR_CHAVE_JWT_MINIMO_32_CHARS
Jwt__Emissor=visiosys-prod
Jwt__ExpiraEmMinutos=480

Auth__Login=SUBSTITUIR_LOGIN_ADMIN
Auth__Senha=SUBSTITUIR_SENHA_ADMIN

Storage__S3Bucket=SUBSTITUIR_NOME_BUCKET
Storage__BaseUrl=https://SUBSTITUIR_NOME_BUCKET.s3.sa-east-1.amazonaws.com

# Path base do domínio público (ver ADR-023). Deixe vazio para servir na raiz.
PathBase=/visiosys

Worker__TribunalBaseUrl=https://api.tribunal.jus.br
Worker__IntervalMinutos=30

Serilog__WriteTo__0__Name=Console
EOF

chmod 640 /etc/visiosys/production.env

# -------------------------------------------------------------------
# 6. Serviços systemd
# -------------------------------------------------------------------
cat > /etc/systemd/system/visiosys-api.service << 'EOF'
[Unit]
Description=Visiosys API (.NET 8)
After=network.target

[Service]
Type=exec
User=ubuntu
WorkingDirectory=/opt/visiosys/api
ExecStart=/opt/visiosys/api/Visiosys.Api
EnvironmentFile=/etc/visiosys/production.env
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=visiosys-api

[Install]
WantedBy=multi-user.target
EOF

cat > /etc/systemd/system/visiosys-worker.service << 'EOF'
[Unit]
Description=Visiosys Worker Service (.NET 8)
After=network.target mongod.service

[Service]
Type=exec
User=ubuntu
WorkingDirectory=/opt/visiosys/worker
ExecStart=/opt/visiosys/worker/Visiosys.Worker
EnvironmentFile=/etc/visiosys/production.env
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=visiosys-worker

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload

# -------------------------------------------------------------------
# 7. nginx — reverse proxy
#    Dois server blocks: um para acesso direto por IP (sem TLS, usado
#    pelo health check do deploy.yml) e outro para o domínio público
#    felipedearaujo.dev/visiosys, com HTTPS via certbot (ver ADR-023).
# -------------------------------------------------------------------
cat > /etc/nginx/sites-available/visiosys << 'EOF'
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        client_max_body_size 50m;
    }
}
EOF

cat > /etc/nginx/sites-available/visiosys-domain << 'EOF'
server {
    listen 80;
    server_name felipedearaujo.dev www.felipedearaujo.dev;

    location = / {
        return 302 /visiosys/;
    }

    location /visiosys/ {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        client_max_body_size 50m;
    }

    location / {
        return 404;
    }
}
EOF

ln -sf /etc/nginx/sites-available/visiosys /etc/nginx/sites-enabled/visiosys
ln -sf /etc/nginx/sites-available/visiosys-domain /etc/nginx/sites-enabled/visiosys-domain
rm -f /etc/nginx/sites-enabled/default
# restart (nao apenas enable --now): o nginx ja sobe rodando apos o apt install,
# entao e preciso recarregar para aplicar o site do visiosys.
nginx -t && systemctl enable nginx && systemctl restart nginx

# O Elastic IP é reassociado a uma instância nova automaticamente (ver ec2.tf),
# entao o DNS de felipedearaujo.dev ja resolve para esta maquina no boot —
# certbot pode rodar sem espera manual. Se falhar (ex: zona Route 53 ainda
# nao criada num provisionamento do zero), so loga e segue: o site continua
# funcionando em HTTP ate o certbot ser rodado manualmente depois.
certbot --nginx -d felipedearaujo.dev -d www.felipedearaujo.dev --non-interactive --agree-tos \
    -m felnixnix@gmail.com --redirect \
    || echo "certbot falhou (DNS pode nao estar propagado ainda) - rodar manualmente depois"

echo "Bootstrap concluido. Edite /etc/visiosys/production.env antes de iniciar os servicos."
