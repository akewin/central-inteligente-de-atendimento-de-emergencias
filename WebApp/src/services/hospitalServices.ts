import api from '../api';
import type { Hospital, HospitalPayload, HospitalWard, PatientTransport } from '../interfaces';

export const listHospitals = async (includeInactive = false) =>
  (await api.get<Hospital[]>('/hospitals', { params: { includeInactive } })).data;
export const createHospital = async (payload: HospitalPayload) => (await api.post<Hospital>('/hospitals', payload)).data;
export const updateHospital = async (id: number, payload: HospitalPayload) => (await api.put<Hospital>(`/hospitals/${id}`, payload)).data;
export const createHospitalWard = async (hospitalId: number, payload: { name: string; totalBeds: number; occupiedBeds: number; active: boolean }) =>
  (await api.post<HospitalWard>(`/hospitals/${hospitalId}/wards`, payload)).data;
export const updateHospitalWard = async (hospitalId: number, ward: HospitalWard) =>
  (await api.put<HospitalWard>(`/hospitals/${hospitalId}/wards/${ward.id}`, { ...ward, expectedUpdatedAt: ward.updatedAt })).data;
export const listHospitalReceptions = async (scope: 'active' | 'history') =>
  (await api.get<PatientTransport[]>('/hospital-receptions', { params: { scope } })).data;
export const actOnReception = async (transport: PatientTransport, action: 'CIENCIA' | 'RECEBIDO' | 'CANCELADO') =>
  (await api.post<PatientTransport>(`/hospital-receptions/${transport.id}/actions`, { action, expectedUpdatedAt: transport.updatedAt })).data;
export const getMyHospital = async () => (await api.get<Hospital>('/hospital-receptions/hospital')).data;
export const updateMyHospitalWard = async (ward: HospitalWard) => (await api.put<HospitalWard>(`/hospital-receptions/wards/${ward.id}`, { ...ward, expectedUpdatedAt: ward.updatedAt })).data;
