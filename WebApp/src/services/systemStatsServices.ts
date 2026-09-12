import api from '../api';
import type { SystemStats } from '../interfaces';

export async function getSystemStats() {
  const { data } = await api.get<SystemStats>('/stats');
  return data;
}
