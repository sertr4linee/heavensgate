import axios, { InternalAxiosRequestConfig } from "axios";
import { IdentityService } from "./identity-service";

const serverUrl = process.env.NEXT_PUBLIC_SERVER_URL;

if (!serverUrl || !serverUrl.startsWith('http')) {
  console.warn('Warning: Server URL might be misconfigured:', serverUrl);
}

const api = axios.create({
  baseURL: serverUrl,
  withCredentials: true,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-Requested-With': 'XMLHttpRequest'
  },
  xsrfCookieName: 'XSRF-TOKEN',
  xsrfHeaderName: 'X-XSRF-TOKEN',
});

const isValidJWT = (token: string) => {
  return /^[A-Za-z0-9-_=]+\.[A-Za-z0-9-_=]+\.?[A-Za-z0-9-_.+/=]*$/.test(token);
};

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('authToken');
  if (token && isValidJWT(token)) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken');
      window.location.href = '/auth/login';
    }
    const errorMessage = process.env.NODE_ENV === 'production' 
      ? 'An error occurred' 
      : error.message;
    console.error(`❌ API Error: ${errorMessage}`);
    return Promise.reject(error);
  }
);

const identityService = new IdentityService(api);

export { api, identityService };
