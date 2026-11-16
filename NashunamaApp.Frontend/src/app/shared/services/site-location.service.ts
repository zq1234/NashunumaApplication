// src/app/core/services/site-location.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import {
  Observable, BehaviorSubject, throwError, of
} from 'rxjs';
import {
  catchError, tap, retry, map, finalize, take
} from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  PaginatedResponse
} from '@core/models/api-response.model';
import {
  SiteLocation,
  CreateSiteLocationRequest,
  UpdateSiteLocationRequest,
  SiteLocationFilter,
  ProvinceLookup,
  DistrictLookup,
  TehsilLookup,
  UcLookup,
  SiteLocationSummary
} from '@core/models/site-location.model';

@Injectable({ providedIn: 'root' })
export class SiteLocationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/SiteLocation`;
  private readonly api = environment.api;

  // ---------- Cache ----------
  private cache = new Map<string, { data: any; timestamp: number }>();
  private cacheDuration = 5 * 60 * 1000; // 5 minutes

  // ---------- Streams ----------
  private siteSubject = new BehaviorSubject<SiteLocation[]>([]);
  public sites$ = this.siteSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private totalCountSubject = new BehaviorSubject<number>(0);
  public totalCount$ = this.totalCountSubject.asObservable();

  // ============================================================
  // LOOKUPS (cascading) — cached, no loading subject (fast & cheap)
  // ============================================================

  getProvinces(): Observable<ApiResponse<ProvinceLookup[]>> {
    return this.cachedGet<ProvinceLookup[]>('siteLocation.getProvinces');
  }

  getDistricts(provinceId: number): Observable<ApiResponse<DistrictLookup[]>> {
    return this.cachedGet<DistrictLookup[]>('siteLocation.getDistricts', {}, { provinceId });
  }

  getTehsils(districtId: number): Observable<ApiResponse<TehsilLookup[]>> {
    return this.cachedGet<TehsilLookup[]>('siteLocation.getTehsils', {}, { districtId });
  }

  getUcs(tehsilId: number): Observable<ApiResponse<UcLookup[]>> {
    return this.cachedGet<UcLookup[]>('siteLocation.getUcs', {}, { tehsilId });
  }

  getSitesByFilters(
    provinceId?: number | null,
    districtId?: number | null,
    tehsilId?: number | null
  ): Observable<ApiResponse<SiteLocation[]>> {
    let params = new HttpParams();
    if (provinceId != null) params = params.set('provinceId', provinceId.toString());
    if (districtId != null) params = params.set('districtId', districtId.toString());
    if (tehsilId != null) params = params.set('tehsilId', tehsilId.toString());

    return this.http.get<ApiResponse<SiteLocation[]>>(
      this.resolve('siteLocation.getSites'),
      { params }
    ).pipe(
      retry(3),
      catchError(this.handleError<SiteLocation[]>('getSitesByFilters'))
    );
  }

  // ============================================================
  // GET PAGINATED SITES
  // ============================================================

  getSites(
    pageNumber: number = 1,
    pageSize: number = 10,
    filter?: SiteLocationFilter
  ): Observable<ApiResponse<PaginatedResponse<SiteLocation>>> {
    this.loadingSubject.next(true);

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (filter) {
      if (filter.search) params = params.set('search', filter.search);
      if (filter.provinceId != null) params = params.set('provinceId', filter.provinceId.toString());
      if (filter.districtId != null) params = params.set('districtId', filter.districtId.toString());
      if (filter.tehsilId   != null) params = params.set('tehsilId',   filter.tehsilId.toString());
      if (filter.ucId       != null) params = params.set('ucId',       filter.ucId.toString());
      if (filter.isActive   != null) params = params.set('isActive',   filter.isActive.toString());
      if (filter.isMobileSite != null) params = params.set('isMobileSite', filter.isMobileSite.toString());
      if (filter.isClosed     != null) params = params.set('isClosed',     filter.isClosed.toString());
      if (filter.sortBy) {
        params = params.set('sortBy', filter.sortBy);
        params = params.set('sortDescending', (filter.sortDescending ?? false).toString());
      }
    }

    const cacheKey = params.toString();
    const cached = this.getCachedData<PaginatedResponse<SiteLocation>>(cacheKey);
    if (cached) {
      this.loadingSubject.next(false);
      this.siteSubject.next(cached.items ?? []);
      this.totalCountSubject.next(cached.totalCount ?? 0);
      return of({
        isSuccess: true,
        message: 'Sites retrieved from cache',
        data: cached,
        errors: null,
        statusCode: 200
      } as ApiResponse<PaginatedResponse<SiteLocation>>);
    }

    const url = this.resolve('siteLocation.getAll');
    return this.http.get<ApiResponse<PaginatedResponse<SiteLocation>>>(url, { params }).pipe(
      retry(3),
      tap(res => {
        if (res.isSuccess && res.data) {
          this.cacheData(cacheKey, res.data);
          this.siteSubject.next(res.data.items ?? []);
          this.totalCountSubject.next(res.data.totalCount ?? 0);
        }
      }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<PaginatedResponse<SiteLocation>>('getSites'))
    );
  }

  /** Get all active sites for a tehsil (client-side convenience). */
  getSitesByTehsilId(tehsilId: number): Observable<ApiResponse<SiteLocation[]>> {
    return this.getSites(1, 500).pipe(
      map(res => {
        if (!res.isSuccess || !res.data?.items) {
          return { ...res, data: [] } as ApiResponse<SiteLocation[]>;
        }
        return {
          ...res,
          data: res.data.items.filter(s => s.tehsilId === tehsilId)
        } as ApiResponse<SiteLocation[]>;
      })
    );
  }

  /** Get all active sites for a UC (client-side convenience). */
  getSitesByUcId(ucId: number): Observable<ApiResponse<SiteLocation[]>> {
    return this.getSites(1, 500).pipe(
      map(res => {
        if (!res.isSuccess || !res.data?.items) {
          return { ...res, data: [] } as ApiResponse<SiteLocation[]>;
        }
        return {
          ...res,
          data: res.data.items.filter(s => s.ucId === ucId)
        } as ApiResponse<SiteLocation[]>;
      })
    );
  }

  /** Get all sites (no paging) — for dropdowns and export. */
  getAllSites(filter?: SiteLocationFilter): Observable<ApiResponse<SiteLocation[]>> {
    let params = new HttpParams().set('getAll', 'true');
    if (filter) {
      if (filter.search) params = params.set('search', filter.search);
      if (filter.provinceId != null) params = params.set('provinceId', filter.provinceId.toString());
      if (filter.districtId != null) params = params.set('districtId', filter.districtId.toString());
      if (filter.tehsilId   != null) params = params.set('tehsilId', filter.tehsilId.toString());
      if (filter.ucId       != null) params = params.set('ucId', filter.ucId.toString());
      if (filter.isActive   != null) params = params.set('isActive', filter.isActive.toString());
    }

    const url = this.resolve('siteLocation.getAll');
    return this.http.get<ApiResponse<SiteLocation[]>>(url, { params }).pipe(
      retry(3),
      catchError(this.handleError<SiteLocation[]>('getAllSites'))
    );
  }

  // ============================================================
  // GET BY ID
  // ============================================================

  getSiteById(id: number): Observable<ApiResponse<SiteLocation>> {
    const cacheKey = `id_${id}`;
    const cached = this.getCachedItem<SiteLocation>(cacheKey);
    if (cached) {
      return of({
        isSuccess: true, message: 'Site retrieved from cache', data: cached, errors: null, statusCode: 200
      } as ApiResponse<SiteLocation>);
    }

    const url = this.resolve('siteLocation.getById', { id });
    return this.http.get<ApiResponse<SiteLocation>>(url).pipe(
      retry(3),
      tap(res => { if (res.isSuccess && res.data) this.cacheItem(cacheKey, res.data); }),
      catchError(this.handleError<SiteLocation>('getSiteById'))
    );
  }

  // ============================================================
  // CREATE
  // ============================================================

  createSite(body: CreateSiteLocationRequest): Observable<ApiResponse<SiteLocation>> {
    this.loadingSubject.next(true);
    const url = this.resolve('siteLocation.create');

    return this.http.post<ApiResponse<SiteLocation>>(url, body).pipe(
      tap(res => {
        if (res.isSuccess && res.data) {
          this.invalidateCache();
          this.siteSubject.next([res.data, ...this.siteSubject.value]);
          this.totalCountSubject.next(this.totalCountSubject.value + 1);
        }
      }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<SiteLocation>('createSite'))
    );
  }

  // ============================================================
  // UPDATE
  // ============================================================

  updateSite(id: number, body: UpdateSiteLocationRequest): Observable<ApiResponse<SiteLocation>> {
    this.loadingSubject.next(true);
    const url = this.resolve('siteLocation.update', { id });

    return this.http.put<ApiResponse<SiteLocation>>(url, body).pipe(
      tap(res => {
        if (res.isSuccess && res.data) {
          this.invalidateCache();
          const items = [...this.siteSubject.value];
          const idx = items.findIndex(x => x.id === id);
          if (idx !== -1) { items[idx] = res.data; this.siteSubject.next(items); }
        }
      }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<SiteLocation>('updateSite'))
    );
  }

  // ============================================================
  // DELETE
  // ============================================================

  deleteSite(id: number): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);
    const url = this.resolve('siteLocation.delete', { id });

    return this.http.delete<ApiResponse<boolean>>(url).pipe(
      tap(res => {
        if (res.isSuccess) {
          this.invalidateCache();
          this.siteSubject.next(this.siteSubject.value.filter(x => x.id !== id));
          this.totalCountSubject.next(Math.max(0, this.totalCountSubject.value - 1));
        }
      }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<boolean>('deleteSite'))
    );
  }

  /** Bulk delete — falls back to sequential deletes if backend has no bulk endpoint. */
  deleteMultipleSites(ids: number[]): Observable<ApiResponse<boolean>> {
    if (!ids?.length) {
      return of({ isSuccess: true, message: 'Nothing to delete', data: true, errors: null, statusCode: 200 });
    }
    const hasBulk = !!(this.api as any)['siteLocation.bulkDelete'];
    if (!hasBulk) {
      // Sequential fallback
      return new Observable<ApiResponse<boolean>>(observer => {
        let remaining = ids.length;
        let anyError: string | null = null;
        ids.forEach(id => this.deleteSite(id).subscribe(r => {
          if (!r.isSuccess) anyError = r.message;
          if (--remaining === 0) {
            observer.next({
              isSuccess: !anyError,
              message: anyError ?? 'Deleted',
              data: !anyError,
              errors: anyError ? [anyError] : null,
              statusCode: anyError ? 400 : 200
            });
            observer.complete();
          }
        }));
      });
    }

    this.loadingSubject.next(true);
    const url = this.resolve('siteLocation.bulkDelete');
    return this.http.post<ApiResponse<boolean>>(url, { ids }).pipe(
      tap(res => {
        if (res.isSuccess) {
          this.invalidateCache();
          const set = new Set(ids);
          this.siteSubject.next(this.siteSubject.value.filter(x => !set.has(x.id)));
          this.totalCountSubject.next(Math.max(0, this.totalCountSubject.value - ids.length));
        }
      }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<boolean>('deleteMultipleSites'))
    );
  }

  // ============================================================
  // SUMMARY / STATISTICS
  // ============================================================

  getSummary(): Observable<ApiResponse<SiteLocationSummary>> {
    const url = this.resolve('siteLocation.summary');
    return this.http.get<ApiResponse<SiteLocationSummary>>(url).pipe(
      retry(3),
      catchError(this.handleError<SiteLocationSummary>('getSummary'))
    );
  }

  // ============================================================
  // EXPORT / IMPORT
  // ============================================================

  exportSites(format: 'excel' | 'csv' = 'excel', filter?: SiteLocationFilter): Observable<Blob> {
    let params = new HttpParams().set('format', format);
    if (filter) {
      if (filter.search) params = params.set('search', filter.search);
      if (filter.provinceId != null) params = params.set('provinceId', filter.provinceId.toString());
      if (filter.districtId != null) params = params.set('districtId', filter.districtId.toString());
      if (filter.tehsilId   != null) params = params.set('tehsilId',   filter.tehsilId.toString());
    }

    const url = this.resolve('siteLocation.export');
    return this.http.get(url, { params, responseType: 'blob' }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Export error:', error);
        return throwError(() => error);
      })
    );
  }

  importSites(file: File): Observable<ApiResponse<any>> {
    const url = this.resolve('siteLocation.import');
    const form = new FormData();
    form.append('file', file);

    this.loadingSubject.next(true);
    return this.http.post<ApiResponse<any>>(url, form).pipe(
      tap(res => { if (res.isSuccess) this.invalidateCache(); }),
      finalize(() => this.loadingSubject.next(false)),
      catchError(this.handleError<any>('importSites'))
    );
  }

  // ============================================================
  // CACHE MANAGEMENT
  // ============================================================

  private cacheData(key: string, data: any): void {
    this.cache.set(key, { data, timestamp: Date.now() });
  }

  private cacheItem(key: string, data: any): void {
    this.cache.set(key, { data, timestamp: Date.now() });
  }

  private getCachedData<T>(key: string): T | null {
    const hit = this.cache.get(key);
    if (hit && Date.now() - hit.timestamp < this.cacheDuration) return hit.data as T;
    return null;
  }

  private getCachedItem<T>(key: string): T | null {
    return this.getCachedData<T>(key);
  }

  private invalidateCache(): void { this.cache.clear(); }
  private invalidateCacheEntry(key: string): void { this.cache.delete(key); }

  refresh(): void { this.invalidateCache(); }

  getCurrentData(): SiteLocation[] { return this.siteSubject.value; }

  clearData(): void {
    this.invalidateCache();
    this.siteSubject.next([]);
    this.totalCountSubject.next(0);
  }

  hasData(): boolean { return this.siteSubject.value.length > 0; }
  isLoading(): boolean { return this.loadingSubject.value; }

  clearExpiredCache(): void {
    const now = Date.now();
    Array.from(this.cache.entries()).forEach(([k, v]) => {
      if (now - v.timestamp > this.cacheDuration) this.cache.delete(k);
    });
  }

  getCacheStats(): { size: number; entries: string[] } {
    return { size: this.cache.size, entries: Array.from(this.cache.keys()) };
  }

  // ============================================================
  // HELPERS
  // ============================================================

  private resolve(key: string, pathParams: Record<string, string | number> = {}): string {
    let path = (this.api as any)[key] as string;
    if (!path) throw new Error(`Endpoint '${key}' not found in environment.api`);
    Object.entries(pathParams).forEach(([k, v]) => {
      path = path.replace(`{${k}}`, encodeURIComponent(String(v)));
    });
    return `${environment.apiUrl}${path}`;
  }

  private cachedGet<T>(
    key: string,
    pathParams: Record<string, string | number> = {},
    query: Record<string, string | number> = {}
  ): Observable<ApiResponse<T>> {
    const url = this.resolve(key, pathParams);
    let params = new HttpParams();
    Object.entries(query).forEach(([k, v]) => (params = params.set(k, String(v))));

    const cacheKey = `${key}?${params.toString()}`;
    const cached = this.getCachedData<T>(cacheKey);
    if (cached) {
      return of({
        isSuccess: true, message: 'Retrieved from cache',
        data: cached, errors: null, statusCode: 200
      } as ApiResponse<T>);
    }

    return this.http.get<ApiResponse<T>>(url, { params }).pipe(
      retry(2),
      tap(res => { if (res.isSuccess && res.data) this.cacheData(cacheKey, res.data); }),
      catchError(this.handleError<T>(key))
    );
  }

  // ============================================================
  // ERROR HANDLING
  // ============================================================

  private handleError<T>(operation = 'operation') {
    return (error: HttpErrorResponse): Observable<never> => {
      let message = 'An error occurred';

      this.loadingSubject.next(false);

      if (error.error instanceof ErrorEvent) {
        message = error.error.message;
        console.error(`[client] ${operation}: ${message}`);
      } else {
        message = error.error?.message || error.message || `Error ${error.status}: ${error.statusText}`;
        console.error(`[server] ${operation}: ${message}`);

        if (error.status === 0) message = 'Cannot reach the server. Check your connection.';
        if (error.status === 401) message = 'Not authorized — please sign in again.';
        if (error.status === 403) message = 'You do not have permission to perform this action.';
        if (error.status === 404) message = 'The requested resource was not found.';
        if (error.status === 409) message = 'A site with this name already exists.';
        if (error.status === 500) message = 'Server error — please try again later.';
      }

      return throwError(() => ({
        isSuccess: false,
        message,
        data: null as T,
        errors: error.error?.errors || [message],
        statusCode: error.status || 500
      }));
    };
  }
}