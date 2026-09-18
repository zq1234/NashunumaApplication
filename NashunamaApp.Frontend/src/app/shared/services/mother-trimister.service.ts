import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse, ApiResponseHelper, ApiStatusCodes } from '@core/models/api-response.model';
import { MotherTrimisterDto } from '@core/models/mother-trimister.model';

export interface MotherTrimisterFilter {
  batchNumber: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class MotherTrimisterService {
  private baseUrl = environment.apiUrl;
  private api = environment.api;

  // Loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  // Simple cache keyed by request params
  private cache = new Map<string, { data: MotherTrimisterDto[]; totalCount: number; timestamp: number }>();
  private cacheDuration = 5 * 60 * 1000; // 5 minutes

  constructor(private http: HttpClient) {}

  private setLoading(v: boolean) { this.loadingSubject.next(v); }

  private buildUrl(key: string, pathParams?: Record<string, string | number>) {
    let url = this.api[key] ?? '';
    if (pathParams) {
      Object.keys(pathParams).forEach(k => url = url.replace(`{${k}}`, encodeURIComponent(String(pathParams[k]))));
    }
    return `${this.baseUrl}${url}`;
  }

  private isCacheValid(entry: { timestamp: number } | undefined) {
    if (!entry) return false;
    return (Date.now() - entry.timestamp) < this.cacheDuration;
  }

  private handleError(error: any): Observable<never> {
    console.error('MotherTrimister API Error:', error);
    let message = 'An error occurred';
    if (error?.error?.message) message = error.error.message;
    return throwError(() => ({ isSuccess: false, message, data: null, errors: null, statusCode: error?.status || 500 }));
  }

  /**
   * Get paged mother-trimister records by batch number
   */
  getByBatch(batchNumber: string, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginatedResponse<MotherTrimisterDto>>> {
    const cacheKey = `${batchNumber}|${pageNumber}|${pageSize}`;
    const cached = this.cache.get(cacheKey);
    if (this.isCacheValid(cached)) {
      return of({
        isSuccess: true,
        message: 'Retrieved from cache',
        data: {
          items: cached!.data,
          pageNumber,
          pageSize,
          totalCount: cached!.totalCount,
          totalPages: Math.ceil(cached!.totalCount / pageSize),
          hasPrevious: pageNumber > 1,
          hasNext: pageNumber < Math.ceil(cached!.totalCount / pageSize)
        },
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.setLoading(true);
    const url = this.buildUrl('motherTrimister.byBatch');
    const params = new HttpParams()
      .set('batchNumber', batchNumber)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<PaginatedResponse<MotherTrimisterDto>>>(url, { params }).pipe(
      tap(resp => {
        if (ApiResponseHelper.isSuccess(resp) && resp.data) {
          this.cache.set(cacheKey, { data: resp.data.items || [], totalCount: resp.data.totalCount || (resp.data.items?.length ?? 0), timestamp: Date.now() });
        }
      }),
      catchError(err => this.handleError(err)),
      finalize(() => this.setLoading(false))
    );
  }

  /**
   * Get single mother-trimister record by CNIC (or id)
   */
  getById(cnic: string): Observable<ApiResponse<MotherTrimisterDto>> {
    this.setLoading(true);
    const url = this.buildUrl('motherTrimister.getById');
    const params = new HttpParams().set('cnic', cnic);
    return this.http.get<ApiResponse<MotherTrimisterDto>>(url, { params }).pipe(
      catchError(err => this.handleError(err)),
      finalize(() => this.setLoading(false))
    );
  }

  /**
   * Export records for a batch (returns file URL or server response)
   */
  exportByBatch(batchNumber: string): Observable<ApiResponse<string>> {
    this.setLoading(true);
    const url = this.buildUrl('motherTrimister.export');
    const params = new HttpParams().set('batchNumber', batchNumber);
    return this.http.get<ApiResponse<string>>(url, { params }).pipe(
      catchError(err => this.handleError(err)),
      finalize(() => this.setLoading(false))
    );
  }
}
