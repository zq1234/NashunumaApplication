// src/app/core/models/site-location/site-location.model.ts

export interface SiteLocation {
  id: number;
  siteName: string | null;
  address: string | null;
  contact: string | null;
  geoLocation: string | null;
  headName: string | null;
  siteTarget: number | null;
  latitude: number | null;
  longitude: number | null;
  isActive: boolean;
  isMobileSite: string | null;
  isClosed: string | null;

  provinceId: number | null;
  province: string | null;
  districtId: number | null;
  district: string | null;
  tehsilId: number | null;
  tehsil: string | null;
  ucId: number | null;
  uc: string | null;

  createdBy: string | null;
  createdOn: string | null;
  updatedBy: string | null;
  updatedOn: string | null;
}

export interface CreateSiteLocationRequest {
  siteName: string;
  provinceId: number;
  districtId: number;
  tehsilId: number;
  ucId?: number | null;
  address?: string | null;
  contact?: string | null;
  geoLocation?: string | null;
  headName?: string | null;
  siteTarget?: number | null;
  latitude?: number | null;
  longitude?: number | null;
  isMobileSite?: string | null;
  isClosed?: string | null;
}

export interface UpdateSiteLocationRequest extends CreateSiteLocationRequest {
  isActive: boolean;
}

export interface SiteLocationFilter {
  search?: string | null;
  provinceId?: number | null;
  districtId?: number | null;
  tehsilId?: number | null;
  ucId?: number | null;
  isActive?: boolean | null;
  isMobileSite?: boolean | null;
  isClosed?: boolean | null;
  sortBy?: string | null;
  sortDescending?: boolean | null;
}

export interface ProvinceLookup { id: number; province: string; }
export interface DistrictLookup { id: number; district: string; provinceId: number | null; }
export interface TehsilLookup   { id: number; tehsil: string;   districtId: number | null; }
export interface UcLookup       { id: number; uc: string;       tehsilId: number | null; }

export interface SiteLocationSummary {
  totalSites: number;
  activeSites: number;
  inactiveSites: number;
  mobileSites: number;
  closedSites: number;
  sitesByProvince: { province: string; count: number }[];
  sitesByDistrict: { district: string; count: number }[];
}