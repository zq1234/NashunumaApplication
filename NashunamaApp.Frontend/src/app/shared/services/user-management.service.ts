// src/app/shared/services/user-management.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { catchError, finalize, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse, ApiResponseHelper, ApiStatusCodes } from '@core/models/api-response.model';
import {
  UserDto,
  UserFilterParams,
  UserSearchParams,
  UserHelper
} from '@core/models/user.model';
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

  // Cache
  private usersCache = new Map<string, { data: UserDto[], totalCount: number, timestamp: number }>();
  private cacheDuration = 5 * 60 * 1000; // 5 minutes

  constructor(private http: HttpClient) {}

  /**
   * Set loading state
   */
  private setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  /**
   * Generate cache key from filters
   */
  private getCacheKey(params: UserFilterParams): string {
    return JSON.stringify(params);
  }

  /**
   * Check if cache is valid
   */
  private isCacheValid(cacheEntry: { timestamp: number }): boolean {
    return Date.now() - cacheEntry.timestamp < this.cacheDuration;
  }

  /**
   * Handle error responses and return formatted ApiResponse
   */
  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    let errorMessage = 'An error occurred. Please try again.';
    let errors: string[] = [errorMessage];
    let statusCode = error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR;

    if (error.error) {
      if (typeof error.error === 'string') {
        errorMessage = error.error;
        errors = [errorMessage];
      } else if (error.error.message) {
        errorMessage = error.error.message;
        errors = [errorMessage];
      } else if (error.error.errors) {
        if (typeof error.error.errors === 'object') {
          const validationErrors = Object.values(error.error.errors).flat();
          errors = validationErrors as string[];
          errorMessage = errors.join(', ');
        } else if (Array.isArray(error.error.errors)) {
          errors = error.error.errors;
          errorMessage = errors.join(', ');
        }
      } else if (error.error.data) {
        errorMessage = error.error.data;
        errors = [errorMessage];
      }
    } else if (error.message) {
      errorMessage = error.message;
      errors = [errorMessage];
    }

    const errorResponse: ApiResponse<any> = {
      isSuccess: false,
      message: errorMessage,
      data: null,
      errors: errors,
      statusCode: statusCode
    };

    return throwError(() => errorResponse);
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
    const cacheKey = this.getCacheKey(params);
    const cached = this.usersCache.get(cacheKey);
    if (cached && this.isCacheValid(cached)) {
      const response: ApiResponse<PaginatedResponse<UserDto>> = {
        isSuccess: true,
        message: 'Users retrieved from cache',
        data: {
          items: cached.data,
          pageNumber: params.pageNumber,
          pageSize: params.pageSize,
          totalCount: cached.totalCount,
          totalPages: Math.ceil(cached.totalCount / params.pageSize),
          hasPrevious: params.pageNumber > 1,
          hasNext: params.pageNumber < Math.ceil(cached.totalCount / params.pageSize)
        },
        errors: null,
        statusCode: ApiStatusCodes.OK
      };
      return of(response);
    }

    this.setLoading(true);

    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

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
        tap(response => {
          if (ApiResponseHelper.isSuccess(response) && response.data) {
            this.usersCache.set(cacheKey, {
              data: response.data.items,
              totalCount: response.data.totalCount,
              timestamp: Date.now()
            });
          }
        }),
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
        tap(() => {
          this.usersCache.clear();
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Toggle user status (Activate/Deactivate)
   * PATCH /api/UserManagement/users/{username}/status
   * Body: raw boolean (matches [FromBody] bool isActive)
   */
  toggleUserStatus(username: string, isActive: boolean): Observable<ApiResponse<boolean>> {
    this.setLoading(true);
    const url = this.buildUrl('user.toggleStatus', { username });

    return this.http.patch<ApiResponse<boolean>>(url, isActive)
      .pipe(
        tap(() => {
          this.usersCache.clear();
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Block user
   * PATCH /api/UserManagement/users/{username}/block
   */
  blockUser(username: string): Observable<ApiResponse<boolean>> {
    this.setLoading(true);
    const url = this.buildUrl('user.blockUser', { username });

    return this.http.patch<ApiResponse<boolean>>(url, {})
      .pipe(
        tap(() => {
          this.usersCache.clear();
        }),
        catchError(this.handleError),
        finalize(() => this.setLoading(false))
      );
  }

  /**
   * Unblock user
   * PATCH /api/UserManagement/users/{username}/unblock
   */
  unblockUser(username: string): Observable<ApiResponse<boolean>> {
    this.setLoading(true);
    const url = this.buildUrl('user.unblockUser', { username });

    return this.http.patch<ApiResponse<boolean>>(url, {})
      .pipe(
        tap(() => {
          this.usersCache.clear();
        }),
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
      catchError((error) => {
        this.setLoading(false);
        throw error;
      }),
      finalize(() => this.setLoading(false))
    ) as Observable<Blob>;
  }

  /**
   * Clear user cache
   */
  clearUserCache(): void {
    this.usersCache.clear();
  }
}