import type { Step } from 'react-joyride';

// Cada passo carrega em `data.route` a rota onde seu alvo existe. O Layout usa
// isso para navegar entre páginas durante o tour (modo controlado): ao avançar
// para um passo de outra rota, ele navega antes de exibir o passo.
type PassoTour = Step & { data: { route: string } };

export const passosTour: PassoTour[] = [
  {
    target: 'body',
    content:
      'Bem-vindo ao Visiosys! Plataforma de gestão de precatórios desenvolvida com .NET, React e infraestrutura na AWS. Este tour rápido apresenta os principais recursos do sistema.',
    placement: 'center',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="nav-precatorios"]',
    content:
      'Lista de precatórios: a tela principal. Aqui você acompanha todos os ativos judiciais — número do processo, tribunal, valor de face e status atual.',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="nav-clientes"]',
    content:
      'Gerenciamento de clientes (credores). Cada precatório está vinculado ao credor que detém o direito ao recebimento.',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="nav-logs"]',
    content:
      'Dashboard de observabilidade. Reúne os logs estruturados do sistema em tempo real — veremos essa tela em detalhe já já.',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="nav-ajuda"]',
    content:
      'Documentação do sistema e acesso à interface interativa da API (Swagger). Ideal para explorar os endpoints disponíveis.',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="btn-novo"]',
    content:
      'Cadastre um novo precatório informando número do processo, tribunal de origem, valor de face, esfera e o credor cedente.',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="tabela"]',
    content:
      'A tabela exibe os precatórios paginados. Clique no número do processo para abrir o detalhe completo: andamentos, pagamentos e documentos anexados.',
    placement: 'top',
    skipBeacon: true,
    data: { route: '/' },
  },
  {
    target: '[data-tour="badge-status"]',
    content:
      'O status indica a fase do precatório: Em Análise → Ag. Documentação → Em Negociação → Ag. Pagamento → Liquidado. Cada transição é registrada no histórico de andamentos.',
    placement: 'auto',
    skipBeacon: true,
    // Alvo depende do carregamento da tabela (inclusive ao voltar de /logs).
    targetWaitTimeout: 3000,
    data: { route: '/' },
  },
  // --- A partir daqui o tour navega para a página de Logs ---
  {
    target: '[data-tour="logs-cards"]',
    content:
      'Esta é a dashboard de observabilidade. Os cards resumem as últimas 24 horas: total de eventos, erros, avisos e tempo médio de resposta. Os dados vêm dos logs estruturados (Serilog) gravados no MongoDB.',
    placement: 'bottom',
    skipBeacon: true,
    // Primeiro passo da rota /logs: dá folga para a navegação renderizar o alvo.
    targetWaitTimeout: 3000,
    data: { route: '/logs' },
  },
  {
    target: '[data-tour="logs-grafico-barras"]',
    content:
      'Volume de eventos por hora nas últimas 24 horas, útil para identificar picos de atividade.',
    placement: 'top',
    skipBeacon: true,
    data: { route: '/logs' },
  },
  {
    target: '[data-tour="logs-grafico-pizza"]',
    content:
      'Distribuição dos eventos por nível de severidade (Information, Warning, Error, Fatal). Uma visão rápida da saúde do sistema.',
    placement: 'top',
    skipBeacon: true,
    data: { route: '/logs' },
  },
  {
    target: '[data-tour="logs-tabela"]',
    content:
      'A tabela lista os logs em tempo real (atualiza a cada 15s), com filtro por nível. Linhas de erro são clicáveis e expandem o stacktrace completo.',
    placement: 'top',
    skipBeacon: true,
    data: { route: '/logs' },
  },
  {
    target: '[data-tour="btn-tour"]',
    content: 'Você pode reiniciar este tour a qualquer momento clicando aqui. Bom uso!',
    placement: 'bottom',
    skipBeacon: true,
    data: { route: '/logs' },
  },
];
