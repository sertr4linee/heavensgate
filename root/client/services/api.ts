import axios from 'axios';
import identityService from './identity-service';

const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_SERVER_URL,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    }
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

api.interceptors.response.use(
    response => response,
    async error => {
        if (error.response?.status === 401) {
            await identityService.handleAuthError(error);
        }
        return Promise.reject(error);
    }
);

export default api;
