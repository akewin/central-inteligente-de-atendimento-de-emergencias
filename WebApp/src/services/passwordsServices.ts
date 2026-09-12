import api from "../api";
import type { PasswordResetPayload } from "../interfaces";

export async function requestPasswordReset(email: string): Promise<string> {
  const { data } = await api.post('/password/request-new', { email });
  return data.message;
}

export async function resetPassword(
  payload: PasswordResetPayload
): Promise<string> {
  const { data } = await api.post('/password/reset', payload);
  return data.message;
}