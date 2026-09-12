import api from '../../api';
import type {
  SystemResourcesPagination,
  SystemResource,
  SystemResourceOption,
} from '../../interfaces';

export async function listSystemResources(
  pageNumber = 1,
  pageSize = 10,
  searchKey: string = ''
) {
  const defaultParams = `page=${pageNumber}&pageSize=${pageSize}`;

  const endpoint = searchKey
    ? `/resources/search?key=${encodeURIComponent(searchKey)}&${defaultParams}`
    : `/resources?${defaultParams}`;

  const { data } = await api.get<SystemResourcesPagination>(endpoint);
  return data;
}

export async function listSystemResourcesForSelect() {
  const { data } = await api.get<SystemResourceOption[]>('/resources/options');
  return data;
}

export async function listSystemResourceById(id: number) {
  const { data } = await api.get<SystemResource>(`/resources/${id}`);
  return data;
}
