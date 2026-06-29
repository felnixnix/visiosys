# ADR-001: Arquitetura em Camadas com DDD e Modelos de Domínio Ricos

**Status:** Aceito  
**Data:** 2026-06-29  
**Requisitos:** RNF01

---

## Contexto

O sistema gerencia um domínio jurídico-financeiro complexo: precatórios têm regras de transição de status rígidas, cálculo de deságio com invariantes matemáticas, e clientes com validação de CPF/CNPJ. Modelar essa lógica em controllers ou serviços anêmicos dispersaria as regras, tornando-as difíceis de testar e de manter à medida que o domínio crescesse.

---

## Decisão

Adotar **Domain-Driven Design (DDD)** com **Modelos de Domínio Ricos** organizados em quatro camadas:

- **Domain:** entidades, value objects, interfaces de repositório, regras de negócio. Sem dependências externas.
- **Application:** casos de uso (use cases), DTOs, interfaces de serviços (ports). Orquestra o domínio.
- **Infrastructure:** implementações concretas (EF Core, MongoDB, S3, HTTP). Depende de Domain e Application.
- **API / Worker:** entrypoints. Apenas wiring de DI e roteamento HTTP.

Entidades como `Precatorio`, `Cliente` e `Pagamento` encapsulam suas invariantes:
- Construtores e factory methods controlam a criação válida.
- Setters `private set` impedem mutação externa.
- Métodos como `AvancarStatus()`, `AtualizarValor()` e `Registrar()` encapsulam as transições.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Transaction Script** (lógica nos controllers/services) | Sem separação de concerns; regras de negócio acopladas à camada HTTP; difícil de testar unitariamente |
| **Anemic Domain Model** | Entidades apenas com getters/setters; regras espalhadas em múltiplos serviços; viola o princípio de encapsulamento |
| **CQRS + Event Sourcing** | Complexidade desproporcional ao volume atual; a equipe é pequena; pode ser adotado futuramente em módulos específicos |

---

## Consequências

**Positivas:**
- Regras de negócio testáveis sem infraestrutura (xUnit puro no Domain.Tests).
- Domínio protegido: qualquer tentativa de criar um `Precatorio` inválido lança exceção na própria entidade.
- Extensível: novas entidades seguem o padrão já pavimentado sem alterar a estrutura.

**Negativas / Trade-offs:**
- Mais código inicial (interfaces, use cases, DTOs separados dos modelos).
- Curva de aprendizado para colaboradores acostumados com Active Record ou MVC simples.
