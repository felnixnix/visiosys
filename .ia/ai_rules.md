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

## 4. Documentação Viva
* **Atualização obrigatória:** Nenhuma tarefa que altere arquitetura, infraestrutura, regras de negócio ou comportamento observável da API é considerada concluída sem a atualização da documentação correspondente (`.ia/roteiro_desenvolvimento.md`, `.ia/README_ARQUITETURA.md` e/ou um novo ADR em `docs/adr/`). A documentação faz parte da entrega, não um passo opcional posterior.
* **Critério do que documentar:** Decisões arquiteturais (escolha de tecnologia, padrão de deploy, modelo de dados) exigem um novo ADR. Correções de bugs de domínio/API com efeito observável (ex: contrato de serialização, regras de validação) exigem ao menos uma menção no roteiro. Scripts/ferramentas versionadas (seeders, utilitários de infra) exigem registro no roteiro ou README de arquitetura informando propósito e como executar.
* **Imutabilidade dos ADRs:** ADRs existentes nunca são reescritos para refletir mudanças; cria-se um novo ADR referenciando o anterior, e apenas a linha `Status:` do ADR superado é anotada (ver `docs/adr/README.md`).
* **Checkpoint de fim de tarefa:** Antes de considerar qualquer entrega finalizada, o assistente deve verificar explicitamente se a documentação ainda reflete o estado real do sistema e atualizá-la na mesma sessão/commit da mudança, não depois.
