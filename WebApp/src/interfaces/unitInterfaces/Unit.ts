import type { PagedResponse } from '../PagedResponse';

export type EmergencyServiceType = 'POLICIA' | 'SAMU' | 'BOMBEIROS';
export type UnitStatus = 'DISPONIVEL' | 'RESERVADA' | 'DESLOCAMENTO' | 'EM_ATENDIMENTO' | 'INDISPONIVEL';

export interface EmergencyService {
  id: number;
  type: EmergencyServiceType;
  emergencyNumber: string;
  displayName: string;
}

export interface Unit {
  id: number;
  name: string;
  service: EmergencyService;
  postalCode: string | null;
  street: string | null;
  number: string | null;
  complement: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  reference: string | null;
  address: string;
  status: UnitStatus;
  createdAt: string;
  updatedAt: string;
}

export interface UnitPayload {
  name: string;
  service: EmergencyServiceType;
  postalCode: string;
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
  reference?: string;
  status: UnitStatus;
  expectedUpdatedAt?: string;
}

export interface UnitFilters {
  search: string;
  emergencyService: '' | EmergencyServiceType;
  status: '' | UnitStatus;
}

export type UnitsPagination = PagedResponse<Unit>;
