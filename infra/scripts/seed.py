#!/usr/bin/env python3
"""Seeder de dados mock para o Visiosys (ambiente de estudos aplicados).

Popula a plataforma com dados realistas (clientes, precatorios, andamentos e
pagamentos) usando a API REAL via localhost — passando pelas regras de dominio
(validacao de CPF/CNPJ, calculo de desagio, transicao de status, auditoria no
MongoDB), e nao por INSERT direto no banco.

Idempotente/resumivel: nao recria clientes/precatorios se ja existirem e so
adiciona andamentos/pagamentos onde faltam.

Execucao (na instancia EC2, onde estao as credenciais e a API):
    scp infra/scripts/seed.py ubuntu@<host>:/tmp/seed.py
    ssh ubuntu@<host> "sudo python3 /tmp/seed.py"

Le o login/senha admin de /etc/visiosys/production.env (exige sudo).
Sem dependencias externas — usa apenas a biblioteca padrao do Python 3.
"""
import json
import os
import random
import re
import urllib.request
import urllib.error
from datetime import datetime, timedelta, timezone

BASE = os.environ.get("VISIOSYS_API", "http://localhost/api")
ENV = os.environ.get("VISIOSYS_ENV_FILE", "/etc/visiosys/production.env")
random.seed(42)


# ---------------- credenciais ----------------
def ler_env(chave):
    with open(ENV) as f:
        for linha in f:
            if linha.startswith(chave + "="):
                return linha.split("=", 1)[1].strip()
    raise SystemExit(f"Chave {chave} nao encontrada em {ENV}")


# ---------------- http ----------------
TOKEN = None


def req(metodo, caminho, corpo=None):
    dados = json.dumps(corpo).encode() if corpo is not None else None
    r = urllib.request.Request(BASE + caminho, data=dados, method=metodo)
    r.add_header("Content-Type", "application/json")
    if TOKEN:
        r.add_header("Authorization", "Bearer " + TOKEN)
    try:
        with urllib.request.urlopen(r, timeout=30) as resp:
            txt = resp.read().decode()
            return resp.status, (json.loads(txt) if txt else None)
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode()


# ---------------- CPF / CNPJ validos (com digito verificador) ----------------
def _dv(nums, pesos):
    s = sum(n * p for n, p in zip(nums, pesos))
    r = s % 11
    return 0 if r < 2 else 11 - r


def gerar_cpf():
    n = [random.randint(0, 9) for _ in range(9)]
    n.append(_dv(n, range(10, 1, -1)))
    n.append(_dv(n, range(11, 1, -1)))
    return "".join(map(str, n))


def gerar_cnpj():
    n = [random.randint(0, 9) for _ in range(8)] + [0, 0, 0, 1]
    p1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    p2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    n.append(_dv(n, p1))
    n.append(_dv(n, p2))
    return "".join(map(str, n))


# ---------------- dados realistas ----------------
NOMES = [
    "Maria da Silva Santos", "Jose Pereira de Oliveira", "Ana Carolina Souza",
    "Joao Batista Ferreira", "Antonia Rodrigues Lima", "Carlos Eduardo Almeida",
    "Francisca das Chagas Costa", "Paulo Henrique Martins", "Sandra Regina Gomes",
    "Marcos Vinicius Araujo", "Luiza Helena Carvalho", "Roberto Carlos Nunes",
]
EMPRESAS = [
    "Construtora Horizonte Ltda", "Transportes Litoral S.A.",
    "Comercial Boa Vista Ltda", "Industria Metalurgica Aurora S.A.",
]
TRIBUNAIS = ["TJSP", "TJRJ", "TJMG", "TJBA", "TJRS", "TRF-1", "TRF-3", "TJPR", "TJPE"]
COD_TRIB = {"TJSP": "8.26", "TJRJ": "8.19", "TJMG": "8.13", "TJBA": "8.05",
            "TJRS": "8.21", "TRF-1": "4.01", "TRF-3": "4.03", "TJPR": "8.16", "TJPE": "8.17"}

AND_TIPOS = ["ObservacaoInterna", "ContatoRealizado", "DocumentoRecebido",
             "PropostaEnviada", "PropostaRecebida", "AtualizacaoStatus"]
AND_TEXTOS = {
    "ObservacaoInterna": ["Analise inicial do processo realizada.", "Verificada a regularidade da documentacao.", "Caso priorizado conforme valor."],
    "ContatoRealizado": ["Contato telefonico com o credor para atualizacao.", "E-mail enviado solicitando documentos pendentes.", "Reuniao agendada com o cliente."],
    "DocumentoRecebido": ["Procuracao assinada recebida.", "Certidao de objeto e pe anexada.", "Comprovante de inscricao do precatorio recebido."],
    "PropostaEnviada": ["Proposta de cessao enviada ao credor com desagio de 22%.", "Enviada proposta revisada apos negociacao."],
    "PropostaRecebida": ["Credor sinalizou aceite da proposta.", "Contraproposta recebida do credor."],
    "AtualizacaoStatus": ["Processo em fase de homologacao.", "Aguardando publicacao no diario oficial."],
}


def numero_cnj(seq, ano, trib):
    dd = random.randint(10, 99)
    orig = random.randint(1, 999)
    return f"{seq:07d}-{dd}.{ano}.{COD_TRIB[trib]}.{orig:04d}"


# ---------------- execucao ----------------
def listar_tudo(caminho):
    st, pg = req("GET", f"/{caminho}?pagina=1&tamanho=100")
    return pg.get("items", []) if st == 200 and pg else []


def main():
    global TOKEN
    login = ler_env("Auth__Login")
    senha = ler_env("Auth__Senha")
    st, body = req("POST", "/auth/login", {"login": login, "senha": senha})
    if st != 200:
        raise SystemExit(f"Falha no login ({st}): {body}")
    TOKEN = body["token"]
    print("Login OK.")

    # ---- clientes (cria so se nao houver) ----
    clientes = listar_tudo("clientes")
    if clientes:
        print(f"Clientes ja existentes: {len(clientes)} (mantidos)")
    else:
        pessoas = [(n, gerar_cpf()) for n in NOMES]
        juridicas = [(n, gerar_cnpj()) for n in EMPRESAS]
        for i, (nome, doc) in enumerate(pessoas + juridicas):
            primeiro = re.sub(r"\W", "", nome.split()[0].lower())
            payload = {
                "nome": nome, "documento": doc,
                "email": f"{primeiro}{i}@exemplo.com.br",
                "telefone": f"(11) 9{random.randint(1000,9999)}-{random.randint(1000,9999)}",
            }
            st, body = req("POST", "/clientes", payload)
            if st in (200, 201):
                clientes.append(body)
            else:
                print(f"  cliente '{nome}' falhou ({st}): {body}")
        print(f"Clientes criados: {len(clientes)}")

    # ---- precatorios (cria so se nao houver) ----
    precatorios = listar_tudo("precatorios")
    if precatorios:
        print(f"Precatorios ja existentes: {len(precatorios)} (mantidos)")
    else:
        seq = 1000
        for i in range(24):
            seq += random.randint(1, 50)
            trib = random.choice(TRIBUNAIS)
            ano = random.randint(2017, 2023)
            valor = round(random.uniform(45_000, 1_900_000), 2)
            cli = random.choice(clientes) if clientes and random.random() < 0.85 else None
            payload = {
                "numero": numero_cnj(seq, ano, trib),
                "tribunalOrigem": trib,
                "valorFace": valor,
                "esfera": random.choice([1, 2, 2, 3]),
                "natureza": random.choice([1, 2, 2]),
            }
            if cli:
                payload["clienteId"] = cli["id"]
            st, body = req("POST", "/precatorios", payload)
            if st in (200, 201):
                precatorios.append(body)
            else:
                print(f"  precatorio {payload['numero']} falhou ({st}): {body}")
        print(f"Precatorios criados: {len(precatorios)}")

    # ---- andamentos (adiciona apenas aos precatorios sem andamento) ----
    total_and = 0
    for p in precatorios:
        existentes = req("GET", f"/precatorios/{p['id']}/andamentos")[1] or []
        if existentes:
            continue
        for _ in range(random.randint(2, 4)):
            tipo = random.choice(AND_TIPOS)
            desc = random.choice(AND_TEXTOS[tipo])
            st, b = req("POST", f"/precatorios/{p['id']}/andamentos",
                        {"descricao": desc, "tipo": tipo})
            if st in (200, 201):
                total_and += 1
            else:
                print(f"  andamento falhou ({st}): {b}")
    print(f"Andamentos registrados: {total_and}")

    # ---- pagamentos (~40% dos precatorios em aberto -> Liquidado, com desagio) ----
    abertos = [p for p in precatorios if p.get("status") not in ("Liquidado", "Cancelado")
               and not (req("GET", f"/precatorios/{p['id']}/pagamentos")[1] or [])]
    total_pag = 0
    for p in random.sample(abertos, k=max(1, int(len(abertos) * 0.4))) if abertos else []:
        base = p["valorFace"]
        valor_pago = round(base * random.uniform(0.68, 0.88), 2)  # desagio 12%-32%
        dias = random.randint(5, 200)
        pago_em = (datetime.now(timezone.utc) - timedelta(days=dias)).strftime("%Y-%m-%d")
        st, body = req("POST", f"/precatorios/{p['id']}/pagamentos",
                       {"valorPago": valor_pago, "pagoEm": pago_em})
        if st in (200, 201):
            total_pag += 1
        else:
            print(f"  pagamento falhou ({st}): {body}")
    print(f"Pagamentos registrados (precatorios liquidados): {total_pag}")
    print("\nSeed concluido.")


if __name__ == "__main__":
    main()
