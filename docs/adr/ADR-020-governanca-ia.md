# ADR-020: Governança de IA com Regras Declarativas

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF18

---

## Contexto

O desenvolvimento do sistema é acelerado por agentes de IA (LLMs com acesso ao repositório via MCP). Sem fronteiras explícitas, um agente pode: gerar commits com menção de IA (violando a padronização), executar `terraform apply` sem aprovação humana, gerar arquivos `.env` com credenciais reais, ou desviar das decisões arquiteturais (ex: introduzir MediatR quando ADR-002 proíbe). O risco não é a IA em si, mas a ausência de governança sobre sua atuação.

---

## Decisão

Criar a pasta `.ia/` no repositório com arquivos de governança que os agentes leem antes de qualquer ação:

| Arquivo | Propósito |
|---------|-----------|
| `arquitetura_sistema_precatorios_completo.md` | Especificação completa de RFs, RNFs e stack tecnológica |
| `roteiro_desenvolvimento.md` | Sequência de fases e dependências técnicas |
| `README_ARQUITETURA.md` | Racional arquitetural e jornada de decisões |
| `ai_rules.md` | Leis irrevogáveis: TDD mandatório, commits sem menção a IA, proibição de comandos AWS CLI destrutivos |
| `ai_agents.md` | Personas operacionais: agente backend respeita .NET Native, agente DevOps usa apenas Terraform |
| `ai_skills.md` | Limites de execução: `terraform apply` exige PAUSA OBRIGATÓRIA e aprovação humana explícita |

Regras imutáveis aplicadas em todas as sessões:
- Commits em pt-BR com Conventional Commits (`feat:`, `fix:`, etc.) — sem `Co-Authored-By: AI` ou menção a IA.
- `terraform apply` proibido sem aprovação explícita do usuário.
- Arquivos `.env` com credenciais reais nunca gerados — apenas placeholders.
- Modelos de domínio nunca anêmicos (sem setters públicos desnecessários).

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Confiança implícita no comportamento padrão da IA** | Agentes sem restrições explícitas tomam decisões baseadas em treinamento geral, não nas decisões específicas do projeto |
| **Revisão manual de cada sugestão da IA** | Inviável para o ritmo de desenvolvimento; governança declarativa é mais escalável |
| **Sem uso de IA no desenvolvimento** | Elimina ganho de produtividade significativo; governança é preferível à proibição |

---

## Consequências

**Positivas:**
- Consistência: o agente conhece as ADRs e respeita as decisões já tomadas.
- Auditabilidade: o Git nunca revela que IA foi usada — os commits são atribuídos ao desenvolvedor, como deveriam ser.
- Proteção de produção: a PAUSA OBRIGATÓRIA antes de `terraform apply` elimina o risco de provisionamento acidental.

**Negativas / Trade-offs:**
- Governança efetiva apenas enquanto o agente lê e interpreta os arquivos `.ia/` — modelos futuros ou configurações diferentes podem ignorar as regras se não forem explicitamente instruídos.
- Manutenção: os arquivos `.ia/` precisam ser atualizados quando as decisões arquiteturais mudam (ex: um novo ADR deve ser refletido em `ai_rules.md` se criar uma restrição nova).
