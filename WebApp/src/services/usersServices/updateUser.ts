import api from '../../api';
import type { UserFormValues } from '../../interfaces';

export async function updateUser(user: UserFormValues) {
  await api.put(`/users/${user.id}`, user);
}
