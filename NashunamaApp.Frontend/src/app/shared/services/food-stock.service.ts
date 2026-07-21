// shared/services/food-stock.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { catchError, tap, retry, shareReplay, map, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '@core/models/api-response.model';
import { FoodStock, FoodStockFilter, CreateFoodStockDto, UpdateFoodStockDto } from '@core/models/food-stock.model';
import { take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class FoodStockService {
  private apiUrl = `${environment.apiUrl}/api/FoodStock`;

  // Cache for food stock data
  private cache = new Map<string, { data: any; timestamp: number }>();
  private cacheDuration = 5 * 60 * 1000; // 5 minutes

  // Observable for real-time updates
  private foodStockSubject = new BehaviorSubject<FoodStock[]>([]);
  public foodStocks$ = this.foodStockSubject.asObservable();

  // Loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  // Total count
  private totalCountSubject = new BehaviorSubject<number>(0);
  public totalCount$ = this.totalCountSubject.asObservable();

  constructor(private http: HttpClient) { }

  // ============================================================
  // GET FOOD STOCKS WITH PAGINATION AND FILTERS
  // ============================================================

  /**
   * Get paginated food stocks with filters
   */
  getFoodStocks(
    pageNumber: number = 1,
    pageSize: number = 10,
    filter?: FoodStockFilter
  ): Observable<ApiResponse<PaginatedResponse<FoodStock>>> {
    this.loadingSubject.next(true);

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    // Add filter parameters if provided
    if (filter) {
      if (filter.searchTerm) {
        params = params.set('searchTerm', filter.searchTerm);
      }
      if (filter.siteName) {
        params = params.set('siteName', filter.siteName);
      }
      if (filter.province) {
        params = params.set('province', filter.province);
      }
      if (filter.district) {
        params = params.set('district', filter.district);
      }
      if (filter.tehsil) {
        params = params.set('tehsil', filter.tehsil);
      }
      if (filter.isActive !== undefined && filter.isActive !== null) {
        params = params.set('isActive', filter.isActive.toString());
      }
      if (filter.isMobileSite) {
        params = params.set('isMobileSite', filter.isMobileSite);
      }
      if (filter.isClosed) {
        params = params.set('isClosed', filter.isClosed);
      }
      if (filter.isManualUpdate) {
        params = params.set('isManualUpdate', filter.isManualUpdate);
      }
      if (filter.sortBy) {
        params = params.set('sortBy', filter.sortBy);
        params = params.set('sortDescending', (filter.sortDescending || false).toString());
      }
    }

    // Check cache
    const cacheKey = params.toString();
    const cached = this.getCachedData(cacheKey);

    if (cached) {
      this.loadingSubject.next(false);
      this.foodStockSubject.next(cached.items);
      this.totalCountSubject.next(cached.totalCount);

      return new Observable<ApiResponse<PaginatedResponse<FoodStock>>>((observer) => {
        observer.next({
          isSuccess: true,
          message: 'Food stocks retrieved from cache',
          data: cached,
          errors: null,
          statusCode: 200
        });
        observer.complete();
      });
    }

    return this.http.get<ApiResponse<PaginatedResponse<FoodStock>>>(this.apiUrl, { params })
      .pipe(
        retry(3),
        tap((response: ApiResponse<PaginatedResponse<FoodStock>>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess && response.data) {
            // Cache the response
            this.cacheData(cacheKey, response.data);
            // Update subjects
            this.foodStockSubject.next(response.data.items);
            this.totalCountSubject.next(response.data.totalCount);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<PaginatedResponse<FoodStock>>('getFoodStocks'))
      );
  }

  // ============================================================
  // GET ALL FOOD STOCKS (Without Pagination)
  // ============================================================

  /**
   * Get all food stocks (without pagination)
   */
  getAllFoodStocks(filter?: Partial<FoodStockFilter>): Observable<ApiResponse<FoodStock[]>> {
    let params = new HttpParams();

    if (filter) {
      if (filter.searchTerm) {
        params = params.set('searchTerm', filter.searchTerm);
      }
      if (filter.siteName) {
        params = params.set('siteName', filter.siteName);
      }
      if (filter.province) {
        params = params.set('province', filter.province);
      }
      if (filter.district) {
        params = params.set('district', filter.district);
      }
      if (filter.tehsil) {
        params = params.set('tehsil', filter.tehsil);
      }
      if (filter.sortBy) {
        params = params.set('sortBy', filter.sortBy);
        params = params.set('sortDescending', (filter.sortDescending ?? true).toString());
      }
      params = params.set('getAll', 'true');
    }

    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/all`, { params })
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getAllFoodStocks'))
      );
  }

  // ============================================================
  // GET SINGLE FOOD STOCK BY ID
  // ============================================================

  /**
   * Get a single food stock by ID
   */
  getFoodStockById(id: number): Observable<ApiResponse<FoodStock>> {
    // Check cache first
    const cacheKey = `id_${id}`;
    const cached = this.getCachedItem(cacheKey);

    if (cached) {
      return new Observable<ApiResponse<FoodStock>>((observer) => {
        observer.next({
          isSuccess: true,
          message: 'Food stock retrieved from cache',
          data: cached,
          errors: null,
          statusCode: 200
        });
        observer.complete();
      });
    }

    return this.http.get<ApiResponse<FoodStock>>(`${this.apiUrl}/${id}`)
      .pipe(
        retry(3),
        tap((response: ApiResponse<FoodStock>) => {
          if (response.isSuccess && response.data) {
            this.cacheItem(cacheKey, response.data);
          }
        }),
        catchError(this.handleError<FoodStock>('getFoodStockById'))
      );
  }

  // ============================================================
  // CREATE FOOD STOCK
  // ============================================================

  /**
   * Create a new food stock record
   */
  createFoodStock(foodStock: CreateFoodStockDto): Observable<ApiResponse<FoodStock>> {
    this.loadingSubject.next(true);

    return this.http.post<ApiResponse<FoodStock>>(this.apiUrl, foodStock)
      .pipe(
        tap((response: ApiResponse<FoodStock>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess && response.data) {
            // Invalidate cache
            this.invalidateCache();
            // Update current list
            const currentItems = this.foodStockSubject.value;
            this.foodStockSubject.next([response.data, ...currentItems]);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<FoodStock>('createFoodStock'))
      );
  }

  // ============================================================
  // UPDATE FOOD STOCK
  // ============================================================

  /**
   * Update an existing food stock record
   */
  updateFoodStock(id: number, foodStock: UpdateFoodStockDto): Observable<ApiResponse<FoodStock>> {
    this.loadingSubject.next(true);

    return this.http.put<ApiResponse<FoodStock>>(`${this.apiUrl}/${id}`, foodStock)
      .pipe(
        tap((response: ApiResponse<FoodStock>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess && response.data) {
            // Invalidate cache
            this.invalidateCache();
            // Update current list
            const currentItems = this.foodStockSubject.value;
            const index = currentItems.findIndex(item => item.id === id);
            if (index !== -1) {
              currentItems[index] = response.data;
              this.foodStockSubject.next([...currentItems]);
            }
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<FoodStock>('updateFoodStock'))
      );
  }

  // ============================================================
  // DELETE FOOD STOCK
  // ============================================================

  /**
   * Delete a food stock record
   */
  deleteFoodStock(id: number): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);

    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
      .pipe(
        tap((response: ApiResponse<boolean>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess) {
            // Invalidate cache
            this.invalidateCache();
            // Remove from current list
            const currentItems = this.foodStockSubject.value;
            const updatedItems = currentItems.filter(item => item.id !== id);
            this.foodStockSubject.next(updatedItems);
            // Update total count
            this.totalCountSubject.next(this.totalCountSubject.value - 1);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<boolean>('deleteFoodStock'))
      );
  }

  /**
   * Delete multiple food stock records (bulk delete)
   */
  deleteMultipleFoodStocks(ids: number[]): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);

    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/bulk-delete`, { ids })
      .pipe(
        tap((response: ApiResponse<boolean>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess) {
            // Invalidate cache
            this.invalidateCache();
            // Remove deleted items from current list
            const currentItems = this.foodStockSubject.value;
            //const updatedItems = currentItems.filter(item => !ids.includes(item.ids));
           // this.foodStockSubject.next(updatedItems);
            this.totalCountSubject.next(this.totalCountSubject.value - ids.length);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<boolean>('deleteMultipleFoodStocks'))
      );
  }

  // ============================================================
  // BULK OPERATIONS
  // ============================================================

  /**
   * Bulk create food stocks
   */
  bulkCreateFoodStocks(foodStocks: CreateFoodStockDto[]): Observable<ApiResponse<FoodStock[]>> {
    this.loadingSubject.next(true);

    return this.http.post<ApiResponse<FoodStock[]>>(`${this.apiUrl}/bulk`, foodStocks)
      .pipe(
        tap((response: ApiResponse<FoodStock[]>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess && response.data) {
            // Invalidate cache
            this.invalidateCache();
            // Update current list
            const currentItems = this.foodStockSubject.value;
            this.foodStockSubject.next([...response.data, ...currentItems]);
            this.totalCountSubject.next(this.totalCountSubject.value + response.data.length);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<FoodStock[]>('bulkCreateFoodStocks'))
      );
  }

  /**
   * Bulk update food stocks
   */
  bulkUpdateFoodStocks(foodStocks: UpdateFoodStockDto[]): Observable<ApiResponse<FoodStock[]>> {
    this.loadingSubject.next(true);

    return this.http.put<ApiResponse<FoodStock[]>>(`${this.apiUrl}/bulk`, foodStocks)
      .pipe(
        tap((response: ApiResponse<FoodStock[]>) => {
          this.loadingSubject.next(false);
          if (response.isSuccess && response.data) {
            // Invalidate cache
            this.invalidateCache();
            // Update current list
            const currentItems = this.foodStockSubject.value;
            const updatedIds = response.data.map((item: FoodStock) => item.id);
            const filteredItems = currentItems.filter(item => !updatedIds.includes(item.id));
            this.foodStockSubject.next([...response.data, ...filteredItems]);
          }
        }),
        finalize(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError<FoodStock[]>('bulkUpdateFoodStocks'))
      );
  }

  // ============================================================
  // VALIDATION METHODS
  // ============================================================

  /**
   * Check if a food stock exists
   */
  checkFoodStockExists(id: number): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.apiUrl}/${id}/exists`)
      .pipe(
        retry(3),
        catchError(this.handleError<boolean>('checkFoodStockExists'))
      );
  }

  /**
   * Validate food stock data
   */
  validateFoodStock(foodStock: CreateFoodStockDto | UpdateFoodStockDto): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/validate`, foodStock)
      .pipe(
        catchError(this.handleError<boolean>('validateFoodStock'))
      );
  }

  // ============================================================
  // ADDITIONAL SEARCH METHODS
  // ============================================================

  /**
   * Search food stocks with advanced filters
   */
  advancedSearchFoodStocks(filters: {
    siteName?: string;
    province?: string;
    district?: string;
    tehsil?: string;
    isActive?: boolean;
    isMobileSite?: boolean;
    isClosed?: boolean;
    isManualUpdate?: boolean;
    fromDate?: string;
    toDate?: string;
    minTarget?: number;
    maxTarget?: number;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<ApiResponse<PaginatedResponse<FoodStock>>> {
    let params = new HttpParams();

    // Add all filter parameters
    Object.keys(filters).forEach(key => {
      const value = filters[key as keyof typeof filters];
      if (value !== undefined && value !== null) {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<PaginatedResponse<FoodStock>>>(`${this.apiUrl}/advanced-search`, { params })
      .pipe(
        retry(3),
        catchError(this.handleError<PaginatedResponse<FoodStock>>('advancedSearchFoodStocks'))
      );
  }

  /**
   * Get food stocks by province
   */
  getFoodStocksByProvince(province: string): Observable<ApiResponse<FoodStock[]>> {
    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/province/${province}`)
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getFoodStocksByProvince'))
      );
  }

  /**
   * Get food stocks by district
   */
  getFoodStocksByDistrict(district: string): Observable<ApiResponse<FoodStock[]>> {
    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/district/${district}`)
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getFoodStocksByDistrict'))
      );
  }

  /**
   * Get food stocks by tehsil
   */
  getFoodStocksByTehsil(tehsil: string): Observable<ApiResponse<FoodStock[]>> {
    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/tehsil/${tehsil}`)
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getFoodStocksByTehsil'))
      );
  }

  // ============================================================
  // CHART AND REPORT METHODS
  // ============================================================

  /**
   * Get data for charts/dashboard
   */
  getChartData(chartType: string, filters?: any): Observable<ApiResponse<any>> {
    let params = new HttpParams().set('chartType', chartType);
    
    if (filters) {
      if (filters.province) params = params.set('province', filters.province);
      if (filters.district) params = params.set('district', filters.district);
      if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
      if (filters.toDate) params = params.set('toDate', filters.toDate);
    }

    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/chart-data`, { params })
      .pipe(
        retry(3),
        catchError(this.handleError<any>('getChartData'))
      );
  }

  /**
   * Download report
   */
  downloadReport(reportType: string, filters?: any): Observable<Blob> {
    let params = new HttpParams().set('reportType', reportType);

    if (filters) {
      if (filters.province) params = params.set('province', filters.province);
      if (filters.district) params = params.set('district', filters.district);
      if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
      if (filters.toDate) params = params.set('toDate', filters.toDate);
    }

    return this.http.get(`${this.apiUrl}/report`, {
      params,
      responseType: 'blob'
    }).pipe(
      catchError((error) => {
        console.error('Report download error:', error);
        return throwError(() => error);
      })
    );
  }

  // ============================================================
  // CACHE MANAGEMENT ENHANCEMENTS
  // ============================================================

  /**
   * Prefetch data for better performance
   */
  prefetchData(filters?: FoodStockFilter): void {
    let params = new HttpParams()
      .set('pageNumber', '1')
      .set('pageSize', '100')
      .set('prefetch', 'true');

    if (filters?.searchTerm) {
      params = params.set('searchTerm', filters.searchTerm);
    }

    this.http.get<ApiResponse<PaginatedResponse<FoodStock>>>(`${this.apiUrl}/prefetch`, { params })
      .pipe(
        take(1),
        tap((response: ApiResponse<PaginatedResponse<FoodStock>>) => {
          if (response.isSuccess && response.data) {
            const cacheKey = 'prefetch_' + Date.now();
            this.cacheData(cacheKey, response.data);
          }
        }),
        catchError((error) => {
          console.warn('Prefetch failed:', error);
          return of(null);
        })
      ).subscribe();
  }

  /**
   * Clear expired cache entries
   */
  clearExpiredCache(): void {
    const now = Date.now();
    const entries = Array.from(this.cache.entries());
    
    entries.forEach(([key, value]) => {
      if ((now - value.timestamp) > this.cacheDuration) {
        this.cache.delete(key);
      }
    });
  }

  /**
   * Get cache statistics
   */
  getCacheStats(): { size: number; entries: string[] } {
    const entries = Array.from(this.cache.keys());
    return {
      size: this.cache.size,
      entries: entries
    };
  }
  
  // ============================================================
  // SEARCH AND FILTER OPERATIONS
  // ============================================================

  /**
   * Search food stocks by term
   */
  searchFoodStocks(searchTerm: string): Observable<ApiResponse<PaginatedResponse<FoodStock>>> {
    const params = new HttpParams()
      .set('searchTerm', searchTerm)
      .set('pageNumber', '1')
      .set('pageSize', '100');

    return this.http.get<ApiResponse<PaginatedResponse<FoodStock>>>(this.apiUrl, { params })
      .pipe(
        retry(3),
        catchError(this.handleError<PaginatedResponse<FoodStock>>('searchFoodStocks'))
      );
  }

  /**
   * Get food stocks by site
   */
  getFoodStocksBySiteName(siteName: string): Observable<ApiResponse<FoodStock[]>> {
    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/site/${siteName}`)
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getFoodStocksBySite'))
      );
  }

  /**
   * Get food stocks by entered by
   */
  getFoodStocksByEnteredBy(enteredBy: string): Observable<ApiResponse<FoodStock[]>> {
    return this.http.get<ApiResponse<FoodStock[]>>(`${this.apiUrl}/enteredby/${enteredBy}`)
      .pipe(
        retry(3),
        catchError(this.handleError<FoodStock[]>('getFoodStocksByEnteredBy'))
      );
  }

  // ============================================================
  // SUMMARY AND STATISTICS
  // ============================================================

  /**
   * Get summary statistics
   */
  getSummaryStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/summary`)
      .pipe(
        retry(3),
        catchError(this.handleError<any>('getSummaryStats'))
      );
  }

  /**
   * Get stock history for a site
   */
  getSiteStockHistory(siteName: string, fromDate?: string, toDate?: string): Observable<ApiResponse<any>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/history/${siteName}`, { params })
      .pipe(
        retry(3),
        catchError(this.handleError<any>('getSiteStockHistory'))
      );
  }

  // ============================================================
  // EXPORT OPERATIONS
  // ============================================================

  /**
   * Export food stock data to Excel/CSV
   */
  exportFoodStocks(format: 'excel' | 'csv' = 'excel', filter?: Partial<FoodStockFilter>): Observable<Blob> {
    let params = new HttpParams().set('format', format);

    if (filter) {
      if (filter.searchTerm) {
        params = params.set('searchTerm', filter.searchTerm);
      }
      if (filter.siteName) {
        params = params.set('siteName', filter.siteName);
      }
    }

    return this.http.get(`${this.apiUrl}/export`, {
      params,
      responseType: 'blob'
    }).pipe(
      catchError((error) => {
        console.error('Export error:', error);
        return throwError(() => error);
      })
    );
  }

  // ============================================================
  // CACHE MANAGEMENT
  // ============================================================

  /**
   * Cache data with timestamp
   */
  private cacheData(key: string, data: PaginatedResponse<FoodStock>): void {
    this.cache.set(key, {
      data: data,
      timestamp: Date.now()
    });
  }

  /**
   * Cache a single item
   */
  private cacheItem(key: string, data: FoodStock): void {
    this.cache.set(key, {
      data: data,
      timestamp: Date.now()
    });
  }

  /**
   * Get cached data if valid
   */
  private getCachedData(key: string): PaginatedResponse<FoodStock> | null {
    const cached = this.cache.get(key);
    if (cached && (Date.now() - cached.timestamp) < this.cacheDuration) {
      return cached.data;
    }
    return null;
  }

  /**
   * Get cached single item
   */
  private getCachedItem(key: string): FoodStock | null {
    const cached = this.cache.get(key);
    if (cached && (Date.now() - cached.timestamp) < this.cacheDuration) {
      return cached.data;
    }
    return null;
  }

  /**
   * Invalidate entire cache
   */
  private invalidateCache(): void {
    this.cache.clear();
  }

  /**
   * Invalidate specific cache entry
   */
  private invalidateCacheEntry(key: string): void {
    this.cache.delete(key);
  }

  // ============================================================
  // HELPERS
  // ============================================================

  /**
   * Refresh current data
   */
  refresh(): void {
    this.invalidateCache();
  }

  /**
   * Get current data without loading
   */
  getCurrentData(): FoodStock[] {
    return this.foodStockSubject.value;
  }

  /**
   * Clear all data
   */
  clearData(): void {
    this.invalidateCache();
    this.foodStockSubject.next([]);
    this.totalCountSubject.next(0);
  }

  /**
   * Check if data is loaded
   */
  hasData(): boolean {
    return this.foodStockSubject.value.length > 0;
  }

  /**
   * Get loading state
   */
  isLoading(): boolean {
    return this.loadingSubject.value;
  }

  // ============================================================
  // ERROR HANDLING
  // ============================================================

  /**
   * Generic error handler
   */
  private handleError<T>(operation = 'operation') {
    return (error: HttpErrorResponse): Observable<never> => {
      let errorMessage = 'An error occurred while performing the operation';

      this.loadingSubject.next(false);

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = error.error.message;
        console.error(`Client-side error: ${errorMessage}`);
      } else {
        // Server-side error
        errorMessage = error.error?.message || error.message || `Error ${error.status}: ${error.statusText}`;
        console.error(`Server-side error: ${errorMessage}`);

        // Handle specific status codes
        if (error.status === 404) {
          errorMessage = 'The requested resource was not found';
        } else if (error.status === 403) {
          errorMessage = 'You do not have permission to perform this action';
        } else if (error.status === 500) {
          errorMessage = 'An internal server error occurred. Please try again later';
        } else if (error.status === 0) {
          errorMessage = 'Unable to connect to the server. Please check your internet connection';
        }
      }

      return throwError(() => ({
        isSuccess: false,
        message: errorMessage,
        data: null as T,
        errors: error.error?.errors || [errorMessage],
        statusCode: error.status || 500
      }));
    };
  }
}