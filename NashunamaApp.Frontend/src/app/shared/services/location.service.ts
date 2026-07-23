// src/app/core/services/location.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, ApiResponseHelper, ApiStatusCodes } from '@core/models/api-response.model';

export interface Province {
  id: number;
  provcode: number;
  province: string;
}

export interface District {
  id: number;
  distcode: number;
  district: string;
  provcode: number;
}

export interface Tehsil {
  id: number;
  distcode: number;
  tehsilcode: number;
  tehsil: string;
}

export interface Uc {
  id: number;
  tehsilcode: number;
  uccode: number;
  uctype: string;
  ucno: number;
  uc: string;
}

export interface ProvinceWithDistricts {
  id: number;
  provcode: number;
  province: string;
  districts: DistrictWithTehsils[];
}

export interface DistrictWithTehsils {
  id: number;
  distcode: number;
  district: string;
  provcode: number;
  tehsils: TehsilWithUcs[];
}

export interface TehsilWithUcs {
  id: number;
  distcode: number;
  tehsilcode: number;
  tehsil: string;
  ucs: Uc[];
}

export interface LocationHierarchy {
  provinces: ProvinceWithDistricts[];
}

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private baseUrl = environment.apiUrl;
  private api = environment.api;

  // Cache for location data - using strings as keys (names)
  private provincesCache = new BehaviorSubject<Province[]>([]);
  private districtsCache = new Map<string, District[]>(); // Key: province name
  private tehsilsCache = new Map<string, Tehsil[]>(); // Key: district name
  private ucsCache = new Map<string, Uc[]>(); // Key: tehsil name
  
  // Loading states
  private loadingProvinces = false;
  private loadingDistricts = new Map<string, boolean>();
  private loadingTehsils = new Map<string, boolean>();
  private loadingUcs = new Map<string, boolean>();

  constructor(private http: HttpClient) {}

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
   * Get all provinces
   * GET /api/Location/provinces
   */
  getProvinces(): Observable<ApiResponse<Province[]>> {
    // Check if already loading
    if (this.loadingProvinces) {
      return this.provincesCache.asObservable().pipe(
        map(data => ({
          isSuccess: true,
          message: 'Provinces loading...',
          data: data,
          errors: null,
          statusCode: ApiStatusCodes.OK
        }))
      );
    }

    // Check cache
    const cachedData = this.provincesCache.getValue();
    if (cachedData && cachedData.length > 0) {
      return of({
        isSuccess: true,
        message: 'Provinces retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingProvinces = true;
    const url = this.buildUrl('location.getProvinces');
    
    return this.http.get<ApiResponse<Province[]>>(url).pipe(
      tap(response => {
        this.loadingProvinces = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.provincesCache.next(response.data);
        }
      }),
      catchError(error => {
        this.loadingProvinces = false;
        const errorResponse: ApiResponse<Province[]> = {
          isSuccess: false,
          message: error.message || 'Failed to load provinces',
          data: [],
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get districts by province name
   * GET /api/Location/districts/{provinceName}
   */
  getDistrictsByProvince(provinceName: string): Observable<ApiResponse<District[]>> {
    if (!provinceName) {
      return of({
        isSuccess: false,
        message: 'Province name is required',
        data: [],
        errors: ['Province name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    // Check if already loading
    if (this.loadingDistricts.get(provinceName)) {
      const cached = this.districtsCache.get(provinceName) || [];
      return of({
        isSuccess: true,
        message: 'Districts loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.districtsCache.has(provinceName)) {
      const cachedData = this.districtsCache.get(provinceName)!;
      return of({
        isSuccess: true,
        message: 'Districts retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingDistricts.set(provinceName, true);
    const url = this.buildUrl('location.getDistrictsByProvince', { provinceName });
    
    return this.http.get<ApiResponse<District[]>>(url).pipe(
      tap(response => {
        this.loadingDistricts.set(provinceName, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.districtsCache.set(provinceName, response.data);
        }
      }),
      catchError(error => {
        this.loadingDistricts.set(provinceName, false);
        const errorResponse: ApiResponse<District[]> = {
          isSuccess: false,
          message: error.message || 'Failed to load districts',
          data: [],
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get tehsils by district name
   * GET /api/Location/tehsils/{districtName}
   */
  getTehsilsByDistrict(districtName: string): Observable<ApiResponse<Tehsil[]>> {
    if (!districtName) {
      return of({
        isSuccess: false,
        message: 'District name is required',
        data: [],
        errors: ['District name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    // Check if already loading
    if (this.loadingTehsils.get(districtName)) {
      const cached = this.tehsilsCache.get(districtName) || [];
      return of({
        isSuccess: true,
        message: 'Tehsils loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.tehsilsCache.has(districtName)) {
      const cachedData = this.tehsilsCache.get(districtName)!;
      return of({
        isSuccess: true,
        message: 'Tehsils retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingTehsils.set(districtName, true);
    const url = this.buildUrl('location.getTehsilsByDistrict', { districtName });
    
    return this.http.get<ApiResponse<Tehsil[]>>(url).pipe(
      tap(response => {
        this.loadingTehsils.set(districtName, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.tehsilsCache.set(districtName, response.data);
        }
      }),
      catchError(error => {
        this.loadingTehsils.set(districtName, false);
        const errorResponse: ApiResponse<Tehsil[]> = {
          isSuccess: false,
          message: error.message || 'Failed to load tehsils',
          data: [],
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get UCs by tehsil name
   * GET /api/Location/ucs/{tehsilName}
   */
  getUcsByTehsil(tehsilName: string): Observable<ApiResponse<Uc[]>> {
    if (!tehsilName) {
      return of({
        isSuccess: false,
        message: 'Tehsil name is required',
        data: [],
        errors: ['Tehsil name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    // Check if already loading
    if (this.loadingUcs.get(tehsilName)) {
      const cached = this.ucsCache.get(tehsilName) || [];
      return of({
        isSuccess: true,
        message: 'UCs loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.ucsCache.has(tehsilName)) {
      const cachedData = this.ucsCache.get(tehsilName)!;
      return of({
        isSuccess: true,
        message: 'UCs retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingUcs.set(tehsilName, true);
    const url = this.buildUrl('location.getUcsByTehsil', { tehsilName });
    
    return this.http.get<ApiResponse<Uc[]>>(url).pipe(
      tap(response => {
        this.loadingUcs.set(tehsilName, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.ucsCache.set(tehsilName, response.data);
        }
      }),
      catchError(error => {
        this.loadingUcs.set(tehsilName, false);
        const errorResponse: ApiResponse<Uc[]> = {
          isSuccess: false,
          message: error.message || 'Failed to load UCs',
          data: [],
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get full location hierarchy
   * GET /api/Location/hierarchy
   */
  getLocationHierarchy(): Observable<ApiResponse<LocationHierarchy>> {
    const url = this.buildUrl('location.getHierarchy');
    return this.http.get<ApiResponse<LocationHierarchy>>(url).pipe(
      tap(response => {
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.cacheHierarchyData(response.data);
        }
      }),
      catchError(error => {
        const errorResponse: ApiResponse<LocationHierarchy> = {
          isSuccess: false,
          message: error.message || 'Failed to load location hierarchy',
          data: { provinces: [] },
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Cache hierarchy data using names as keys
   */
  private cacheHierarchyData(hierarchy: LocationHierarchy): void {
    // Cache provinces
    this.provincesCache.next(
      hierarchy.provinces.map(p => ({
        id: p.id,
        provcode: p.provcode,
        province: p.province
      }))
    );

    // Cache districts, tehsils, and UCs using names as keys
    hierarchy.provinces.forEach(province => {
      const districts = province.districts.map(d => ({
        id: d.id,
        distcode: d.distcode,
        district: d.district,
        provcode: d.provcode
      }));
      // Use province name as key
      this.districtsCache.set(province.province, districts);

      province.districts.forEach(district => {
        const tehsils = district.tehsils.map(t => ({
          id: t.id,
          distcode: t.distcode,
          tehsilcode: t.tehsilcode,
          tehsil: t.tehsil
        }));
        // Use district name as key
        this.tehsilsCache.set(district.district, tehsils);

        district.tehsils.forEach(tehsil => {
          const ucs = tehsil.ucs.map(u => ({
            id: u.id,
            tehsilcode: u.tehsilcode,
            uccode: u.uccode,
            uctype: u.uctype,
            ucno: u.ucno,
            uc: u.uc
          }));
          // Use tehsil name as key
          this.ucsCache.set(tehsil.tehsil, ucs);
        });
      });
    });
  }

  /**
   * Get province by name
   * GET /api/Location/province/{provinceName}
   */
  getProvinceByName(provinceName: string): Observable<ApiResponse<Province>> {
    if (!provinceName) {
      return of({
        isSuccess: false,
        message: 'Province name is required',
        data: null as any,
        errors: ['Province name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    const url = this.buildUrl('location.getProvinceByName', { provinceName });
    return this.http.get<ApiResponse<Province>>(url).pipe(
      catchError(error => {
        const errorResponse: ApiResponse<Province> = {
          isSuccess: false,
          message: error.message || 'Failed to load province',
          data: null as any,
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get district by name
   * GET /api/Location/district/{districtName}
   */
  getDistrictByName(districtName: string): Observable<ApiResponse<District>> {
    if (!districtName) {
      return of({
        isSuccess: false,
        message: 'District name is required',
        data: null as any,
        errors: ['District name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    const url = this.buildUrl('location.getDistrictByName', { districtName });
    return this.http.get<ApiResponse<District>>(url).pipe(
      catchError(error => {
        const errorResponse: ApiResponse<District> = {
          isSuccess: false,
          message: error.message || 'Failed to load district',
          data: null as any,
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Get tehsil by name
   * GET /api/Location/tehsil/{tehsilName}
   */
  getTehsilByName(tehsilName: string): Observable<ApiResponse<Tehsil>> {
    if (!tehsilName) {
      return of({
        isSuccess: false,
        message: 'Tehsil name is required',
        data: null as any,
        errors: ['Tehsil name is required'],
        statusCode: ApiStatusCodes.BAD_REQUEST
      });
    }

    const url = this.buildUrl('location.getTehsilByName', { tehsilName });
    return this.http.get<ApiResponse<Tehsil>>(url).pipe(
      catchError(error => {
        const errorResponse: ApiResponse<Tehsil> = {
          isSuccess: false,
          message: error.message || 'Failed to load tehsil',
          data: null as any,
          errors: [error.message || 'An error occurred'],
          statusCode: error.status || ApiStatusCodes.INTERNAL_SERVER_ERROR
        };
        return of(errorResponse);
      })
    );
  }

  /**
   * Clear all caches
   */
  clearCache(): void {
    this.provincesCache.next([]);
    this.districtsCache.clear();
    this.tehsilsCache.clear();
    this.ucsCache.clear();
    this.loadingDistricts.clear();
    this.loadingTehsils.clear();
    this.loadingUcs.clear();
    this.loadingProvinces = false;
  }

  /**
   * Get cached provinces
   */
  getCachedProvinces(): Province[] {
    return this.provincesCache.getValue();
  }

  /**
   * Get cached districts for a province by name
   */
  getCachedDistricts(provinceName: string): District[] | null {
    return this.districtsCache.get(provinceName) || null;
  }

  /**
   * Get cached tehsils for a district by name
   */
  getCachedTehsils(districtName: string): Tehsil[] | null {
    return this.tehsilsCache.get(districtName) || null;
  }

  /**
   * Get cached UCs for a tehsil by name
   */
  getCachedUcs(tehsilName: string): Uc[] | null {
    return this.ucsCache.get(tehsilName) || null;
  }

  /**
   * Check if provinces are cached
   */
  hasCachedProvinces(): boolean {
    return this.provincesCache.getValue().length > 0;
  }

  /**
   * Check if districts are cached for a province by name
   */
  hasCachedDistricts(provinceName: string): boolean {
    return this.districtsCache.has(provinceName);
  }

  /**
   * Check if tehsils are cached for a district by name
   */
  hasCachedTehsils(districtName: string): boolean {
    return this.tehsilsCache.has(districtName);
  }

  /**
   * Check if UCs are cached for a tehsil by name
   */
  hasCachedUcs(tehsilName: string): boolean {
    return this.ucsCache.has(tehsilName);
  }
}