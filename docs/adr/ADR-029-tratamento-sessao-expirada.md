# ADR-029 — Tratamento centralizado de sessão expirada (401)

**Status:** Aceito  
**Data:** 2026-07-03

## Contexto

A autenticação usa um único token JWT de acesso, com validade de 8 horas
(`Jwt__ExpiraEmMinutos=480`), sem refresh token. Quando o token expira, qualquer
requisição autenticada passa a retornar **401**.

No comportamento inicial, cada página tratava a falha da requisição no seu próprio
`catch`, exibindo um erro genérico ("Erro ao carregar" com botão "tentar
novamente"). Para o 401 de sessão expirada isso era confuso: o retry falharia de
novo, e o usuário ficava preso numa tela de erro em vez de ser levado ao login.

## Decisão

Centralizar o tratamento de 401 no **cliente da API** (`src/api/client.ts`), num
único ponto por onde todas as requisições passam. A regra distingue os dois tipos
de 401:

- **401 com token presente** significa sessão expirada ou token inválido. O cliente
  limpa o token, sinaliza a expiração e redireciona para a tela de login, que exibe
  o aviso "Sua sessão expirou. Faça login novamente.".
- **401 sem token** (por exemplo, tentativa de login com senha errada, que não envia
  `Authorization`) continua tratado por quem chamou. Assim a tela de login mostra
  "Credenciais inválidas" sem redirecionar.

Detalhes:

- **Guarda contra loop:** o redirect é ignorado se a rota atual já for `/login`.
- **Reload completo** (`window.location.assign`) em vez de navegação client-side: um
  evento raro (expiração) em que zerar o estado do app e reinicializar a
  autenticação a partir do storage é mais simples e robusto do que propagar o
  evento do cliente (não-React) para o contexto de autenticação.

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| Tratar o 401 em cada página | Repetitivo, propenso a inconsistência e fácil de esquecer numa tela nova |
| Ponte do cliente para o `AuthContext` (redirect via React Router) | Mais "SPA", mas exige acoplar o cliente não-React ao estado do React; o reload é suficiente para um evento raro |
| Refresh token com renovação automática | Fora de escopo: o modelo atual usa um único token de acesso; renovação silenciosa é uma evolução, não uma correção deste bug |

## Consequências

- Comportamento consistente de expiração em todas as telas, presentes e futuras.
- UX clara: o usuário vai ao login e entende o porquê pelo aviso.
- O 401 de senha errada no login permanece local, sem redirect nem loop.
- Custo: um reload completo ao expirar (aceitável, dado que é raro).
- Verificado em produção forjando um token inválido: a navegação seguinte
  redireciona ao login com o aviso.
