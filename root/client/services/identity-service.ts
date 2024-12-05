import api from './api';
import { User } from './interfaces/user';

interface AuthResponse {
    token: string;
    refreshToken?: string;
    isSuccess: boolean;
    message: string;
}

class IdentityService {
    async register(data: { fullName: string; email: string; password: string }) {
        return api.post('/api/Account/register', data);
    }

    async login(data: { email: string; password: string }) {
        const { data: response } = await api.post<AuthResponse>('/api/Account/login', data);
        if (response.token) {
            localStorage.setItem('authToken', response.token);
            if (response.refreshToken) {
                localStorage.setItem('refreshToken', response.refreshToken);
            }
        }
        return response;
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

    async logout() {
        await api.post('/api/Account/logout');
        localStorage.removeItem('authToken');
    }
}

export default new IdentityService(); 