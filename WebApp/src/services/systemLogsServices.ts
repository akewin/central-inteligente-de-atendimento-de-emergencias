import api from '../api';
import type {
  SystemLogFiltersPayload,
  SystemLogsPagination,
} from '../interfaces';

export async function getLogReports(params: SystemLogFiltersPayload) {
  const queryParams = new URLSearchParams();

  if (params.startDate) queryParams.append('startDate', params.startDate);
  if (params.endDate) queryParams.append('endDate', params.endDate);
  if (params.userId) queryParams.append('userId', String(params.userId));
  if (params.action) queryParams.append('action', params.action);
  if (params.page) queryParams.append('page', String(params.page));
  if (params.pageSize) queryParams.append('pageSize', String(params.pageSize));

  const queryString = queryParams.toString();
  const url = `/reports${queryString ? `?${queryString}` : ''}`;

  const { data } = await api.get<SystemLogsPagination>(url);
  return data;
}

export async function getLogDetails(logId: number) {
  const { data } = await api.get(`/reports/${logId}`);
  return data;
}