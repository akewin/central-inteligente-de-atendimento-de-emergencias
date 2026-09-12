export interface SystemLogFiltersPayload {
  startDate?: string;
  endDate?: string;
  userId?: number;
  action?: string;
  page?: number;
  pageSize?: number;
}
