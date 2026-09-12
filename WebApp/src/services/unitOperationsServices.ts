import api from '../api';
import type { DispatchStatus, Hospital, OperationalDispatch, OperationalRoute, PagedResponse, PatientTransport } from '../interfaces';

export async function listMyDispatches(scope: 'active' | 'history', page = 1, pageSize = 10) {
  return (await api.get<PagedResponse<OperationalDispatch>>('/unit-operations/dispatches', { params: { scope, page, pageSize } })).data;
}

export async function transitionMyDispatch(id: number, targetStatus: DispatchStatus, expectedUpdatedAt: string) {
  return (await api.post<OperationalDispatch>(`/unit-operations/dispatches/${id}/transitions`, { targetStatus, expectedUpdatedAt })).data;
}

export async function getOccurrenceDispatches(occurrenceId: number) {
  return (await api.get<OperationalDispatch[]>(`/occurrences/${occurrenceId}/dispatches`)).data;
}

export async function requestPatientTransport(id: number, operationalNotes?: string) {
  return (await api.post<PatientTransport>(`/unit-operations/dispatches/${id}/transport-request`, { operationalNotes })).data;
}
export async function assignTransportDestination(id: number, hospital: Hospital, wardId: number, operationalNotes?: string) {
  return (await api.put<PatientTransport>(`/unit-operations/dispatches/${id}/transport-destination`, { hospitalId: hospital.id, hospitalWardId: wardId, operationalNotes })).data;
}
export async function startPatientTransport(id: number) {
  return (await api.post<PatientTransport>(`/unit-operations/dispatches/${id}/transport-start`)).data;
}
export async function completeDispatchOnSite(id: number) {
  return (await api.post<PatientTransport>(`/unit-operations/dispatches/${id}/complete-on-site`)).data;
}

export async function getMyDispatchRoute(id: number) {
  return (await api.get<OperationalRoute>(`/unit-operations/dispatches/${id}/route`)).data;
}
