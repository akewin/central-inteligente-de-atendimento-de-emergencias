import axios from 'axios';

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (typeof error.response?.data === 'string' && error.response.data) {
      return error.response.data;
    }
    return (
      error.response?.data?.error ||
      error.response?.data?.message ||
      error.message ||
      'Erro desconhecido no servidor.'
    );
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Erro inesperado.';
}
