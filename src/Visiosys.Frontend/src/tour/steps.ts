import type { Step } from 'react-joyride';

export const passosTour: Step[] = [
  {
    target: 'body',
    content:
      'Bem-vindo ao Visiosys! Plataforma de gestão de precatórios desenvolvida com .NET, React e infraestrutura na AWS. Este tour rápido apresenta os principais recursos do sistema.',
    placement: 'center',
    disableBeacon: true,
  },
  {
    target: '[data-tour="nav-precatorios"]',
    content:
      'Lista de precatórios: a tela principal. Aqui você acompanha todos os ativos judiciais — número do processo, tribunal, valor de face e status atual.',
    placement: 'bottom',
    disableBeacon: true,
  },
  {
    target: '[data-tour="nav-clientes"]',
    content:
      'Gerenciamento de clientes (credores). Cada precatório está vinculado ao credor que detém o direito ao recebimento.',
    placement: 'bottom',
    disableBeacon: true,
  },
  {
    target: '[data-tour="nav-ajuda"]',
    content:
      'Documentação do sistema e acesso à interface interativa da API (Swagger). Ideal para explorar os endpoints disponíveis.',
    placement: 'bottom',
    disableBeacon: true,
  },
  {
    target: '[data-tour="btn-novo"]',
    content:
      'Cadastre um novo precatório informando número do processo, tribunal de origem, valor de face, esfera e o credor cedente.',
    placement: 'bottom',
    disableBeacon: true,
  },
  {
    target: '[data-tour="tabela"]',
    content:
      'A tabela exibe os precatórios paginados. Clique no número do processo para abrir o detalhe completo: andamentos, pagamentos e documentos anexados.',
    placement: 'top',
    disableBeacon: true,
  },
  {
    target: '[data-tour="badge-status"]',
    content:
      'O status indica a fase do precatório: Em Análise → Ag. Documentação → Em Negociação → Ag. Pagamento → Liquidado. Cada transição é registrada no histórico de andamentos.',
    placement: 'auto',
    disableBeacon: true,
  },
  {
    target: '[data-tour="btn-tour"]',
    content: 'Você pode reiniciar este tour a qualquer momento clicando aqui.',
    placement: 'bottom',
    disableBeacon: true,
  },
];
