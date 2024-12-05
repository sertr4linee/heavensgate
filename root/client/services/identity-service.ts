import { AxiosInstance } from 'axios';
import { User, LoginCredentials, RegisterCredentials } from './interfaces/user';

export class IdentityService {
  private api: AxiosInstance;

  constructor(api: AxiosInstance) {
    this.api = api;
  }

  /**
   * Connecte un utilisateur
   */
  async login(credentials: LoginCredentials) {
    const { data } = await this.api.post<User>('/api/Account/login', credentials);
    return data;
  }

  /**
   * Inscrit un nouvel utilisateur
   */
  async register(credentials: RegisterCredentials) {
    const { data } = await this.api.post<User>('/api/Account/register', credentials);
    return data;
  }

  /**
   * Déconnecte l'utilisateur courant
   */
  async logout() {
    await this.api.post('/api/Account/logout');
  }

  /**
   * Récupère les informations de l'utilisateur connecté
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
   * Vérifie si l'utilisateur est authentifié
   */
  async isAuthenticated(): Promise<boolean> {
    const user = await this.getCurrentUser();
    return user !== null;
  }
} 