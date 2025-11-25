import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';

// --- TYPES ---
export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
}

export interface RegisterResponse {
    accessToken: string;
    refreshToken: string;
}

export interface LoggedUserResponse {
    userName: string;
    loginDate: string;
    sessionId: string;
    hostName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// --- API CONFIG ---
// If running locally, point to your backend URL. 
// In production/RR DNS, this should be empty string '' to use relative path
// VITE_API_URL will be injected at build time.
// If empty, it defaults to current origin (relative paths).
const BASE_URL = import.meta.env.VITE_API_URL || '';

const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Add Token
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response Interceptor: Refresh Token Logic
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // If 401 Unauthorized and we haven't retried yet
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) throw new Error('No refresh token available');

        // Use a fresh axios instance to avoid interceptor loops
        const { data } = await axios.post<LoginResponse>(`${BASE_URL}/auth/refresh`, {
          refreshToken,
        } as RefreshTokenRequest);

        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);

        // Update the header for the retry
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - logout user
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.reload();
        return Promise.reject(refreshError);
      }
    }
    return Promise.reject(error);
  }
);

export default api;