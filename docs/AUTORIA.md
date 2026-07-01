# Autoria, Conhecimento e Uso de IA

## Por que este documento existe

Em 2026, a pergunta sobre um projeto deixou de ser "usou IA?", já que quase todo desenvolvedor usa. A pergunta que importa é **como**. Este documento explica isso de forma aberta: como o Visiosys foi construído, quem tomou as decisões e o que eu realmente entendo do que está aqui.

## IA como copiloto, não como piloto

Desenvolvi o Visiosys com o Claude Code como copiloto. A divisão de trabalho foi clara.

**O que a IA acelerou:** scaffolding e boilerplate, exploração rápida de alternativas de implementação, escrita de testes, detecção mais rápida de bugs (por exemplo, o conflito de versão do sink MongoDB em publish single-file) e a redação de documentação.

**O que foi decisão minha:** a arquitetura (DDD em camadas), o mecanismo de deploy (AWS SSM com OIDC no lugar de SSH com chave estática), a persistência poliglota e o motivo de cada banco, os trade-offs que aceitei conscientemente (como o RDS single-AZ por custo) e o modelo de domínio (o precatório como agregado central, e não o cliente).

Cada decisão relevante está registrada como **ADR** em [`docs/adr/`](adr/), com contexto, alternativas descartadas e consequências. Um ADR não é código gerado: é o registro de uma escolha e do raciocínio por trás dela. A IA amplifica quem a usa. Ela acelera a digitação e a exploração, mas não substitui o entendimento do problema nem a responsabilidade pela decisão.

## Autoavaliação de conhecimento (livro fechado)

Para tornar o "eu entendo o que construí" verificável, e não apenas uma afirmação, respondi a uma bateria técnica de livro fechado sobre as decisões e os fundamentos do próprio projeto. **Resultado: 11 de 12.**

| # | Tema | Resposta correta | Acertei? |
|---|---|---|:--:|
| 1 | OIDC no deploy | Credenciais efêmeras por execução, sem chave estática | ✅ |
| 2 | Concorrência otimista | Evita sobrescrita silenciosa em edição simultânea | ✅ |
| 3 | Bug em single-file | Carga por reflection com descasamento de versão do driver | ✅ |
| 4 | Circuit breaker | Para de chamar a dependência falha por um tempo | ✅ |
| 5 | Índice de banco | Índice composto (status, criado_em) | ✅ |
| 6 | async/await | Libera a thread durante o I/O (mais concorrência) | ✅ |
| 7 | Idempotência | Chave de idempotência contra reenvio duplicado | ✅ |
| 8 | Persistência poliglota | Auditoria é append-only com esquema flexível | ✅ |
| 9 | Stale closure (React) | Trava no valor inicial capturado pelo closure | ✅ |
| 10 | TypeScript | `any` desliga a checagem; `unknown` exige narrowing | ✅ |
| 11 | Réplicas de leitura | Lag de replicação (consistência eventual) | ❌ |
| 12 | JWT stateless | Não dá para revogar antes de expirar sem manter estado | ✅ |

### O erro, sem varrer para baixo do tapete

Errei a questão **11**. Marquei "o custo dobra" quando o trade-off que de fato define réplicas de leitura é o **lag de replicação**: leituras nas réplicas podem retornar dados levemente desatualizados (consistência eventual), porque a propagação a partir do primário leva tempo. É um tema que estou aprofundando na minha formação em System Design. Mantive esse erro aqui de propósito. Se eu listasse só os acertos, isto viraria propaganda no lugar de transparência.

## O que domino e o que ainda estou construindo

**Forte:** arquitetura de aplicação, .NET/C#, AWS, CI/CD, modelagem de domínio e os trade-offs das minhas próprias escolhas.

**Em desenvolvimento:** system design em escala (replicação, sharding, particionamento), que estudo formalmente; e a profundidade que só a experiência em produção, com usuários reais e em time, proporciona.

*Método: a autoavaliação foi conduzida em diálogo, de livro fechado, sem consulta. As perguntas estão resumidas acima para que qualquer pessoa julgue a dificuldade por conta própria.*
