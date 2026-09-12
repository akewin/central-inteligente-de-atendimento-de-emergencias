import api from '../../api';
import type {
  EmergencyService,
  Unit,
  UnitFilters,
  UnitPayload,
  UnitsPagination,
} from '../../interfaces';

export async function listEmergencyServices() {
  const { data } = await api.get<EmergencyService[]>('/emergency-services');
  return data;
}

export async function listUnits(page: number, pageSize: number, filters: UnitFilters) {
  const { data } = await api.get<UnitsPagination>('/units', {
    params: {
      page,
      pageSize,
      search: filters.search || undefined,
      emergencyService: filters.emergencyService || undefined,
      status: filters.status || undefined,
    },
  });
  return data;
}

export async function createUnit(payload: UnitPayload) {
  const { data } = await api.post<Unit>('/units', payload);
  return data;
}

export async function updateUnit(id: number, payload: UnitPayload) {
  const { data } = await api.put<Unit>(`/units/${id}`, payload);
  return data;
}
