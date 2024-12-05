export interface User {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  twoFactorEnabled: boolean;
  phoneNumberConfirmed: boolean;
  accessFailedCount: number;
  roles: string[];
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterCredentials {
  email: string;
  password: string;
  fullName: string;
} 