import api from './api';
import { User } from './interfaces/user';
import { logger } from './logger';

interface AuthResponse {
    token: string;
    refreshToken?: string;
    isSuccess: boolean;
    message: string;
}

class IdentityService {
    async register(data: { fullName: string; email: string; password: string }) {
        try {
            const response = await api.post('/api/Account/register', data);
            logger.debug('User registered', { email: data.email });
            return response;
        } catch (error) {
            logger.error('Registration failed', error);
            throw error;
        }
    }

    async login(data: { email: string; password: string }) {
        try {
            const { data: response } = await api.post<AuthResponse>('/api/Account/login', data);
            if (response.token) {
                localStorage.setItem('authToken', response.token);
                if (response.refreshToken) {
                    localStorage.setItem('refreshToken', response.refreshToken);
                }
                logger.debug('User logged in', { email: data.email });
            }
            return response;
        } catch (error) {
            logger.error('Login failed', error);
            throw error;
        }
    }

    async getCurrentUser() {
        try {
            const { data } = await api.get<User>('/api/Account/detail');
            return data;
        } catch (error) {
            await this.handleAuthError(error);
            return null;
        }
    }

    public async handleAuthError(error: any) {
        if (error?.response?.status === 401) {
            const refreshToken = localStorage.getItem('refreshToken');
            if (refreshToken) {
                try {
                    const { data } = await api.post<AuthResponse>('/api/Account/refresh-token', { refreshToken });
                    localStorage.setItem('authToken', data.token);
                    return;
                } catch {
                    this.logout();
                }
            }
            this.logout();
        }
    }

    async logout(router?: any) {
        try {
            await api.post('/api/Account/logout');
            logger.debug('User logged out');
        } catch (error) {
            logger.error('Logout failed', error);
        } finally {
            localStorage.removeItem('authToken');
            localStorage.removeItem('refreshToken');
            if (router) {
                router.refresh();
            }
        }
    }
}

export default new IdentityService(); 