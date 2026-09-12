import api from '../../api';
import type { SystemResource } from '../../interfaces';

export async function createSystemResource(resource: SystemResource) {
  const { data } = await api.post('/resources', resource);
  return data;
}
