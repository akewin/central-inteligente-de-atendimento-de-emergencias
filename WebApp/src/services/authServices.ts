import api from '../api';
import type {
  LoginPayload,
  LoginResponse,
  ExternalLoginPayload,
  RefreshRequest,
  RefreshResponse,
} from '../interfaces';

export async function login(payload: LoginPayload): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>('/auth/login', payload);
  return data;
}

export async function externalLogin(
  payload: ExternalLoginPayload
): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>('/auth/external', payload);
  return data;
}

export async function refreshToken(
  payload: RefreshRequest
): Promise<RefreshResponse> {
  const { data } = await api.post<RefreshResponse>('/auth/refresh', payload);
  return data;
}

export async function logout(): Promise<void> {
  await api.post('/auth/logout');
}
