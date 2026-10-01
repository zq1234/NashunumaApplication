import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError, from } from 'rxjs';
import { catchError, finalize, tap, switchMap, map } from 'rxjs/operators';
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

  /**
   * Upload an Excel/CSV file containing batch numbers. Backend returns JSON for small files
   * or a streamed file for large files. This method inspects Content-Type to decide.
   */
  uploadBatchFile(file: File, format: 'csv' | 'xlsx') {
    const url = this.buildUrl('motherTrimister.byBatchExcel');
    const form = new FormData();
    form.append('file', file, file.name);
    const params = new HttpParams().set('format', format);

    return this.http.post(url, form, { observe: 'response', responseType: 'blob', params }).pipe(
      switchMap((response: any) => {
        const headers = response.headers;
        const contentType = (headers.get('content-type') || '').toLowerCase();
        const disposition = headers.get('content-disposition');
        const blob = response.body as Blob;

        if (contentType.includes('application/json') || contentType.includes('text/json')) {
          return from(blob.text()).pipe(
            map(text => {
              const parsed = JSON.parse(text) as ApiResponse<any>;
              return { kind: 'json', payload: parsed } as const;
            })
          );
        }

        const responseFilename = this.extractFilenameFromContentDisposition(disposition);
        const filename = this.ensureDownloadFilename(responseFilename, contentType, file.name);
        const downloadBlob = new Blob([blob], { type: contentType || 'application/octet-stream' });
        return of({ kind: 'file', blob: downloadBlob, filename, contentType } as const);
      }),
      catchError(err => throwError(() => new Error('Failed to upload or process the file. Please try again.')))
    );
  }

  private extractFilenameFromContentDisposition(header: string | null): string {
    if (!header) return '';
    try {
      const fnStar = header.match(/filename\*=(?:UTF-8'')?([^;\n]+)/i);
      if (fnStar && fnStar[1]) {
        try { return decodeURIComponent(fnStar[1].replace(/['"]/g, '').trim()); } catch { return fnStar[1].replace(/['"]/g, '').trim(); }
      }
      const fn = header.match(/filename=(?:\s*)"?([^";]+)"?/i);
      if (fn && fn[1]) return fn[1].trim();
    } catch {}
    return '';
  }

  private ensureDownloadFilename(filename: string, contentType: string, fallbackFilename: string): string {
    const normalizedType = contentType.toLowerCase();
    const isExcel = normalizedType.includes('spreadsheetml') || normalizedType.includes('ms-excel');
    const isCsv = normalizedType.includes('csv') || normalizedType.includes('text/plain');
    const expectedExtension = isExcel ? '.xlsx' : isCsv ? '.csv' : '';
    const candidate = filename || fallbackFilename || 'BatchExport';

    if (!expectedExtension) {
      return candidate;
    }

    const withoutKnownExtension = candidate.replace(/\.(xlsx|xls|csv|txt)$/i, '');
    return `${withoutKnownExtension}${expectedExtension}`;
  }

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
