# ADR-008: Autenticação JWT Stateless

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF09

---

## Contexto

A API REST precisa proteger todos os endpoints contra acesso não autorizado. O modelo de autenticação precisa ser compatível com um SPA React (cliente separado do servidor) e com futuras integrações entre serviços. Manter estado de sessão no servidor (cookies de sessão) criaria dependência de estado que dificulta escalabilidade horizontal.

---

## Decisão

Usar **JWT (JSON Web Token)** com assinatura HMAC-SHA256 para autenticação stateless.

- Endpoint `/api/auth/login` valida credenciais e retorna o token.
- O SPA armazena o token em `sessionStorage` (expirado ao fechar a aba, sem risco de CSRF).
- Cada requisição subsequente envia o token no header `Authorization: Bearer {token}`.
- O middleware `JwtBearer` do ASP.NET Core valida assinatura, emissor, audiência e expiração em cada requisição — sem consulta ao banco.
- A chave JWT e as credenciais de login residem em variáveis de ambiente (ver ADR-019), nunca no código.
- O Swagger UI suporta injeção do token via fluxo `Bearer` para facilitar testes manuais da API.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Cookies de sessão (Session)** | Requer estado no servidor; incompatível com múltiplas instâncias sem Redis/sticky sessions |
| **ASP.NET Core Identity** | Framework completo com banco de dados de usuários; overhead excessivo para a fase atual (usuário único de backoffice) |
| **OAuth2 / OpenID Connect (Keycloak, Auth0)** | Correto para multi-tenant ou SSO empresarial; complexidade desnecessária para MVP |
| **API Keys estáticas** | Sem expiração nem controle granular; inadequado para interface de usuário |

---

## Consequências

**Positivas:**
- Stateless: qualquer instância da API valida qualquer token sem consulta ao banco ou cache compartilhado.
- Portável: o mesmo token pode ser usado por clientes web, mobile ou serviços internos.
- Expiração configurável: `Jwt:ExpiraEmMinutos` por ambiente.

**Negativas / Trade-offs:**
- Tokens JWT não podem ser revogados antes de expirar (sem implementar uma blocklist, o que adiciona estado). Para o perfil atual (backoffice interno), a expiração de 8h é aceitável.
- `sessionStorage` perde o token ao fechar a aba — o usuário precisa fazer login novamente em cada sessão. Escolha intencional de segurança para dados jurídico-financeiros.
