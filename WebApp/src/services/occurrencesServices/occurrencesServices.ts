import api from '../../api';
import type {
  OccurrenceCreatePayload,
  OccurrenceFilters,
  OccurrenceRead,
  OccurrencesPagination,
  AIAnalysisRead,
  EmergencyServiceType,
  OccurrenceUnitRecommendations,
  DispatchConfirmationRead,
  OccurrenceStatus,
  OccurrenceStatusTransitionRead,
  OccurrenceTimelineItem,
  OccurrenceMapData,
  PatientTransport,
} from '../../interfaces';

export async function createOccurrence(payload: OccurrenceCreatePayload) {
  const { data } = await api.post<OccurrenceRead>('/occurrences', payload);
  return data;
}

export async function requestOccurrenceAnalysis(id: number) {
  const { data } = await api.post<AIAnalysisRead>(`/occurrences/${id}/analysis`);
  return data;
}

export async function confirmOccurrenceServices(
  id: number,
  confirmedServices: EmergencyServiceType[],
) {
  const { data } = await api.put(`/occurrences/${id}/service-confirmation`, {
    confirmedServices,
  });
  return data;
}

export async function listOccurrences(
  page = 1,
  pageSize = 10,
  filters: OccurrenceFilters = {},
) {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });

  if (filters.status) params.set('status', filters.status);
  if (filters.search?.trim()) params.set('search', filters.search.trim());

  const { data } = await api.get<OccurrencesPagination>(
    `/occurrences?${params.toString()}`,
  );
  return data;
}

export async function getOccurrenceById(id: number) {
  const { data } = await api.get<OccurrenceRead>(`/occurrences/${id}`);
  return data;
}

export async function getOccurrenceUnitRecommendations(id: number, limitPerService = 3) {
  const { data } = await api.get<OccurrenceUnitRecommendations>(
    `/occurrences/${id}/unit-recommendations`,
    { params: { limitPerService } },
  );
  return data;
}

export async function confirmDispatch(id: number, unitIds: number[]) {
  const { data } = await api.post<DispatchConfirmationRead>(
    `/occurrences/${id}/dispatches`,
    { unitIds },
  );
  return data;
}

export async function transitionOccurrenceStatus(
  id: number,
  targetStatus: Extract<OccurrenceStatus, 'EM_ATENDIMENTO' | 'FINALIZADA' | 'CANCELADA'>,
  expectedUpdatedAt: string,
  reason?: string,
) {
  const { data } = await api.put<OccurrenceStatusTransitionRead>(`/occurrences/${id}/status`, {
    targetStatus, expectedUpdatedAt, reason,
  });
  return data;
}

export async function getOccurrenceTimeline(id: number) {
  const { data } = await api.get<OccurrenceTimelineItem[]>(`/occurrences/${id}/timeline`);
  return data;
}

export async function getOccurrenceMap(id: number, hospitalId?: number) {
  const { data } = await api.get<OccurrenceMapData>(`/occurrences/${id}/map`, {
    params: {
      radiusKm: 15,
      hospitalLimit: 8,
      hospitalId: hospitalId || undefined,
    },
  });
  return data;
}

export async function getOccurrenceTransport(id: number) {
  return (await api.get<PatientTransport | null>(`/occurrences/${id}/transport`)).data;
}
export async function assignSamuToTransport(id: number, unitId: number) {
  return (await api.post<PatientTransport>(`/occurrences/${id}/transport/samu`, { unitId })).data;
}

export async function confirmOccurrenceHospital(id: number, hospitalId: number) {
  const { data } = await api.put(`/occurrences/${id}/hospital`, { hospitalId });
  return data;
}
