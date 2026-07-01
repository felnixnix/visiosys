# ADR-025 — Sink Serilog próprio para gravar logs no MongoDB

**Status:** Aceito  
**Data:** 2026-07-01

## Contexto

A dashboard de observabilidade lê logs estruturados de uma coleção `logs` no
MongoDB (ver `LogRepository`). Para popular essa coleção, a primeira implementação
usou o pacote **`Serilog.Sinks.MongoDB 5.0.0`**.

Esse pacote foi compilado contra **`MongoDB.Driver 2.13.1`**, enquanto a camada
`Visiosys.Infrastructure` usa **`MongoDB.Driver 2.28.0`**. O descasamento causou
uma sequência de falhas:

- **Em build:** erros CS0012 no projeto `Api` sempre que o sink era configurado em
  código, porque a assinatura dos métodos do sink expõe tipos do `MongoDB.Driver`
  numa versão que o `Api` não referencia.
- **Em runtime (produção):** ao mover a configuração do sink para o `appsettings`
  (via reflection), o build passou, mas em **publish single-file** só existe **um**
  `MongoDB.Driver.dll` empacotado (o 2.28.0). Quando o Serilog invocava o método do
  sink por reflection, a resolução da assinatura falhava e a aplicação lançava
  exceção **no startup**, entrando em crash-loop no systemd.

O problema é estrutural: qualquer sink de terceiros que exponha tipos do
`MongoDB.Driver` numa versão diferente da nossa é incompatível com o nosso publish.

## Decisão

Remover o `Serilog.Sinks.MongoDB` e implementar um **sink próprio** na camada
`Visiosys.Infrastructure` (`MongoLogSink`), que usa o **mesmo `MongoDB.Driver 2.28.0`**
já presente na camada — o mesmo driver usado por `MongoAuditLogService`.

Características:

- **Sem conflito de versão:** o sink e o resto da aplicação compartilham o único
  driver referenciado.
- **Sem reflection:** o sink é configurado em código, por um método de extensão
  (`WriteTo.MongoDbLogs(connectionString, "logs")`) cuja assinatura pública expõe
  **apenas** tipos do Serilog e strings — nunca tipos do `MongoDB.Driver`. Isso
  elimina o CS0012 no `Api` e a descoberta de assembly por reflection que quebrava
  em single-file.
- **Formato acoplado ao leitor:** o documento gerado casa exatamente com o que o
  `LogRepository` lê (`Timestamp`, `Level`, `RenderedMessage`, `Properties.*`,
  `Exception`), tornando o contrato explícito.
- **Escrita assíncrona em lote:** os eventos entram numa fila limitada
  (`Channel`, política `DropWrite`) drenada por um worker de fundo que faz
  `InsertMany`, sem bloquear a thread que emite o log. Sob pico, prefere descartar
  logs a degradar a aplicação.
- **Reuso da connection string:** o sink é ativado a partir de
  `ConnectionStrings:Mongo`, que já existe em produção. Nenhuma variável de ambiente
  nova é necessária.
- **Falha isolada:** o `MongoClient` conecta de forma lazy e erros de escrita são
  reportados via `SelfLog` do Serilog — uma indisponibilidade do Mongo **não**
  derruba a aplicação.

Adicionalmente, o bloco `catch` do `Program.cs` passou a **relançar** a exceção de
startup (antes engolia e saía com código 0), para que uma falha de inicialização
seja visível ao systemd e ao health gate do deploy, em vez de virar crash-loop
silencioso.

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| Subir a versão do `Serilog.Sinks.MongoDB` | Risco de novo descasamento de versão do driver; a incompatibilidade com single-file permaneceria latente |
| Alinhar todo o projeto à versão de driver do pacote | Forçaria downgrade do `MongoDB.Driver` e revisão de todo o código de persistência documental |
| Ler os logs de outra fonte (ex.: Seq) | Exigiria infra adicional em produção e refatorar a dashboard, que já consulta o Mongo |

## Consequências

- Uma dependência externa a menos (`Serilog.Sinks.MongoDB` removido).
- O comportamento do sink (formato, batching, tratamento de falha) fica sob nosso
  controle e testável.
- `Visiosys.Infrastructure` passa a referenciar `Serilog` (core) para expor o método
  de extensão e implementar `ILogEventSink`.
- Validado localmente de ponta a ponta: requisições geram documentos na coleção
  `logs` com os campos e tipos esperados pela dashboard (`StatusCode` inteiro,
  `Elapsed` em ponto flutuante para as agregações de média).
