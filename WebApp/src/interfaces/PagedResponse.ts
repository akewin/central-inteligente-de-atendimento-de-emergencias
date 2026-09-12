export interface PagedResponse<T> {
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
  data: T[];
}
