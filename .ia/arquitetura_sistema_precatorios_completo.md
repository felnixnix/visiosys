# Especificação de Arquitetura e Requisitos de Software

**Projeto:** Sistema de Gestão e Assessoria de Precatórios  
**Autor:** Felipe de Araújo  
**Função:** Software developer  
**Data:** Junho de 2026  

---

## 1. Visão Geral do Sistema
O sistema tem como objetivo gerir o ciclo de vida de precatórios (ativos judiciais), abrangendo desde a captação do credor, análise jurídica, cálculo de deságio, captação automática de andamentos, até a negociação final e liquidação do ativo. A plataforma será composta por uma **API REST** robusta de alta performance, um **Worker Service** para processamento em background, e uma interface web dinâmica (**Single Page Application**).

---

## 2. Pilha Tecnológica (Stack) e Padrões Arquiteturais

O projeto adota uma filosofia **".NET Native"**, priorizando os recursos oficiais e embutidos do ecossistema da Microsoft para reduzir a dependência de pacotes de terceiros e simplificar a manutenção.

* **Padrão Arquitetural de Software:** Domain-Driven Design (DDD) com Modelos de Domínio Ricos (Rich Domain Models).
* **Backend Core:** C# / .NET (versão 8.0+)
* **Processamento em Background:** .NET Worker Services (BackgroundService nativo)
* **Frontend:** React
* **Banco de Dados Transacional:** PostgreSQL (dados estruturados, financeiros e integridade relacional)
* **Banco de Dados Documental:** MongoDB (logs complexos, auditoria de acessos e histórico processual)
* **Armazenamento de Ficheiros (Object Storage):** AWS S3
* **Observabilidade e Dashboards:** Seq (ingestão de logs estruturados JSON)
* **Ambiente de Execução:** Docker / Docker Compose
* **Infraestrutura de Hospedagem:** AWS EC2 (Instância `t4g.medium` - ARM/Graviton)
* **Gerenciamento de Domínio:** AWS Route 53
* **Orquestração de CI/CD:** GitHub Actions
* **Ferramenta de Infraestrutura como Código:** Terraform

---

## 3. Requisitos Funcionais (RF)

| ID | Módulo | Descrição do Requisito |
| :--- | :--- | :--- |
| **RF01** | Cadastro de Clientes | Permitir o registo completo de credores (dados pessoais, contatos e informações bancárias para pagamento). |
| **RF02** | Gestão de Precatórios | Permitir o registo detalhado do ativo judicial (esfera municipal/estadual/federal, tribunal de origem, valor de face, natureza alimentar ou comum). |
| **RF03** | Cálculo de Deságio | Calcular automaticamente propostas de compra aplicando taxas de deságio parametrizáveis sobre o valor atualizado do precatório. |
| **RF04** | Gestão de Documentos | Permitir o upload, armazenamento seguro e associação de ficheiros PDF (procurações, certidões, contratos e petições). |
| **RF05** | Histórico de Andamentos | Manter um log cronológico e imutável de todas as interações com o cliente e movimentações processuais. |
| **RF06** | Controle de Pagamentos | Gerir os status financeiros do ativo (aguardando pagamento, liquidado, repasse efetuado). |
| **RF07** | Portal de Ajuda e Integração | Disponibilizar uma secção no frontend com a documentação de uso do sistema para os operadores, contendo um link de acesso direto para a documentação técnica da API (Swagger). |
| **RF08** | Captura Automática de Dados | O sistema deve capturar de forma automatizada e assíncrona as atualizações e andamentos dos precatórios diretamente de fontes públicas (como portais de transparência), notificando o painel operacional sobre mudanças de status. |

---

## 4. Requisitos Não Funcionais (RNF)

| ID | Categoria | Descrição do Requisito |
| :--- | :--- | :--- |
| **RNF01** | Arquitetura de Backend | A API REST e os serviços de background devem ser desenvolvidos estritamente com recursos nativos do ecossistema C# .NET. |
| **RNF02** | Arquitetura de Frontend | A interface do utilizador deve ser uma SPA construída em React. |
| **RNF03** | Persistência Relacional | Toda a consistência transacional, dados de clientes e conciliação financeira deve ser garantida via PostgreSQL. |
| **RNF04** | Persistência NoSQL | O histórico dinâmico de processos e as trilhas de auditoria complexas devem ser armazenados no MongoDB. |
| **RNF05** | Armazenamento Remoto | Ficheiros físicos (PDFs) submetidos não podem ser persistidos no disco local da EC2. Devem ser armazenados no AWS S3, guardando apenas as chaves de referência e URLs no PostgreSQL. |
| **RNF06** | Infraestrutura | Toda a aplicação deve ser conteinerizada via Docker e hospedada em instâncias virtuais AWS EC2 baseadas na arquitetura ARM (`t4g.medium`). |
| **RNF07** | Automação (CI/CD) | O pipeline de compilação, testes e publicação deve ser implementado nativamente através do GitHub Actions. |
| **RNF08** | Observabilidade | O registo de logs estruturados em formato JSON deve utilizar a biblioteca padrão `Microsoft.Extensions.Logging` e ser centralizado no Seq. |
| **RNF09** | Segurança (Autenticação) | Toda a comunicação com a API REST deve ser protegida por autenticação e autorização via tokens JWT. A interface do Swagger deve suportar a injeção do token via fluxo `Bearer`. |
| **RNF10** | Documentação de API | A API deve expor os seus endpoints através do padrão OpenAPI (Swagger UI). |
| **RNF11** | Infraestrutura como Código | Todo o provisionamento da AWS deve ser feito de forma declarativa utilizando **Terraform**. |
| **RNF12** | Privacidade e LGPD | O sistema deve manter uma trilha de auditoria estrita no MongoDB registando qual utilizador leu ou modificou dados financeiros sensíveis. |
| **RNF13** | Resiliência (Health Checks) | A API e o Worker devem expor endpoints nativos de diagnóstico de saúde (`/health`). |
| **RNF14** | Disaster Recovery | Backups lógicos e snapshots dos volumes EBS devem ser automatizados e exportados para um bucket AWS S3 isolado. |
| **RNF15** | Controlo de Concorrência | A aplicação deve utilizar Concorrência Otimista (Optimistic Concurrency) via EF Core para evitar sobrescrita de dados concorrentes (Lost Updates) em precatórios e negociações. |
| **RNF16** | Gestão de Segredos | Nenhuma credencial pode ser guardada no código-fonte. O sistema utilizará os GitHub Secrets no pipeline CI/CD, injetando as chaves apenas como Variáveis de Ambiente na infraestrutura de produção. |
| **RNF17** | Proteção contra Força Bruta | Endpoints de autenticação e outras rotas sensíveis devem implementar limite de taxa (Rate Limiting) nativo do .NET 8, evitando sobrecarga e ataques ao sistema. |
| **RNF18** | Governança de IA | O sistema deve conter ficheiros de configuração estritos (`rules.md`, `agents.md`, `skills.md`) para governar e limitar a atuação de agentes de inteligência artificial assistentes no repositório. |

---

## 5. Práticas de Engenharia e Qualidade de Código

* **Test-Driven Development (TDD):** O ciclo de desenvolvimento será estritamente guiado por testes (Testcontainers para integração real com Postgres/Mongo).
* **Padrão de Commit (Git):** Commits atómicos em **Português (pt-BR)** (`feat:`, `fix:`, etc.). É estritamente proibido incluir qualquer menção referenciando o uso de IA.
* **Gerenciamento de Infraestrutura Assistida (IaC + MCP):** O uso de servidores MCP para automação deve ser direcionado para a escrita e validação de ficheiros `.tf` (Terraform), versionando as alterações no Git.

---

## 6. Diretrizes de Implementação e Resiliência (.NET Native)

* **Concorrência Otimista:** Uso de `[Timestamp]` ou `RowVersion` no mapeamento das entidades do Entity Framework Core para rejeitar automaticamente edições simultâneas e conflituantes.
* **Worker Services e Resiliência de Rede:** Para extrair dados de portais de transparência, o Worker usará a biblioteca `Microsoft.Extensions.Http.Resilience` (políticas baseadas em Polly). Em caso de instabilidade nos servidores governamentais, aplicam-se re-tentativas automáticas (Exponential Backoff) e circuit breakers. Note-se que o foco atual das extrações são portais de transparência genéricos.
* **Rate Limiting Nativo:** Configuração de `builder.Services.AddRateLimiter()` protegendo endpoints críticos contra ataques repetitivos.
