# ADR-022: Exposição do Swagger em Produção Protegida por Basic Auth

**Status:** Aceito
**Data:** 2026-06-30
**Requisitos:** RF07, RNF10
**Relaciona-se com:** complementa o [ADR-008](ADR-008-autenticacao-jwt.md) (autenticação JWT da API).

---

## Contexto

O RF07 exige um portal de ajuda no frontend com link direto para a documentação técnica da API, e o RNF10 exige que a API exponha seus endpoints via OpenAPI/Swagger UI. A implementação original (`Program.cs`) registrava o Swagger apenas sob `app.Environment.IsDevelopment()`, deixando-o indisponível em produção — um requisito formalmente "concluído" no roteiro que, na prática, não se sustentava: a rota `/swagger` retornava 404 e o frontend não tinha nenhuma página de ajuda.

Liberar o Swagger publicamente em produção sem proteção, porém, exporia o contrato completo da API (rotas, modelos, exemplos) a qualquer visitante não autenticado — informação útil para reconhecimento de um eventual atacante.

---

## Decisão

- **Swagger habilitado em todos os ambientes.** Em produção, a rota `/swagger` (UI e `swagger.json`) é protegida por **HTTP Basic Auth**, verificada por um middleware simples antes de `UseSwagger()`/`UseSwaggerUI()` em `Program.cs`.
- As credenciais usadas são as **mesmas credenciais administrativas** já existentes (`Auth:Login` / `Auth:Senha`, lidas de `/etc/visiosys/production.env`) — sem novo segredo a gerenciar.
- Em desenvolvimento, o comportamento não muda: acesso livre, sem Basic Auth.
- **Frontend:** nova página `/ajuda` ([AjudaPage.tsx](../../src/Visiosys.Frontend/src/pages/AjudaPage.tsx)), acessível pelo menu principal, com um resumo do fluxo de uso do sistema e um link para `/swagger`, satisfazendo o RF07.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Manter Swagger só em dev** | Não satisfaz o RNF10 (API deve expor OpenAPI) nem o link exigido pelo RF07 |
| **Liberar Swagger sem proteção em produção** | Expõe o contrato completo da API a qualquer visitante anônimo — risco de reconhecimento desnecessário |
| **Exigir o próprio JWT do sistema para abrir o Swagger UI** | O Swagger UI já suporta injetar o Bearer token *depois* de aberto (ADR-008); exigir JWT só para *ver* a UI implicaria replicar lógica de login fora do fluxo normal da SPA |
| **Restringir `/swagger` por IP no nginx** | Resolveria, mas a equipe e o "cliente" de estudo acessam de IPs variáveis; Basic Auth é mais simples de operar sem reconfigurar infraestrutura a cada acesso |

---

## Consequências

**Positivas:**
- RF07 e RNF10 efetivamente satisfeitos em produção, não apenas em desenvolvimento.
- Nenhum segredo novo: reaproveita `Auth:Login`/`Auth:Senha` já existentes e geridos pelo [ADR-019](ADR-019-gestao-segredos.md).
- Middleware nativo do ASP.NET Core, sem pacote de terceiros (alinhado ao [ADR-002](ADR-002-dotnet-native.md)).

**Negativas / Trade-offs:**
- Basic Auth trafega usuário/senha em Base64 (não criptografado) — aceitável aqui porque a conexão já roda sobre o domínio servido pela aplicação; se HTTPS for ativado (pendência da Fase 5), o Basic Auth passa a viajar dentro do TLS automaticamente, sem mudança de código.
- Reaproveitar a credencial administrativa do sistema para o Swagger acopla os dois acessos: rotacionar a senha admin também invalida o acesso ao Swagger (efeito esperado, não um problema na prática atual).
