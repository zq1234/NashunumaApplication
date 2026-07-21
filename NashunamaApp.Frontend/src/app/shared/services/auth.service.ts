// src/app/core/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { 
  LoginDto, 
  LoginResponse, 
  User, 
  RegisterDto,
  ChangePasswordDto,
  ResetPasswordDto,
  RefreshTokenDto,
  TokenResponseDto,
  LogoutResponse,
  UserDto,
  mapLoginResponseToUser
} from '@core/models/auth.model';
import { ApiResponse } from '@core/models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Storage keys
  private tokenKey = `${environment.storage.prefix}${environment.auth.tokenKey}`;
  private userKey = `${environment.storage.prefix}${environment.auth.userKey}`;
  private refreshTokenKey = `${environment.storage.prefix}${environment.auth.refreshTokenKey}`;
  private tokenExpiryKey = `${environment.storage.prefix}${environment.auth.tokenExpiryKey}`;

  // State subjects
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadStoredUser();
  }

  /**
   * Load stored user data from localStorage on initialization
   */
  private loadStoredUser(): void {
    const storedUser = localStorage.getItem(this.userKey);
    const token = localStorage.getItem(this.tokenKey);
    const tokenExpiry = localStorage.getItem(this.tokenExpiryKey);

    if (storedUser && token) {
      // Check if token is expired
      if (tokenExpiry && new Date(tokenExpiry) < new Date()) {
        this.clearLocalStorage();
        return;
      }

      try {
        const user = JSON.parse(storedUser) as User;
        if (user && user.id) {
          this.currentUserSubject.next(user);
          this.isAuthenticatedSubject.next(true);
        } else {
          this.clearLocalStorage();
        }
      } catch (e) {
        this.clearLocalStorage();
      }
    }
  }

  /**
   * Set loading state
   */
  private setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  /**
   * Handle error responses
   */
  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    let errorMessage = 'An error occurred. Please try again.';
    
    if (error.error) {
      if (typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error.message) {
        errorMessage = error.error.message;
      } else if (error.error.errors) {
        // Handle validation errors
        const validationErrors = Object.values(error.error.errors).flat();
        errorMessage = validationErrors.join(', ');
      }
    } else if (error.message) {
      errorMessage = error.message;
    }

    return throwError(() => new Error(errorMessage));
  }

  /**
   * Login user
   */
  login(loginDto: LoginDto): Observable<ApiResponse<LoginResponse>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}${environment.auth.loginUrl}`;
    
    return this.http.post<ApiResponse<LoginResponse>>(url, loginDto)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.handleLoginResponse(response.data);
          }
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Register new user
   */
  register(registerDto: RegisterDto): Observable<ApiResponse<string>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}${environment.auth.registerUrl}`;
    
    return this.http.post<ApiResponse<string>>(url, registerDto)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Change password for authenticated user
   */
  changePassword(changePasswordDto: ChangePasswordDto): Observable<ApiResponse<string>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}${environment.auth.changePasswordUrl}`;
    
    return this.http.post<ApiResponse<string>>(url, changePasswordDto)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Reset password (forgot password flow)
   */
  resetPassword(resetPasswordDto: ResetPasswordDto): Observable<ApiResponse<string>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}${environment.auth.resetPasswordUrl}`;
    
    return this.http.post<ApiResponse<string>>(url, resetPasswordDto)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Refresh authentication token
   */
  refreshToken(refreshTokenDto: RefreshTokenDto): Observable<ApiResponse<TokenResponseDto>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}${environment.auth.refreshTokenUrl}`;
    
    return this.http.post<ApiResponse<TokenResponseDto>>(url, refreshTokenDto)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.updateTokens(response.data);
          }
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Logout user
   */
  logout(): Observable<ApiResponse<LogoutResponse>> {
    this.setLoading(true);
    const userId = this.currentUserSubject.value?.id;
    
    // If no user ID, just clear local storage and redirect
    if (!userId) {
      this.clearLocalStorage();
      this.router.navigate(['/signin']);
      return of({
        isSuccess: true,
        message: 'Logged out successfully',
        data: { message: 'Logged out successfully' },
        errors: null,
        statusCode: 200
      });
    }

    const url = `${environment.apiUrl}${environment.auth.logoutUrl}`;
    return this.http.post<ApiResponse<LogoutResponse>>(url, { userId })
      .pipe(
        tap(() => {
          this.clearLocalStorage();
          this.router.navigate(['/signin']);
        }),
        catchError((error) => {
          // Even if API call fails, clear local storage
          this.clearLocalStorage();
          this.router.navigate(['/signin']);
          return this.handleError(error);
        }),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get current user profile
   */
  getProfile(): Observable<ApiResponse<UserDto>> {
    this.setLoading(true);
    const url = `${environment.apiUrl}/api/Auth/profile`;
    
    return this.http.get<ApiResponse<UserDto>>(url)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.updateUserProfile(response.data);
          }
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Validate token
   */
  validateToken(token: string): Observable<ApiResponse<boolean>> {
    const url = `${environment.apiUrl}${environment.auth.validateTokenUrl}`;
    return this.http.post<ApiResponse<boolean>>(url, { token })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Handle successful login response
   */
  private handleLoginResponse(response: LoginResponse): void {
    if (!response || !response.token) {
      console.error('Invalid login response: token is missing', response);
      return;
    }

    // Set expiry (default 24 hours if not provided)
    const expiresIn = response.expiresIn || 86400;
    const expiryDate = new Date();
    expiryDate.setSeconds(expiryDate.getSeconds() + expiresIn);

    // Store token
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.tokenExpiryKey, expiryDate.toISOString());
    
    // Map the response to User model using the helper function
    const user = mapLoginResponseToUser(response);
    
    // Store user
    localStorage.setItem(this.userKey, JSON.stringify(user));
    
    // Update subjects
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  /**
   * Update tokens during refresh
   */
  private updateTokens(tokenResponse: TokenResponseDto): void {
    const expiryDate = new Date(tokenResponse.expiresAt);

    localStorage.setItem(this.tokenKey, tokenResponse.accessToken);
    if (tokenResponse.refreshToken) {
      localStorage.setItem(this.refreshTokenKey, tokenResponse.refreshToken);
    }
    localStorage.setItem(this.tokenExpiryKey, expiryDate.toISOString());
  }

  /**
   * Update user profile information
   */
  private updateUserProfile(userData: UserDto): void {
    const currentUser = this.currentUserSubject.value;
    if (currentUser && userData) {
      const updatedUser: User = {
        ...currentUser,
        id: userData.id || currentUser.id,
        email: userData.email || currentUser.email,
        firstName: userData.firstName || currentUser.firstName,
        lastName: userData.lastName || currentUser.lastName,
        fullName: userData.fullName || currentUser.fullName,
        roles: userData.roles || currentUser.roles,
        permissions: userData.permissions || currentUser.permissions,
        isActive: userData.isActive !== undefined ? userData.isActive : currentUser.isActive,
        lastLoginAt: userData.lastLoginAt || currentUser.lastLoginAt,
        createdAt: userData.createdAt || currentUser.createdAt,
        updatedAt: userData.updatedAt || currentUser.updatedAt
      };
      
      localStorage.setItem(this.userKey, JSON.stringify(updatedUser));
      this.currentUserSubject.next(updatedUser);
    }
  }

  /**
   * Clear all stored authentication data
   */
  private clearLocalStorage(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.tokenExpiryKey);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  /**
   * Get current authentication token
   */
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  /**
   * Get current user
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    const tokenExpiry = localStorage.getItem(this.tokenExpiryKey);
    
    if (!token || !tokenExpiry) {
      return false;
    }

    // Check if token is expired
    if (new Date(tokenExpiry) < new Date()) {
      this.clearLocalStorage();
      return false;
    }

    return this.isAuthenticatedSubject.value;
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.roles) return false;
    return roles.some(role => user.roles!.includes(role));
  }

  /**
   * Check if user has all specified roles
   */
  hasAllRoles(roles: string[]): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.roles) return false;
    return roles.every(role => user.roles!.includes(role));
  }

  /**
   * Check if user has specific permission
   */
  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    if (!user || !user.permissions) return false;
    return user.permissions.includes(permission);
  }
}