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
