export type EsferaPrecatorio = 'Municipal' | 'Estadual' | 'Federal';
export type NaturezaPrecatorio = 'Comum' | 'Alimentar';
export type StatusPrecatorio =
  | 'EmAnalise'
  | 'AguardandoDocumentacao'
  | 'EmNegociacao'
  | 'AguardandoPagamento'
  | 'Liquidado'
  | 'Cancelado';

export interface PrecatorioDto {
  id: string;
  numero: string;
  tribunalOrigem: string;
  valorFace: number;
  valorAtualizado: number | null;
  esfera: EsferaPrecatorio;
  natureza: NaturezaPrecatorio;
  status: StatusPrecatorio;
  clienteId: string | null;
  criadoEm: string;
}

export interface PaginaDto<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanho: number;
}

export interface TokenDto {
  token: string;
  expiraEm: string;
}

export interface ApiErro {
  erro: string;
}

export type TipoConta = 'Corrente' | 'Poupanca';

export interface ClienteDto {
  id: string;
  nome: string;
  documento: string;
  email: string;
  telefone: string | null;
  bancoCodigo: string | null;
  bancoAgencia: string | null;
  bancoNumeroConta: string | null;
  bancoTipoConta: TipoConta | null;
  criadoEm: string;
}

export type TipoAndamento =
  | 'AtualizacaoStatus'
  | 'DocumentoRecebido'
  | 'ContatoRealizado'
  | 'ObservacaoInterna'
  | 'PropostaEnviada'
  | 'PropostaRecebida'
  | 'PagamentoRegistrado';

export interface AndamentoDto {
  id: string;
  precatorioId: string;
  descricao: string;
  tipo: TipoAndamento;
  registradoPorLogin: string;
  ocorridoEm: string;
}

export interface PagamentoDto {
  id: string;
  precatorioId: string;
  valorPago: number;
  valorBase: number;
  percDesagio: number;
  registradoPorLogin: string;
  pagoEm: string;
  criadoEm: string;
}

export type TipoDocumento =
  | 'Procuracao'
  | 'Certidao'
  | 'Contrato'
  | 'Peticao'
  | 'Outro';

export interface DocumentoDto {
  id: string;
  nomeOriginal: string;
  tipo: TipoDocumento;
  urlDownload: string;
  tamanhoBytes: number;
  contentType: string;
  enviadoPorLogin: string;
  precatorioId: string | null;
  clienteId: string | null;
  criadoEm: string;
}

export type NivelLog = 'Verbose' | 'Debug' | 'Information' | 'Warning' | 'Error' | 'Fatal';

export interface LogEntryDto {
  timestamp: string;
  level: NivelLog;
  mensagem: string;
  metodo: string | null;
  caminho: string | null;
  statusCode: number | null;
  elapsedMs: number | null;
  excecao: string | null;
}

export interface ContaPorHoraDto { hora: number; total: number; }
export interface ContaPorNivelDto { nivel: string; total: number; }

export interface LogStatsDto {
  totalRequests24h: number;
  erros24h: number;
  avisos24h: number;
  mediaElapsedMs: number;
  porHora: ContaPorHoraDto[];
  porNivel: ContaPorNivelDto[];
}
