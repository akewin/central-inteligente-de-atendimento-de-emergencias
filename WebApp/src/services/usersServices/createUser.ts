import api from '../../api';
import type { UserFormValues } from '../../interfaces';

export async function createUser(user: UserFormValues) {
  await api.post('/users', user);
}
