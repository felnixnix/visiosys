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
