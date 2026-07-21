// src/app/core/models/auth.models.ts
import { ApiResponse } from './api-response.model';

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ResetPasswordDto {
  email: string;
  newPassword: string;
  confirmNewPassword: string;
  token?: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

export interface TokenResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  permissions: string[];
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface User extends UserDto {
  avatar?: string;
  theme?: string;
  language?: string;
  // Additional fields from your API
  username?: string;
  designation?: string;
  userType?: string;
  siteId?: number | null;
  siteName?: string;
  province?: string;
  district?: string;
  tehsil?: string;
  mobileNumber?: string;
  isAdmin?: boolean;
}

// Updated LoginResponse to match your actual API
export interface LoginResponse {
  token: string;
  username: string;
  userId: string;
  email: string;
  personName: string;
  designation: string;
  userType: string;
  siteId: number | null;
  siteName: string;
  province: string;
  district: string;
  tehsil: string;
  mobileNumber: string;
  isAdmin: boolean;
  expiresIn?: number;
}

export interface LogoutResponse {
  message: string;
}

// Helper function to convert LoginResponse to User
export function mapLoginResponseToUser(response: LoginResponse): User {
  const nameParts = response.personName?.split(' ') || ['', ''];
  
  return {
    id: response.userId,
    email: response.email,
    firstName: nameParts[0] || '',
    lastName: nameParts.slice(1).join(' ') || '',
    fullName: response.personName || '',
    roles: ['User'],
    permissions: [],
    isActive: true,
    lastLoginAt: new Date().toISOString(),
    createdAt: new Date().toISOString(),
    updatedAt: null,
    avatar: `https://ui-avatars.com/api/?name=${encodeURIComponent(response.personName || 'User')}&background=random`,
    theme: 'light',
    language: 'en',
    // Additional fields
    username: response.username,
    designation: response.designation,
    userType: response.userType,
    siteId: response.siteId,
    siteName: response.siteName,
    province: response.province,
    district: response.district,
    tehsil: response.tehsil,
    mobileNumber: response.mobileNumber,
    isAdmin: response.isAdmin
  };
}