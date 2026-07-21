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

  // Cache for location data
  private provincesCache = new BehaviorSubject<Province[]>([]);
  private districtsCache = new Map<number, District[]>();
  private tehsilsCache = new Map<number, Tehsil[]>();
  private ucsCache = new Map<number, Uc[]>();
  
  // Loading states
  private loadingProvinces = false;
  private loadingDistricts = new Map<number, boolean>();
  private loadingTehsils = new Map<number, boolean>();
  private loadingUcs = new Map<number, boolean>();

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
   * Get districts by province code
   */
  getDistrictsByProvince(provinceCode: number): Observable<ApiResponse<District[]>> {
    // Check if already loading
    if (this.loadingDistricts.get(provinceCode)) {
      const cached = this.districtsCache.get(provinceCode) || [];
      return of({
        isSuccess: true,
        message: 'Districts loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.districtsCache.has(provinceCode)) {
      const cachedData = this.districtsCache.get(provinceCode)!;
      return of({
        isSuccess: true,
        message: 'Districts retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingDistricts.set(provinceCode, true);
    const url = this.buildUrl('location.getDistrictsByProvince', { provinceCode });
    
    return this.http.get<ApiResponse<District[]>>(url).pipe(
      tap(response => {
        this.loadingDistricts.set(provinceCode, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.districtsCache.set(provinceCode, response.data);
        }
      }),
      catchError(error => {
        this.loadingDistricts.set(provinceCode, false);
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
   * Get tehsils by district code
   */
  getTehsilsByDistrict(districtCode: number): Observable<ApiResponse<Tehsil[]>> {
    // Check if already loading
    if (this.loadingTehsils.get(districtCode)) {
      const cached = this.tehsilsCache.get(districtCode) || [];
      return of({
        isSuccess: true,
        message: 'Tehsils loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.tehsilsCache.has(districtCode)) {
      const cachedData = this.tehsilsCache.get(districtCode)!;
      return of({
        isSuccess: true,
        message: 'Tehsils retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingTehsils.set(districtCode, true);
    const url = this.buildUrl('location.getTehsilsByDistrict', { districtCode });
    
    return this.http.get<ApiResponse<Tehsil[]>>(url).pipe(
      tap(response => {
        this.loadingTehsils.set(districtCode, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.tehsilsCache.set(districtCode, response.data);
        }
      }),
      catchError(error => {
        this.loadingTehsils.set(districtCode, false);
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
   * Get UCs by tehsil code
   */
  getUcsByTehsil(tehsilCode: number): Observable<ApiResponse<Uc[]>> {
    // Check if already loading
    if (this.loadingUcs.get(tehsilCode)) {
      const cached = this.ucsCache.get(tehsilCode) || [];
      return of({
        isSuccess: true,
        message: 'UCs loading...',
        data: cached,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    // Check cache
    if (this.ucsCache.has(tehsilCode)) {
      const cachedData = this.ucsCache.get(tehsilCode)!;
      return of({
        isSuccess: true,
        message: 'UCs retrieved from cache',
        data: cachedData,
        errors: null,
        statusCode: ApiStatusCodes.OK
      });
    }

    this.loadingUcs.set(tehsilCode, true);
    const url = this.buildUrl('location.getUcsByTehsil', { tehsilCode });
    
    return this.http.get<ApiResponse<Uc[]>>(url).pipe(
      tap(response => {
        this.loadingUcs.set(tehsilCode, false);
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.ucsCache.set(tehsilCode, response.data);
        }
      }),
      catchError(error => {
        this.loadingUcs.set(tehsilCode, false);
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
   * Cache hierarchy data
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

    // Cache districts, tehsils, and UCs
    hierarchy.provinces.forEach(province => {
      const districts = province.districts.map(d => ({
        id: d.id,
        distcode: d.distcode,
        district: d.district,
        provcode: d.provcode
      }));
      this.districtsCache.set(province.provcode, districts);

      province.districts.forEach(district => {
        const tehsils = district.tehsils.map(t => ({
          id: t.id,
          distcode: t.distcode,
          tehsilcode: t.tehsilcode,
          tehsil: t.tehsil
        }));
        this.tehsilsCache.set(district.distcode, tehsils);

        district.tehsils.forEach(tehsil => {
          const ucs = tehsil.ucs.map(u => ({
            id: u.id,
            tehsilcode: u.tehsilcode,
            uccode: u.uccode,
            uctype: u.uctype,
            ucno: u.ucno,
            uc: u.uc
          }));
          this.ucsCache.set(tehsil.tehsilcode, ucs);
        });
      });
    });
  }

  /**
   * Get province by code
   */
  getProvinceByCode(provinceCode: number): Observable<ApiResponse<Province>> {
    const url = this.buildUrl('location.getProvinceByCode', { provinceCode });
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
   * Get district by code
   */
  getDistrictByCode(districtCode: number): Observable<ApiResponse<District>> {
    const url = this.buildUrl('location.getDistrictByCode', { districtCode });
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
   * Get tehsil by code
   */
  getTehsilByCode(tehsilCode: number): Observable<ApiResponse<Tehsil>> {
    const url = this.buildUrl('location.getTehsilByCode', { tehsilCode });
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
   * Get cached districts for a province
   */
  getCachedDistricts(provinceCode: number): District[] | null {
    return this.districtsCache.get(provinceCode) || null;
  }

  /**
   * Get cached tehsils for a district
   */
  getCachedTehsils(districtCode: number): Tehsil[] | null {
    return this.tehsilsCache.get(districtCode) || null;
  }

  /**
   * Get cached UCs for a tehsil
   */
  getCachedUcs(tehsilCode: number): Uc[] | null {
    return this.ucsCache.get(tehsilCode) || null;
  }

  /**
   * Check if provinces are cached
   */
  hasCachedProvinces(): boolean {
    return this.provincesCache.getValue().length > 0;
  }

  /**
   * Check if districts are cached for a province
   */
  hasCachedDistricts(provinceCode: number): boolean {
    return this.districtsCache.has(provinceCode);
  }

  /**
   * Check if tehsils are cached for a district
   */
  hasCachedTehsils(districtCode: number): boolean {
    return this.tehsilsCache.has(districtCode);
  }

  /**
   * Check if UCs are cached for a tehsil
   */
  hasCachedUcs(tehsilCode: number): boolean {
    return this.ucsCache.has(tehsilCode);
  }
}