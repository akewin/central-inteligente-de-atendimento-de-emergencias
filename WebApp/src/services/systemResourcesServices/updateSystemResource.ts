import api from '../../api';
import type { SystemResource } from '../../interfaces';

export async function updateSystemResource(resource: SystemResource) {
  await api.put(`/resources/${resource.id}`, resource);
}
