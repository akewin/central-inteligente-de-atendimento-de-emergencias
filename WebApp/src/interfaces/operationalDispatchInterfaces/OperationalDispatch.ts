import type { PatientTransport } from '../hospitalInterfaces/Hospital';
export type DispatchStatus = 'ATRIBUIDO' | 'ACEITO' | 'NO_LOCAL' | 'EM_ATENDIMENTO' | 'CONCLUIDO' | 'CANCELADO';

export interface OperationalDispatch {
  id: number; occurrenceId: number; status: DispatchStatus; createdAt: string; updatedAt: string;
  acceptedAt?: string; arrivedAt?: string; serviceStartedAt?: string; completedAt?: string; cancelledAt?: string;
  unitName: string; unitService: string; occurrenceStatus: string; description: string; address: string;
  type?: string; priority?: string; services: string[];
  transport?: PatientTransport | null;
}

export interface DispatchRealtimeEvent {
  dispatchId: number; occurrenceId: number; unitId: number; dispatchStatus: DispatchStatus;
  occurrenceStatus: string; updatedAt: string;
}

export interface OperationalRoute {
  dispatchId: number;
  stage: 'VITIMA' | 'HOSPITAL';
  originLabel: string;
  destinationLabel: string;
  distanceKm: number;
  estimatedMinutes: number;
  origin: { latitude: number; longitude: number; label: string };
  destination: { latitude: number; longitude: number; label: string };
  geometry: { latitude: number; longitude: number }[];
}
