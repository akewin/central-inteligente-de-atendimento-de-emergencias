export type OccurrenceStatus =
  | 'ABERTA'
  | 'EM_ANALISE'
  | 'AGUARDANDO_CONFIRMACAO'
  | 'DESPACHADA'
  | 'EM_ATENDIMENTO'
  | 'FINALIZADA'
  | 'CANCELADA';

export type OccurrenceType =
  | 'ACIDENTE_TRANSITO'
  | 'INCENDIO'
  | 'AGRESSAO'
  | 'ROUBO'
  | 'FERIMENTO'
  | 'MAL_SUBITO'
  | 'PESSOA_DESAPARECIDA'
  | 'RESGATE'
  | 'OUTROS';

export type OccurrencePriority = 'BAIXA' | 'MEDIA' | 'ALTA' | 'CRITICA';

export type EmergencyServiceType = 'POLICIA' | 'SAMU' | 'BOMBEIROS';

export interface AIAnalysisRead {
  id: number;
  occurrenceId: number;
  recommendedType: OccurrenceType;
  recommendedPriority: OccurrencePriority;
  recommendedServices: EmergencyServiceType[];
  reason: string;
  provider: string;
  model: string;
  createdAt: string;
}

export interface OccurrenceServiceConfirmationRead {
  confirmedServices: EmergencyServiceType[];
  confirmedBy: OccurrenceCreator;
  confirmedAt: string;
}

export interface OccurrenceCreator {
  id: number;
  username: string;
  fullName: string;
}

export interface OccurrenceList {
  id: number;
  descriptionSummary: string;
  locationDescription: string;
  status: OccurrenceStatus;
  confirmedType: OccurrenceType | null;
  confirmedPriority: OccurrencePriority | null;
  createdBy: OccurrenceCreator;
  createdAt: string;
}

export interface OccurrenceRead {
  id: number;
  description: string;
  locationDescription: string;
  postalCode: string | null;
  street: string | null;
  number: string | null;
  complement: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  reference: string | null;
  status: OccurrenceStatus;
  confirmedType: OccurrenceType | null;
  confirmedPriority: OccurrencePriority | null;
  createdBy: OccurrenceCreator;
  createdAt: string;
  updatedAt: string;
  latestAIAnalysis: AIAnalysisRead | null;
  serviceConfirmation: OccurrenceServiceConfirmationRead | null;
}

export interface OccurrenceCreatePayload {
  description: string;
  postalCode: string;
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
  reference?: string;
}

export interface OccurrenceFilters {
  status?: OccurrenceStatus | '';
  search?: string;
}
