import api from '../../api';

export async function deleteSystemResource(id: string) {
  await api.delete(`/resources/${id}`);
}
