import { AxiosInstance } from 'axios';
import { User, LoginCredentials, RegisterCredentials } from './interfaces/user';
import { AuthResponse } from './interfaces/auth';

export class IdentityService {
  private api: AxiosInstance;

  constructor(api: AxiosInstance) {
    this.api = api;
  }

  /**
   * log an user
   */
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const { data } = await this.api.post<AuthResponse>('/api/Account/login', credentials);
    return data;
  }

  /**
   * register an user
   */
  async register(credentials: RegisterCredentials) {
    const { data } = await this.api.post<User>('/api/Account/register', credentials);
    return data;
  }

  /**
   * disconnect an user
   */
  async logout() {
    await this.api.post('/api/Account/logout');
  }

  /**
   * get the current user
   */
  async getCurrentUser() {
    try {
      const { data } = await this.api.get<User>('/api/Account/detail');
      return data;
    } catch (error) {
      return null;
    }
  }

  /**
   * check if the user is authenticated
   */
  async isAuthenticated(): Promise<boolean> {
    const user = await this.getCurrentUser();
    return user !== null;
  }
} 