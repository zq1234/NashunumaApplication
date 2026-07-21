// src/app/core/models/food-stock.model.ts
export interface FoodStock {
  id: number | null;
  openingStockBoxesMamta: string | null;
  receivedStockBoxesMamta: string | null;
  distributedBoxesMamta: string | null;
  closingStockBoxesMamta: string | null;
  remarks: string | null;
  enteredBy: string | null;
  enteredOn: string | null;
  siteId: string | null;
  openingStockSachetsMamta: string | null;
  closingStockSachetsMamta: string | null;
  distributedSachetsMamta: string | null;
  openingStockSachetsWawa: string | null;
  closingStockSachetsWawa: string | null;
  distributedSachetsWawa: string | null;
  openingStockBoxesWawa: string | null;
  receivedStockBoxesWawa: string | null;
  distributedBoxesWawa: string | null;
  closingStockBoxesWawa: string | null;
  unit: string | null;
  rutfReceived: string | null;
  rutfOpening: string | null;
  rutfDistributed: string | null;
  rutfClosing: string | null;
  ifaReceived: string | null;
  ifaOpening: string | null;
  ifaDistributed: string | null;
  ifaClosing: string | null;
  activityTime: string | null;
  isManualUpdate: string | null;
  mmsReceived: string | null;
  mmsOpening: string | null;
  mmsDistributed: string | null;
  mmsClosing: string | null;
  // Site Location fields
  siteName?: string | null;
  address?: string | null;
  contact?: string | null;
  geoLocation?: string | null;
  province?: string | null;
  district?: string | null;
  tehsil?: string | null;
  headName?: string | null;
  isClosed?: string | null;
  isMobileSite?: string | null;
  provinceNew?: string | null;
  districtNew?: string | null;
  tehsilNew?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  // UI helper fields
  srNo?: number;
  wawa?: number;
  mamta?: number;
  rutf?: number;
  ifa?: string;
  dated?: string;
}

export interface CreateFoodStockDto {
  openingStockBoxesMamta?: string | null;
  receivedStockBoxesMamta?: string | null;
  distributedBoxesMamta?: string | null;
  closingStockBoxesMamta?: string | null;
  remarks?: string | null;
  enteredBy?: string | null;
  enteredOn?: string | null;
  siteId?: string | null;
  openingStockSachetsMamta?: string | null;
  closingStockSachetsMamta?: string | null;
  distributedSachetsMamta?: string | null;
  openingStockSachetsWawa?: string | null;
  closingStockSachetsWawa?: string | null;
  distributedSachetsWawa?: string | null;
  openingStockBoxesWawa?: string | null;
  receivedStockBoxesWawa?: string | null;
  distributedBoxesWawa?: string | null;
  closingStockBoxesWawa?: string | null;
  unit?: string | null;
  rutfReceived?: string | null;
  rutfOpening?: string | null;
  rutfDistributed?: string | null;
  rutfClosing?: string | null;
  ifaReceived?: string | null;
  ifaOpening?: string | null;
  ifaDistributed?: string | null;
  ifaClosing?: string | null;
  activityTime?: string | null;
  isManualUpdate?: string | null;
  mmsReceived?: string | null;
  mmsOpening?: string | null;
  mmsDistributed?: string | null;
  mmsClosing?: string | null;
}

export interface UpdateFoodStockDto extends CreateFoodStockDto {
  id: number;
}

export interface FoodStockFilter {
  searchTerm?: string;
  province?: string;
  district?: string;
  tehsil?: string;
  siteName?: string;
  siteId?: string;
  isActive?: number;
  isMobileSite?: string;
  isClosed?: string;
  isManualUpdate?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface SiteSummary {
  totalSites: number;
  notUpdated: number;
  lowStockSites: number;
}