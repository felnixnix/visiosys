# Jornada de Arquitetura e Racional de Desenvolvimento

**Projeto:** Sistema de Gestão e Assessoria de Precatórios  
**Autor:** Felipe de Araújo  
**Função:** Software developer  
**Data:** Junho de 2026  

---

## 1. Introdução e Visão do Engenheiro

Este documento funciona como um diário de bordo arquitetural e guia de racional descritivo para o Sistema de Gestão e Assessoria de Precatórios. 

Para sustentar um cenário jurídico-financeiro complexo sem gerar dívida técnica precoce, a arquitetura assenta nestes pilares:
1. **Isolamento de regras de negócio (DDD/Domínios Ricos).**
2. **Confiança na suíte de testes (TDD + Testcontainers).**
3. **Soberania do ecossistema nativo (.NET Native + IaC Declarativa).**
4. **Resiliência e Conformidade de Produção (Armazenamento Remoto, Seq, LGPD, Proteções de Rede e Concorrência).**

---

## 2. Modelos Ricos e Filosofia Nativa (.NET 8)

Optou-se por **Domain-Driven Design (DDD)** para blindar a lógica. Entidades como `Precatorio` contêm os seus próprios comportamentos, rejeitando as tradicionais classes anémicas.
A filosofia **".NET Native"** ditou escolhas robustas: WebApplicationFactory para testes, Logs Estruturados (JSON) no host e rotinas em background (`BackgroundService`) sem dependências externas pesadas.

---

## 3. O Refinamento da "Última Milha" de Produção

As decisões arquiteturais focaram-se na mitigação de falhas catastróficas comuns em sistemas transacionais web:

* **Evitar "Lost Updates" (Concorrência Otimista):** Dois operadores nunca podem sobrescrever o trabalho um do outro no mesmo precatório. Implementámos mecanismos de *Optimistic Concurrency* utilizando propriedades de controlo do EF Core. Se houver edição simultânea, a transação é bloqueada para salvaguardar a consistência financeira.
* **Gestão Rigorosa de Segredos:** Evitando vetores de vulnerabilidade, todas as chaves (AWS, JWT, DBs) residem encriptadas nos GitHub Secrets. O CI/CD injeta-as no ambiente Docker estritamente como variáveis de ambiente voláteis.
* **Proteção Nativa (Rate Limiting):** Como a aplicação será exposta publicamente na AWS, implementou-se o middleware nativo de *Rate Limiting* do .NET 8, travando tentativas de login por força bruta e garantindo a saúde do servidor sem custos extras de WAF.
* **Tolerância a Falhas na Integração:** O Worker Service fará varreduras em portais de transparência e diários da república para atualizar os processos judiciais. Como são fontes instáveis, incorporou-se a `Microsoft.Extensions.Http.Resilience` para gerir *timeouts* e re-tentativas de forma assíncrona, assegurando a estabilidade.

---

## 4. Persistência e Qualidade Assegurada

A arquitetura usa uma abordagem poliglota: **PostgreSQL** (consistência financeira transacional), **MongoDB** (históricos de auditoria orientados a documentos - LGPD) e **AWS S3** (documentos físicos). A observabilidade global fica a cargo do **Seq**, permitindo consultas JSON avançadas.

---

## 5. Governança de Inteligência Artificial e Agentes Autônomos

Num cenário de desenvolvimento acelerado por ferramentas baseadas em Model Context Protocol (MCP) e agentes de IA, o código e a infraestrutura correm o risco de divergir dos padrões arquiteturais caso não existam fronteiras bem definidas.

Para garantir que a IA atua como uma extensão das decisões da engenharia, implementámos uma **pasta de Governança de IA (`docs/ai/`)**:
* **`rules.md`:** Define as leis irrevogáveis (TDD mandatório, proibição de modelos anémicos, commits padronizados sem menção a IA).
* **`agents.md`:** Delimita as "personas" operacionais, garantindo que o agente DevOps só utilize Terraform para a AWS e o agente Backend respeite o ecossistema .NET Native.
* **`skills.md`:** Limita a capacidade de execução no terminal, exigindo pausas para aprovação humana antes de operações críticas como um `terraform apply`.

Esta camada assegura que a força motriz da IA não corrompa a integridade técnica, mantendo o repositório organizado e o ambiente de produção salvaguardado.

Todo o processo de infraestrutura é declarativo, definido via **Terraform** e aplicado no **GitHub Actions** através de commits atómicos em pt-BR (garantidos sem anotações de auxílio de IA), assegurando que o MVP ganhe vida na cloud sem falhas manuais.
