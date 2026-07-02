# ADR-028 — Busca e filtros nas listagens

**Status:** Aceito  
**Data:** 2026-07-02

## Contexto

As listagens de precatórios e clientes só ofereciam paginação. Com o volume de
dados crescendo (o ambiente de estudos já tem dezenas de registros por lista),
localizar um registro específico exigia paginar manualmente. Era preciso adicionar
busca, mas com campos **condizentes com a natureza de cada página** — os critérios
úteis para clientes (pessoas/empresas) são diferentes dos de precatórios (ativos
judiciais).

## Decisão

Adicionar busca **filtrando no servidor**, com um padrão replicável entre as
camadas:

1. **Um record de filtro por entidade** no domínio (`FiltroClientes`,
   `FiltroPrecatorios`), com campos opcionais que combinam com E (AND).
2. **O repositório aplica os critérios** montando um `IQueryable`: `ILIKE` para
   texto (case-insensitive, "contém") e igualdade para enums.
3. **`Listar` e `Contar` compartilham a mesma query filtrada.** Este é o ponto de
   correção não óbvio: se a contagem ignorasse o filtro, a paginação exibiria um
   total errado e páginas vazias. Um método `Filtrar(filtro)` privado é reutilizado
   pelos dois.
4. **Use case e controller** apenas repassam o filtro; o controller recebe os
   critérios via query string (`[FromQuery]`), inclusive enums (bind por nome ou
   valor numérico).

### Campos por página (adaptação à natureza)

- **Clientes** (credores): nome, CPF/CNPJ, tipo (PF/PJ, derivado do tamanho do
  documento) e **filtro alfabético A-Z** pela inicial do nome — útil para uma lista
  de pessoas.
- **Precatórios** (ativos judiciais): número do processo, tribunal, esfera, status
  e natureza. **Sem A-Z**, que não faz sentido para número de processo.

### UX

O gatilho da busca é um **botão "Filtrar"** (o A-Z de clientes aplica na hora). Ao
mudar o filtro, a listagem volta para a primeira página. O estado vazio distingue
"nenhum cadastrado" de "nenhum encontrado para o filtro".

## Alternativas descartadas

| Alternativa | Razão |
|---|---|
| Filtrar no cliente (frontend) | Não escala: exigiria baixar todos os registros; a paginação e a performance degradam com o volume |
| Busca ao digitar (debounce) | Válida, mas o botão é mais previsível e econômico em requisições; menos ruído de rede |
| Dropdown com tribunais distintos | Mais amigável, mas exige um endpoint de valores distintos; deixado como evolução futura (o texto "contém" resolve o caso atual) |

## Consequências

- Padrão de filtro replicável entre entidades (aplicado igual em clientes e
  precatórios).
- Paginação correta sobre o conjunto filtrado, por construção.
- Cada consulta filtrada é coberta por testes de integração (Testcontainers).
- Evolução natural, se necessário: dropdowns de valores distintos, busca por faixa
  de valor e ordenação configurável.
