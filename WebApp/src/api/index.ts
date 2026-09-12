import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const baseURL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:5209/api';

const api = axios.create({ baseURL });

const refreshClient = axios.create({ baseURL });

let isRefreshing = false;
let refreshSubscribers: ((token: string | null) => void)[] = [];

function subscribeTokenRefresh(callback: (token: string | null) => void) {
  refreshSubscribers.push(callback);
}

function notifySubscribers(token: string | null) {
  refreshSubscribers.forEach((callback) => callback(token));
  refreshSubscribers = [];
}

api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('token');

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,

  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined;

    if (!originalRequest) return Promise.reject(error);

    if (error.response?.status !== 401)
      return Promise.reject(error);

    if (originalRequest._retry)
      return Promise.reject(error);

    const storedRefreshToken = localStorage.getItem('refreshToken');

    if (!storedRefreshToken) {
      performLogout();
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        subscribeTokenRefresh((newToken) => {
          if (newToken) {
            originalRequest._retry = true;
            resolve(api(originalRequest));
          } else {
            reject(error);
          }
        });
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const refreshResponse = await refreshClient.post('/auth/refresh', {
        refreshToken: storedRefreshToken,
      });

      const { token, refreshToken } = refreshResponse.data;

      localStorage.setItem('token', token);
      localStorage.setItem('refreshToken', refreshToken);

      notifySubscribers(token);

      window.dispatchEvent(new Event('tokenRefreshed'));

      return api(originalRequest);
    } catch (refreshError) {
      notifySubscribers(null); // 👈 ESSENCIAL
      performLogout();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

function performLogout() {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('authUser');

  window.dispatchEvent(new Event('logout'));
}

export default api;
