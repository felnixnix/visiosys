# ADR-026 — Swagger em produção acessível também pela conta demo

**Status:** Aceito (revisa o [ADR-022](ADR-022-swagger-producao-basic-auth.md))  
**Data:** 2026-07-01

## Contexto

O [ADR-022](ADR-022-swagger-producao-basic-auth.md) protegeu o Swagger em produção
com Basic Auth, reaproveitando **apenas** as credenciais administrativas, para não
expor o contrato completo da API publicamente.

Na prática, o Visiosys é um **projeto de portfólio**: o tour guiado apresenta o
Swagger como um dos recursos e o público-alvo (recrutadores, revisores técnicos)
acessa o sistema pela **conta de demonstração** (`user` / `user`). Com a regra
anterior, esse visitante seguia o tour, clicava em Swagger e era barrado por não ter
a senha de administrador — quebrando justamente o que a demonstração quer mostrar.

## Decisão

O Basic Auth do Swagger passa a aceitar **as credenciais de acesso ao sistema em
ambos os níveis**: administrador (`Auth:Login` / `Auth:Senha`) **ou** conta demo
(`Auth:DemoLogin` / `Auth:DemoSenha`).

A barreira contra indexação anônima e crawlers permanece — o acesso ainda exige
login. Apenas se amplia o conjunto de credenciais válidas para incluir a conta demo,
que é pública e propositalmente compartilhada no contexto do portfólio.

A página de Ajuda foi atualizada para refletir que a conta demo também dá acesso.

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| Manter só admin (ADR-022) | Quebra o showcase: o visitante demo não vê a documentação da API |
| Tornar o Swagger totalmente público | Exporia o contrato a qualquer bot/crawler sem nenhuma barreira |

## Consequências

- Quem explora o portfólio pela conta demo alcança o Swagger, coerente com o tour.
- A proteção contra acesso anônimo/automatizado é mantida (login continua obrigatório).
- A decisão do ADR-022 permanece registrada; este ADR a revisa sem reescrevê-la.
