import type { OccurrenceCreator } from './Occurrence';

export interface DispatchRead {
  id: number;
  unitId: number;
  unitName: string;
  service: string;
  unitStatus: 'RESERVADA' | 'DESLOCAMENTO' | 'EM_ATENDIMENTO' | 'DISPONIVEL';
  status: 'ATRIBUIDO' | 'ACEITO' | 'NO_LOCAL' | 'EM_ATENDIMENTO' | 'CONCLUIDO' | 'CANCELADO';
  createdAt: string;
  updatedAt: string;
  acceptedAt?: string;
  arrivedAt?: string;
  serviceStartedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
}

export interface DispatchConfirmationRead {
  occurrenceId: number;
  occurrenceStatus: 'DESPACHADA';
  confirmedAt: string;
  confirmedBy: OccurrenceCreator;
  dispatches: DispatchRead[];
}
