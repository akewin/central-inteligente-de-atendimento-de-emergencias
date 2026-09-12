import api from '../../api';
import type { UserRead, UsersPagination } from '../../interfaces';

export async function listUsers(
  pageNumber = 1,
  pageSize = 10,
  searchKey: string = ''
) {
  const defaultParams = `page=${pageNumber}&pageSize=${pageSize}`;

  const endpoint = searchKey
    ? `/users/search?key=${encodeURIComponent(searchKey)}&${defaultParams}`
    : `/users?${defaultParams}`;

  const { data } = await api.get<UsersPagination>(endpoint);
  return data;
}

export async function listUsersForSelect() {
  const { data } = await api.get<UserRead[]>('/users/options');
  return data;
}

export async function listOperationalUnitOptions(userId?: number) {
  const response = await api.get<Array<{ id: number; name: string; service: string }>>('/users/operational-unit-options', { params: { userId } });
  return response.data;
}

export async function listUserById(id: number) {
  const { data } = await api.get<UserRead>(`/users/${id}`);
  return data;
}
