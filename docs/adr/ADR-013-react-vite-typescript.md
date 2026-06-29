# ADR-013: React + Vite + TypeScript para o SPA

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF02, RF07

---

## Contexto

O RNF02 especifica que a interface do usuário deve ser uma **SPA construída em React**. A decisão de bundler, linguagem e estrutura de estado é livre. O sistema é um backoffice interno com fluxos relativamente simples (listagens, formulários, detalhes) mas com dados financeiros que exigem tipagem forte.

---

## Decisão

Usar **React 19 + Vite + TypeScript** com:
- **React Router v7** para roteamento client-side (sem framework SSR).
- **`sessionStorage`** para persistência do JWT (sem Redux/Zustand — estado simples não justifica gerenciador externo).
- **`fetch` nativo** com wrapper tipado (`api.get<T>`, `api.post<T>`) — sem Axios.
- **CSS Variables + classes utilitárias** em `index.css` — sem framework CSS (Tailwind, Bootstrap).
- **Vite proxy** em dev: `/api` → `http://localhost:5000` (sem CORS para desenvolvimento).
- **Build output para `wwwroot/`** da API: o ASP.NET Core serve o SPA via `UseStaticFiles()`.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Next.js** | SSR é desnecessário para backoffice interno; add SEO/crawling não são requisitos; complexidade extra |
| **Angular** | Framework completo com opinionismo forte; team size e escopo não justificam a curva |
| **Vue.js** | Excelente, mas RNF02 especifica React explicitamente |
| **Blazor (C#)** | Interessante no ecossistema .NET, mas ecossistema React é mais maduro; componentes de terceiros mais disponíveis |
| **Axios** em vez de fetch | Adiciona dependência; `fetch` nativo em 2026 tem suporte completo e é suficiente |
| **Tailwind CSS** | Ótima opção, mas adiciona build step extra e esconde CSS dos devs menos experientes; CSS Variables atendem a escala atual |

---

## Consequências

**Positivas:**
- Vite: HMR (Hot Module Replacement) instantâneo em dev; build extremamente rápido.
- TypeScript: erros de contrato com a API detectados em tempo de compilação.
- Build gerado em `wwwroot/`: zero configuração extra de servidor web — o próprio ASP.NET Core serve o SPA.
- `MapFallbackToFile("index.html")`: React Router funciona com F5 em rotas client-side.

**Negativas / Trade-offs:**
- Sem SSR: SEO limitado (irrelevante para backoffice interno autenticado).
- CSS manual: consistência visual depende de disciplina; frameworks CSS evitariam retrabalho em equipes maiores.
- `sessionStorage`: token perdido ao fechar aba — usuário precisa fazer login novamente. Escolha intencional de segurança (ver ADR-008).
