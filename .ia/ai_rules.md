# Regras de Atuação (AI Rules)

Este documento define as diretrizes estritas de código e comportamento para qualquer assistente de Inteligência Artificial atuando neste repositório.

## 1. Engenharia de Software e Código
* **Filosofia ".NET Native":** Priorizar bibliotecas nativas do .NET 8 (ex: `System.Text.Json`, `Microsoft.Extensions.Logging`, `BackgroundService`). É proibida a sugestão de pacotes de terceiros sem justificativa explícita de ausência de recurso nativo.
* **Domain-Driven Design (DDD):** Entidades (ex: `Precatorio.cs`) não devem ser anémicas. Propriedades devem possuir `private set`. Toda alteração de estado deve ocorrer através de métodos de negócio que incluam validações internas.
* **Test-Driven Development (TDD):** A geração de código funcional deve ser precedida ou acompanhada pela escrita do respetivo teste no xUnit. Nenhum domínio pode existir sem cobertura de testes.

## 2. Padrão de Versionamento (Git)
* **Idioma:** Todas as mensagens de commit devem ser escritas em Português (pt-BR).
* **Semântica:** Utilizar os prefixos do Conventional Commits (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`, `test:`).
* **Proibição Expressa:** Nunca incluir frases como "gerado por IA", "auxiliado por IA", "IA-generated" ou similares nas mensagens de commit. O histórico deve ser exclusivamente técnico e assinado pela equipa humana.

## 3. Infraestrutura
* **Modo Declarativo:** A infraestrutura deve ser sempre provisionada via Terraform (`.tf`). 
* **Proibição CLI AWS:** É estritamente proibido gerar ou executar comandos imperativos da AWS CLI (`aws ec2 run-instances`, etc.) para criar recursos de produção.
