# Capacidades e Limites (AI Skills & MCP)

Regras de interação do agente com o ambiente local (Terminal, Sistema de Ficheiros) via Model Context Protocol (MCP).

## 1. Operações de Terminal Autorizadas
* `dotnet build`: Para validar a sintaxe e compilação do código gerado.
* `dotnet test`: Para garantir que a suíte de testes (xUnit + Testcontainers) passa com sucesso após cada iteração.
* `terraform fmt` e `terraform validate`: Para formatar e validar os scripts de infraestrutura.

## 2. Operações com Intervenção Obrigatória
* O agente tem autorização para rodar `terraform plan` autonomamente.
* **PAUSA OBRIGATÓRIA:** É estritamente proibido rodar `terraform apply` sem solicitar aprovação explícita do utilizador humano. O agente deve apresentar o plano e aguardar o "sim".

## 3. Manipulação de Ficheiros Sensíveis
* O agente não deve gerar ficheiros `.env` locais contendo senhas reais ou chaves da AWS.
* Segredos devem ser exemplificados usando placeholders (ex: `AWS_ACCESS_KEY_ID=your_access_key_here`).
