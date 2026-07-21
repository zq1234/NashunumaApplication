// src/app/shared/services/user-management.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '@core/models/api-response.model';
import { 
  UserDto, 
  UserFilterParams,
  UserSearchParams,
  UserHelper} from '@core/models/user.model';
import { 
  UpdateUserLocationDto, 
  LocationStatsDto, 
  UserSummaryStats,
  LocationHelper
} from '@core/models/user-location.model';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  
  private baseUrl = environment.apiUrl;
  private api = environment.api;

  // Loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) {}

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
        const validationErrors = Object.values(error.error.errors).flat();
        errorMessage = validationErrors.join(', ');
      }
    } else if (error.message) {
      errorMessage = error.message;
    }

    return throwError(() => new Error(errorMessage));
  }

  /**
   * Helper method to build URL with path parameters
   */
  private buildUrl(endpointKey: string, pathParams?: Record<string, string | number>): string {
    let url = this.api[endpointKey];
    if (pathParams) {
      Object.keys(pathParams).forEach(key => {
        url = url.replace(`{${key}}`, encodeURIComponent(pathParams[key]));
      });
    }
    return `${this.baseUrl}${url}`;
  }

  /**
   * Get paged users with filters
   * GET /api/UserManagement/users
   */
  getPagedUsers(params: UserFilterParams): Observable<ApiResponse<PaginatedResponse<UserDto>>> {
    this.setLoading(true);
    
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    // Add optional parameters if they exist
    if (params.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params.province) {
      httpParams = httpParams.set('province', params.province);
    }
    if (params.district) {
      httpParams = httpParams.set('district', params.district);
    }
    if (params.tehsil) {
      httpParams = httpParams.set('tehsil', params.tehsil);
    }
    if (params.userType) {
      httpParams = httpParams.set('userType', params.userType);
    }
    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive.toString());
    }

    const url = this.buildUrl('user.getUsers');
    
    return this.http.get<ApiResponse<PaginatedResponse<UserDto>>>(url, { params: httpParams })
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get user by username
   * GET /api/UserManagement/users/{username}
   */
  getUserByUsername(username: string): Observable<ApiResponse<UserDto>> {
    this.setLoading(true);
    const url = this.buildUrl('user.getByUsername', { username });
    
    return this.http.get<ApiResponse<UserDto>>(url)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Transfer user location
   * PUT /api/UserManagement/users/{username}/location
   */
  transferUserLocation(username: string, locationDto: UpdateUserLocationDto): Observable<ApiResponse<UserDto>> {
    this.setLoading(true);
    const url = this.buildUrl('user.transferLocation', { username });
    
    return this.http.put<ApiResponse<UserDto>>(url, locationDto)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Toggle user status (Activate/Deactivate)
   * PATCH /api/UserManagement/users/{username}/status
   */
  toggleUserStatus(username: string, isActive: boolean): Observable<ApiResponse<boolean>> {
    this.setLoading(true);
    const url = this.buildUrl('user.toggleStatus', { username });
    
    return this.http.patch<ApiResponse<boolean>>(url, isActive)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Block user
   * POST /api/UserManagement/users/{username}/block
   */
  blockUser(username: string): Observable<ApiResponse<boolean>> {
    this.setLoading(true);
    const url = this.buildUrl('user.blockUser', { username });
    
    return this.http.post<ApiResponse<boolean>>(url, {})
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get users by location
   * GET /api/UserManagement/users/by-location
   */
  getUsersByLocation(
    province?: string,
    district?: string,
    tehsil?: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<ApiResponse<PaginatedResponse<UserDto>>> {
    this.setLoading(true);
    
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (province) params = params.set('province', province);
    if (district) params = params.set('district', district);
    if (tehsil) params = params.set('tehsil', tehsil);

    const url = this.buildUrl('user.getByLocation');
    
    return this.http.get<ApiResponse<PaginatedResponse<UserDto>>>(url, { params })
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get location statistics
   * GET /api/UserManagement/statistics/location
   */
  getLocationStatistics(): Observable<ApiResponse<LocationStatsDto>> {
    this.setLoading(true);
    const url = this.buildUrl('user.getStatistics');
    
    return this.http.get<ApiResponse<LocationStatsDto>>(url)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get user summary statistics
   * GET /api/UserManagement/statistics/summary
   */
  getUserSummaryStats(): Observable<ApiResponse<UserSummaryStats>> {
    this.setLoading(true);
    const url = `${this.baseUrl}/api/UserManagement/statistics/summary`;
    
    return this.http.get<ApiResponse<UserSummaryStats>>(url)
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Get users by role
   * GET /api/UserManagement/users/by-role/{role}
   */
  getUsersByRole(
    role: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<ApiResponse<PaginatedResponse<UserDto>>> {
    this.setLoading(true);
    
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    const url = `${this.baseUrl}/api/UserManagement/users/by-role/${role}`;
    
    return this.http.get<ApiResponse<PaginatedResponse<UserDto>>>(url, { params })
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Search users
   * GET /api/UserManagement/users/search
   */
  searchUsers(
    searchTerm: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<ApiResponse<PaginatedResponse<UserDto>>> {
    this.setLoading(true);
    
    let params = new HttpParams()
      .set('searchTerm', searchTerm)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    const url = `${this.baseUrl}/api/UserManagement/users/search`;
    
    return this.http.get<ApiResponse<PaginatedResponse<UserDto>>>(url, { params })
      .pipe(
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Export users to Excel
   * GET /api/UserManagement/users/export
   */
  exportUsers(filters?: {
    searchTerm?: string;
    province?: string;
    district?: string;
    tehsil?: string;
    userType?: string;
    isActive?: boolean;
  }): Observable<Blob> {
    this.setLoading(true);
    
    let params = new HttpParams();
    
    if (filters) {
      if (filters.searchTerm) params = params.set('searchTerm', filters.searchTerm);
      if (filters.province) params = params.set('province', filters.province);
      if (filters.district) params = params.set('district', filters.district);
      if (filters.tehsil) params = params.set('tehsil', filters.tehsil);
      if (filters.userType) params = params.set('userType', filters.userType);
      if (filters.isActive !== undefined) params = params.set('isActive', filters.isActive.toString());
    }

    const url = `${this.baseUrl}/api/UserManagement/users/export`;
    
    return this.http.get(url, { 
      params, 
      responseType: 'blob' 
    }).pipe(
      finalize(() => this.setLoading(false))
    ) as Observable<Blob>;
  }
}