// src/app/core/models/user-location.model.ts

import { UserDto } from './user.model';

/**
 * Update User Location DTO
 * Contains both province/district/tehsil names and their IDs
 */
export interface UpdateUserLocationDto {
  province: string;
  provinceId: number | null;
  district: string;
  districtId: number | null;
  tehsil: string;
  tehsilId: number | null;
  siteId: number | null;
  siteName: string;
}

/**
 * Location Statistics DTO
 * Contains aggregated location-based user statistics
 */
export interface LocationStatsDto {
  provinceStats: { [key: string]: number };
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  // Optional detailed breakdowns
  provinces?: {
    name: string;
    count: number;
    percentage: number;
  }[];
  districts?: {
    name: string;
    province: string;
    count: number;
  }[];
  tehsils?: {
    name: string;
    district: string;
    count: number;
  }[];
}

/**
 * User Summary Statistics
 * Contains overall user statistics for dashboard
 */
export interface UserSummaryStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  blockedUsers: number;
  newUsersThisMonth: number;
  usersByType: {
    userType: string;
    count: number;
  }[];
  usersByProvince: {
    province: string;
    count: number;
  }[];
}

/**
 * Location details for a user
 */
export interface UserLocation {
  province: string;
  provinceId: number | null;
  district: string;
  districtId: number | null;
  tehsil: string;
  tehsilId: number | null;
  siteId: number | null;
  siteName: string;
  siteAddress: string | null;
  siteContact: string | null;
  siteGeoLocation: string | null;
  siteProvince: string | null;
  siteDistrict: string | null;
  siteTehsil: string | null;
  siteHeadName: string | null;
  siteIsClosed: number | null;
  siteIsMobileSite: number | null;
  siteProvinceNew: string | null;
  siteDistrictNew: string | null;
  siteTehsilNew: string | null;
  siteLatitude: string | null;
  siteLongitude: string | null;
}

/**
 * Location filter parameters
 */
export interface LocationFilterParams {
  province?: string;
  district?: string;
  tehsil?: string;
  siteId?: number;
  isMobileSite?: boolean;
  isClosed?: boolean;
}

// ============================================
// Helper Functions for Location
// ============================================

export class LocationHelper {
  /**
   * Extract location from UserDto
   */
  static fromUser(user: UserDto): UserLocation {
    return {
      province: user.province,
      provinceId: null,
      district: user.district,
      districtId: null,
      tehsil: user.tehsil,
      tehsilId: null,
      siteId: user.siteId,
      siteName: user.siteName,
      siteAddress: user.siteAddress,
      siteContact: user.siteContact,
      siteGeoLocation: user.siteGeoLocation,
      siteProvince: user.siteProvince,
      siteDistrict: user.siteDistrict,
      siteTehsil: user.siteTehsil,
      siteHeadName: user.siteHeadName,
      siteIsClosed: user.siteIsClosed,
      siteIsMobileSite: user.siteIsMobileSite,
      siteProvinceNew: user.siteProvinceNew,
      siteDistrictNew: user.siteDistrictNew,
      siteTehsilNew: user.siteTehsilNew,
      siteLatitude: user.siteLatitude,
      siteLongitude: user.siteLongitude
    };
  }

  /**
   * Get full location string
   */
  static getFullLocation(location: UserLocation): string {
    const parts = [location.province, location.district, location.tehsil].filter(Boolean);
    return parts.join(', ') || 'No location set';
  }

  /**
   * Get full site address
   */
  static getSiteAddress(location: UserLocation): string {
    const parts = [
      location.siteAddress,
      location.siteProvince,
      location.siteDistrict,
      location.siteTehsil
    ].filter(Boolean);
    return parts.join(', ') || 'No address available';
  }

  /**
   * Check if location has complete details
   */
  static isComplete(location: UserLocation): boolean {
    return !!(location.province && location.district && location.tehsil);
  }

  /**
   * Check if site is mobile
   */
  static isMobileSite(location: UserLocation): boolean {
    return location.siteIsMobileSite === 1;
  }

  /**
   * Check if site is closed
   */
  static isClosed(location: UserLocation): boolean {
    return location.siteIsClosed === 1;
  }

  /**
   * Create default UpdateUserLocationDto
   */
  static createDefaultUpdateDto(): UpdateUserLocationDto {
    return {
      province: '',
      provinceId: null,
      district: '',
      districtId: null,
      tehsil: '',
      tehsilId: null,
      siteId: null,
      siteName: ''
    };
  }

  /**
   * Convert UserDto to UpdateUserLocationDto
   */
  static toUpdateDto(user: UserDto): UpdateUserLocationDto {
    return {
      province: user.province,
      provinceId: null,
      district: user.district,
      districtId: null,
      tehsil: user.tehsil,
      tehsilId: null,
      siteId: user.siteId,
      siteName: user.siteName
    };
  }

  /**
   * Check if location has changed between two locations
   */
  static hasChanged(location1: UserLocation, location2: UserLocation): boolean {
    return location1.province !== location2.province ||
           location1.district !== location2.district ||
           location1.tehsil !== location2.tehsil ||
           location1.siteId !== location2.siteId;
  }
}