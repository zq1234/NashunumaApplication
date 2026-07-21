// src/app/core/models/user.model.ts

/**
 * User DTO - matches the API response structure
 */
export interface UserDto {
  userId: string;
  username: string;
  personName: string;
  email: string;
  mobilenumber: string;
  designation: string;
  usertype: string;
  province: string;
  district: string;
  tehsil: string;
  siteId: number | null;
  siteName: string;
  isactive: string;
  isadmin: string;
  lastlogindatetime: string | null;
  imeino: string | null;
  macaddress: string | null;
  requestdatetime: string | null;
  activedatetime: string | null;
  activedby: string | null;
  changeType: string | null;
  istransferred: number | null;
  // Site location fields
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
 * User Filter Parameters for paginated queries
 */
export interface UserFilterParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  province?: string;
  district?: string;
  tehsil?: string;
  userType?: string;
  isActive?: boolean;
}

/**
 * User Search Parameters with sorting
 */
export interface UserSearchParams extends UserFilterParams {
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

/**
 * User Status Types
 */
export type UserStatus = 'active' | 'inactive' | 'blocked';

/**
 * User Type Constants
 */
export const UserTypes = {
  ADMIN: 'Admin',
  USER: 'User',
  MANAGER: 'Manager',
  COORDINATOR: 'Coordinator'
} as const;

export type UserType = typeof UserTypes[keyof typeof UserTypes];

/**
 * User Statistics Interface for computed statistics
 */
export interface UserStatistics {
  total: number;
  active: number;
  inactive: number;
  blocked: number;
  byRole: {
    [key: string]: number;
  };
  byStatus: {
    active: number;
    inactive: number;
    blocked: number;
  };
  newThisMonth: number;
  lastUpdated: Date;
}

/**
 * User Export Options
 */
export interface UserExportOptions {
  format: 'excel' | 'csv' | 'pdf';
  includeFields?: string[];
  filters?: UserSearchParams;
}

// ============================================
// Helper Functions for UserDto
// ============================================

export class UserHelper {
  /**
   * Check if user is active
   */
  static isActive(user: UserDto): boolean {
    return user.isactive === '1' || user.isactive === 'true';
  }

  /**
   * Check if user is admin
   */
  static isAdmin(user: UserDto): boolean {
    return user.isadmin === '1' || user.isadmin === 'true';
  }

  /**
   * Check if user is blocked
   */
  static isBlocked(user: UserDto): boolean {
    return user.isactive === '0' || user.isactive === 'false';
  }

  /**
   * Get user's full name
   */
  static getFullName(user: UserDto): string {
    return user.personName || user.username || 'Unknown User';
  }

  /**
   * Get user's display name
   */
  static getDisplayName(user: UserDto): string {
    return user.personName || user.username || 'Unknown User';
  }

  /**
   * Get user's status text
   */
  static getStatusText(user: UserDto): string {
    return this.isActive(user) ? 'Active' : 'Inactive';
  }

  /**
   * Get user's status class for styling
   */
  static getStatusClass(user: UserDto): string {
    return this.isActive(user) ? 'status-active' : 'status-inactive';
  }

  /**
   * Get user's role display name
   */
  static getRoleDisplayName(user: UserDto): string {
    return user.usertype || 'User';
  }

  /**
   * Check if user has a specific role
   */
  static hasRole(user: UserDto, role: string): boolean {
    return user.usertype?.toLowerCase() === role.toLowerCase();
  }

  /**
   * Get user's location as a formatted string
   */
  static getLocation(user: UserDto): string {
    const parts = [user.province, user.district, user.tehsil].filter(Boolean);
    return parts.join(', ') || 'No location set';
  }

  /**
   * Get user's site information
   */
  static getSiteInfo(user: UserDto): string {
    return user.siteName || 'No site assigned';
  }

  /**
   * Check if user has a mobile site
   */
  static isMobileSite(user: UserDto): boolean {
    return user.siteIsMobileSite === 1;
  }

  /**
   * Check if user's site is closed
   */
  static isSiteClosed(user: UserDto): boolean {
    return user.siteIsClosed === 1;
  }

  /**
   * Get user's last login date formatted
   */
  static getLastLogin(user: UserDto): string {
    if (!user.lastlogindatetime) return 'Never';
    try {
      const date = new Date(user.lastlogindatetime);
      return date.toLocaleDateString('en-PK', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return user.lastlogindatetime;
    }
  }

  /**
   * Get user's request date formatted
   */
  static getRequestDate(user: UserDto): string {
    if (!user.requestdatetime) return 'Never';
    try {
      const date = new Date(user.requestdatetime);
      return date.toLocaleDateString('en-PK', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
      });
    } catch {
      return user.requestdatetime;
    }
  }

  /**
   * Convert UserDto to a simplified user object for display
   */
  static toDisplayUser(user: UserDto): {
    id: string;
    name: string;
    email: string;
    role: string;
    status: string;
    location: string;
    site: string;
  } {
    return {
      id: user.userId,
      name: this.getDisplayName(user),
      email: user.email,
      role: this.getRoleDisplayName(user),
      status: this.getStatusText(user),
      location: this.getLocation(user),
      site: this.getSiteInfo(user)
    };
  }

  /**
   * Get user's avatar initials
   */
  static getInitials(user: UserDto): string {
    const name = this.getDisplayName(user);
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  /**
   * Get user's avatar color based on name
   */
  static getAvatarColor(user: UserDto): string {
    const colors = [
      '#4F46E5', '#7C3AED', '#2563EB', '#059669',
      '#D97706', '#DC2626', '#7F8C8D', '#2C3E50'
    ];
    const name = this.getDisplayName(user);
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  /**
   * Get user's avatar URL
   */
  static getAvatarUrl(user: UserDto): string {
    const name = this.getDisplayName(user);
    const color = this.getAvatarColor(user).replace('#', '');
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=${color}&color=fff&size=128`;
  }

  /**
   * Get user's full address including site
   */
  static getFullAddress(user: UserDto): string {
    const parts = [
      user.siteAddress,
      user.siteProvince,
      user.siteDistrict,
      user.siteTehsil
    ].filter(Boolean);
    return parts.join(', ') || 'No address available';
  }

  /**
   * Check if user has a valid site location
   */
  static hasValidSiteLocation(user: UserDto): boolean {
    return !!(user.siteProvince && user.siteDistrict && user.siteTehsil);
  }

  /**
   * Get user's transfer status
   */
  static getTransferStatus(user: UserDto): string {
    if (user.istransferred === 1) return 'Transferred';
    if (user.istransferred === 0) return 'Not Transferred';
    return 'Unknown';
  }

  /**
   * Create a default/empty UserDto
   */
  static createEmpty(): UserDto {
    return {
      userId: '',
      username: '',
      personName: '',
      email: '',
      mobilenumber: '',
      designation: '',
      usertype: '',
      province: '',
      district: '',
      tehsil: '',
      siteId: null,
      siteName: '',
      isactive: '1',
      isadmin: '0',
      lastlogindatetime: null,
      imeino: null,
      macaddress: null,
      requestdatetime: null,
      activedatetime: null,
      activedby: null,
      changeType: null,
      istransferred: null,
      siteAddress: null,
      siteContact: null,
      siteGeoLocation: null,
      siteProvince: null,
      siteDistrict: null,
      siteTehsil: null,
      siteHeadName: null,
      siteIsClosed: null,
      siteIsMobileSite: null,
      siteProvinceNew: null,
      siteDistrictNew: null,
      siteTehsilNew: null,
      siteLatitude: null,
      siteLongitude: null
    };
  }

  /**
   * Create default UserFilterParams
   */
  static createDefaultFilters(): UserFilterParams {
    return {
      pageNumber: 1,
      pageSize: 10,
      searchTerm: '',
      province: '',
      district: '',
      tehsil: '',
      userType: '',
      isActive: undefined
    };
  }
}