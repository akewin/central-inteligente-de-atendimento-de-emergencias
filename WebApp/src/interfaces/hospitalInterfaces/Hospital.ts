export interface HospitalWard {
  id: number; hospitalId: number; name: string; totalBeds: number; occupiedBeds: number;
  reservedBeds: number; availableBeds: number; active: boolean; updatedAt: string;
}
export interface Hospital {
  id: number; name: string; postalCode: string; street: string; number: string; complement?: string;
  neighborhood: string; city: string; state: string; latitude: number; longitude: number;
  hasEmergencyDepartment: boolean; active: boolean; wards: HospitalWard[]; updatedAt: string;
}
export interface HospitalPayload extends Omit<Hospital, 'id' | 'wards' | 'updatedAt'> { expectedUpdatedAt?: string }
export interface PatientTransport {
  id: number; occurrenceId: number; requestedByDispatchId: number; samuDispatchId?: number; samuUnitId?: number;
  hospitalId?: number; hospitalWardId?: number; hospitalName?: string; wardName?: string;
  status: 'AGUARDANDO_CENTRAL' | 'AGUARDANDO_DESTINO' | 'HOSPITAL_AVISADO' | 'EM_TRANSPORTE' | 'RECEBIDO' | 'CANCELADO';
  operationalNotes?: string; priority?: string; type?: string; description: string; origin: string;
  estimatedMinutes?: number; acknowledgedAt?: string; createdAt: string; updatedAt: string;
}
