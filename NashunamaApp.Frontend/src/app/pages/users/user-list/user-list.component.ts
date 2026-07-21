// src/app/pages/users/user-list/user-list.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, forkJoin } from 'rxjs';
import { UserManagementService } from '@shared/services/user-management.service';
import { LocationService, Province, District, Tehsil } from '@shared/services/location.service';
import { UserDto, UserFilterParams } from '@core/models/user.model';
import { UpdateUserLocationDto } from '@core/models/user-location.model';
import { ApiResponse, PaginatedResponse, ApiResponseHelper } from '@core/models/api-response.model';
import { DataTableComponent, DataTableColumn } from '@shared/components/data-table/data-table.component';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DataTableComponent,
    ModalComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  // Data
  users: UserDto[] = [];
  totalItems = 0;
  
  // UI State
  loading = false;
  error: string | null = null;
  success: string | null = null;
  loadingLocations = false;
  
  // Filters
  searchTerm = '';
  selectedProvinceCode: number | null = null;
  selectedDistrictCode: number | null = null;
  selectedTehsilCode: number | null = null;
  userType = '';
  isActive: boolean | null = null;
  
  // Dropdown options
  provinces: Province[] = [];
  districts: District[] = [];
  tehsils: Tehsil[] = [];
  userTypes: string[] = ['Admin', 'User', 'Manager', 'Coordinator', 'Supervisor'];
  
  // Display values for filters
  provinceDisplay = '';
  districtDisplay = '';
  tehsilDisplay = '';
  
  // Pagination
  pageNumber = 1;
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50, 100];
  
  // Search debounce
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  
  // Modal states
  isProfileModalOpen = false;
  isTransferModalOpen = false;
  isConfirmModalOpen = false;
  selectedUser: UserDto | null = null;
  
  // Transfer form
  transferForm: UpdateUserLocationDto = {
    province: '',
    provinceId: null,
    district: '',
    districtId: null,
    tehsil: '',
    tehsilId: null,
    siteId: null,
    siteName: ''
  };
  
  // Confirm modal data
  confirmModalData = {
    title: '',
    message: '',
    confirmText: 'Confirm',
    isDanger: false,
    action: '' as 'toggle' | 'block' | 'delete' | null
  };
  
  // Column Definitions for DataTableComponent
  columns: DataTableColumn[] = [
    { field: 'srNo', header: 'S.No', sortable: false, width: '60px', type: 'number' },
    { field: 'username', header: 'Username', sortable: true, width: '120px' },
    { field: 'personName', header: 'Full Name', sortable: true, width: '150px' },
    { field: 'email', header: 'Email', sortable: true, width: '200px' },
    { field: 'mobilenumber', header: 'Mobile', sortable: true, width: '120px' },
    { field: 'designation', header: 'Designation', sortable: true, width: '130px' },
    { field: 'province', header: 'Province', sortable: true, width: '120px' },
    { field: 'district', header: 'District', sortable: true, width: '120px' },
    { field: 'tehsil', header: 'Tehsil', sortable: true, width: '120px' },
    { field: 'siteName', header: 'Site', sortable: true, width: '150px' },
    { field: 'usertype', header: 'Type', sortable: true, width: '100px', type: 'badge' },
    { field: 'isactive', header: 'Status', sortable: true, width: '100px', type: 'status' }
  ];

  constructor(
    private userService: UserManagementService,
    private locationService: LocationService
  ) {}

  ngOnInit(): void {
    this.setupSearchDebounce();
    this.loadLocationData();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.pageNumber = 1;
      this.loadUsers();
    });
  }

  /**
   * Load location data (provinces, districts, tehsils)
   */
  loadLocationData(): void {
    this.loadingLocations = true;
    
    // Try to get cached data first
    const cachedProvinces = this.locationService.getCachedProvinces();
    if (cachedProvinces && cachedProvinces.length > 0) {
      this.provinces = cachedProvinces;
      this.loadingLocations = false;
      return;
    }

    // Load from API
    this.locationService.getProvinces().subscribe({
      next: (response: ApiResponse<Province[]>) => {
        this.loadingLocations = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.provinces = response.data;
        } else {
          console.error('Failed to load provinces:', response.message);
          this.setDefaultFilterOptions();
        }
      },
      error: (error) => {
        this.loadingLocations = false;
        console.error('Error loading provinces:', error);
        this.setDefaultFilterOptions();
      }
    });
  }

  /**
   * Set default filter options as fallback
   */
  private setDefaultFilterOptions(): void {
    // Create default province objects
    this.provinces = [
      { id: 1, provcode: 1, province: 'Punjab' },
      { id: 2, provcode: 2, province: 'Sindh' },
      { id: 3, provcode: 3, province: 'KPK' },
      { id: 4, provcode: 4, province: 'Balochistan' },
      { id: 5, provcode: 5, province: 'Islamabad' },
      { id: 6, provcode: 6, province: 'Gilgit-Baltistan' },
      { id: 7, provcode: 7, province: 'AJK' }
    ];
  }

  /**
   * Handle province change - load districts
   */
  onProvinceChange(provinceCode: number | null): void {
    this.selectedProvinceCode = provinceCode;
    this.selectedDistrictCode = null;
    this.selectedTehsilCode = null;
    this.districts = [];
    this.tehsils = [];
    this.districtDisplay = '';
    this.tehsilDisplay = '';

    if (provinceCode) {
      // Get province name for display
      const province = this.provinces.find(p => p.provcode === provinceCode);
      this.provinceDisplay = province?.province || '';

      // Check cache first
      const cachedDistricts = this.locationService.getCachedDistricts(provinceCode);
      if (cachedDistricts && cachedDistricts.length > 0) {
        this.districts = cachedDistricts;
      } else {
        // Load from API
        this.locationService.getDistrictsByProvince(provinceCode).subscribe({
          next: (response: ApiResponse<District[]>) => {
            if (ApiResponseHelper.isSuccess(response) && response.data) {
              this.districts = response.data;
            } else {
              console.error('Failed to load districts:', response.message);
            }
          },
          error: (error) => {
            console.error('Error loading districts:', error);
          }
        });
      }
    }
    
    this.pageNumber = 1;
    this.loadUsers();
  }

  /**
   * Handle district change - load tehsils
   */
  onDistrictChange(districtCode: number | null): void {
    this.selectedDistrictCode = districtCode;
    this.selectedTehsilCode = null;
    this.tehsils = [];
    this.tehsilDisplay = '';

    if (districtCode) {
      // Get district name for display
      const district = this.districts.find(d => d.distcode === districtCode);
      this.districtDisplay = district?.district || '';

      // Check cache first
      const cachedTehsils = this.locationService.getCachedTehsils(districtCode);
      if (cachedTehsils && cachedTehsils.length > 0) {
        this.tehsils = cachedTehsils;
      } else {
        // Load from API
        this.locationService.getTehsilsByDistrict(districtCode).subscribe({
          next: (response: ApiResponse<Tehsil[]>) => {
            if (ApiResponseHelper.isSuccess(response) && response.data) {
              this.tehsils = response.data;
            } else {
              console.error('Failed to load tehsils:', response.message);
            }
          },
          error: (error) => {
            console.error('Error loading tehsils:', error);
          }
        });
      }
    }
    
    this.pageNumber = 1;
    this.loadUsers();
  }

  /**
   * Handle tehsil change
   */
  onTehsilChange(tehsilCode: number | null): void {
    this.selectedTehsilCode = tehsilCode;

    if (tehsilCode) {
      const tehsil = this.tehsils.find(t => t.tehsilcode === tehsilCode);
      this.tehsilDisplay = tehsil?.tehsil || '';
    } else {
      this.tehsilDisplay = '';
    }
    
    this.pageNumber = 1;
    this.loadUsers();
  }

  /**
   * Load users with current filters
   */
  loadUsers(): void {
    this.error = null;
    this.loading = true;

    const filters: UserFilterParams = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm || undefined,
      province: this.provinceDisplay || undefined,
      district: this.districtDisplay || undefined,
      tehsil: this.tehsilDisplay || undefined,
      userType: this.userType || undefined,
      isActive: this.isActive !== null ? this.isActive : undefined
    };

    this.userService.getPagedUsers(filters).subscribe({
      next: (response: ApiResponse<PaginatedResponse<UserDto>>) => {
        this.loading = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.users = response.data.items.map((item, index) => ({
            ...item,
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1
          }));
          this.totalItems = response.data.totalCount;
        } else {
          this.error = response.message || 'Failed to load users';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred while loading users';
        console.error('Error loading users:', error);
      }
    });
  }

  /**
   * Load users by location
   */
  loadUsersByLocation(): void {
    this.error = null;
    this.loading = true;

    this.userService.getUsersByLocation(
      this.provinceDisplay || undefined,
      this.districtDisplay || undefined,
      this.tehsilDisplay || undefined,
      this.pageNumber,
      this.pageSize
    ).subscribe({
      next: (response: ApiResponse<PaginatedResponse<UserDto>>) => {
        this.loading = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.users = response.data.items.map((item, index) => ({
            ...item,
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1
          }));
          this.totalItems = response.data.totalCount;
        } else {
          this.error = response.message || 'Failed to load users by location';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred while loading users by location';
        console.error('Error loading users by location:', error);
      }
    });
  }

  /**
   * Search users
   */
  searchUsers(): void {
    if (!this.searchTerm || this.searchTerm.length < 2) {
      this.loadUsers();
      return;
    }

    this.error = null;
    this.loading = true;

    this.userService.searchUsers(
      this.searchTerm,
      this.pageNumber,
      this.pageSize
    ).subscribe({
      next: (response: ApiResponse<PaginatedResponse<UserDto>>) => {
        this.loading = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.users = response.data.items.map((item, index) => ({
            ...item,
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1
          }));
          this.totalItems = response.data.totalCount;
        } else {
          this.error = response.message || 'No users found';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred while searching';
        console.error('Error searching users:', error);
      }
    });
  }

  /**
   * Export users to Excel
   */
  exportUsers(): void {
    this.loading = true;
    const filters = {
      searchTerm: this.searchTerm || undefined,
      province: this.provinceDisplay || undefined,
      district: this.districtDisplay || undefined,
      tehsil: this.tehsilDisplay || undefined,
      userType: this.userType || undefined,
      isActive: this.isActive !== null ? this.isActive : undefined
    };

    this.userService.exportUsers(filters).subscribe({
      next: (blob: Blob) => {
        this.loading = false;
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `users_export_${new Date().toISOString().split('T')[0]}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.showSuccess('Users exported successfully');
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'Failed to export users';
        console.error('Error exporting users:', error);
      }
    });
  }

  /**
   * Load users by role
   */
  loadUsersByRole(role: string): void {
    this.error = null;
    this.loading = true;

    this.userService.getUsersByRole(role, this.pageNumber, this.pageSize).subscribe({
      next: (response: ApiResponse<PaginatedResponse<UserDto>>) => {
        this.loading = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.users = response.data.items.map((item, index) => ({
            ...item,
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1
          }));
          this.totalItems = response.data.totalCount;
          this.userType = role;
        } else {
          this.error = response.message || 'Failed to load users by role';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred while loading users by role';
        console.error('Error loading users by role:', error);
      }
    });
  }

  // Helper methods for stats
  getActiveCount(): number {
    return this.users.filter(user => user.isactive === '1' || user.isactive === 'true').length;
  }

  getInactiveCount(): number {
    return this.users.filter(user => user.isactive !== '1' && user.isactive !== 'true').length;
  }

  getAdminCount(): number {
    return this.users.filter(user => user.usertype?.toLowerCase() === 'admin').length;
  }

  // Handle view from datatable
  onView(user: UserDto): void {
    this.viewProfile(user);
  }

  // Handle edit from datatable
  onEdit(user: UserDto): void {
    this.transferLocation(user);
  }

  // Handle delete from datatable
  onDelete(user: UserDto): void {
    this.blockUser(user);
  }

  onSearchChange(term: string): void {
    if (term.length >= 2) {
      this.searchUsers();
    } else if (term.length === 0) {
      this.loadUsers();
    }
    this.searchSubject.next(term);
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadUsers();
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.pageNumber = event.page;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.selectedProvinceCode = null;
    this.selectedDistrictCode = null;
    this.selectedTehsilCode = null;
    this.provinceDisplay = '';
    this.districtDisplay = '';
    this.tehsilDisplay = '';
    this.userType = '';
    this.isActive = null;
    this.districts = [];
    this.tehsils = [];
    this.pageNumber = 1;
    this.searchSubject.next('');
    this.loadUsers();
  }

  // Profile Modal
  viewProfile(user: UserDto): void {
    this.selectedUser = user;
    this.isProfileModalOpen = true;
  }

  closeProfileModal(): void {
    this.isProfileModalOpen = false;
    this.selectedUser = null;
  }

  // Transfer Location Modal
  transferLocation(user: UserDto): void {
    this.selectedUser = user;
    // Pre-fill form with current values
    this.transferForm = {
      province: user.province || '',
      provinceId: null,
      district: user.district || '',
      districtId: null,
      tehsil: user.tehsil || '',
      tehsilId: null,
      siteId: user.siteId || null,
      siteName: user.siteName || ''
    };
    this.isTransferModalOpen = true;
  }

  closeTransferModal(): void {
    this.isTransferModalOpen = false;
    this.selectedUser = null;
  }

  handleTransferSubmit(): void {
    if (!this.selectedUser) return;
    
    this.userService.transferUserLocation(this.selectedUser.username, this.transferForm).subscribe({
      next: (response: ApiResponse<UserDto>) => {
        if (ApiResponseHelper.isSuccess(response)) {
          this.showSuccess(response.message || 'Location transferred successfully');
          this.closeTransferModal();
          this.loadUsers();
        } else {
          this.showError(response.message || 'Failed to transfer location');
        }
      },
      error: (error) => {
        this.showError('Error transferring location');
        console.error(error);
      }
    });
  }

  // Toggle User Status
  toggleUserStatus(user: UserDto): void {
    const isActive = user.isactive === '1' || user.isactive === 'true';
    const action = isActive ? 'deactivate' : 'activate';
    
    this.selectedUser = user;
    this.confirmModalData = {
      title: `${action.charAt(0).toUpperCase() + action.slice(1)} User`,
      message: `Are you sure you want to ${action} user "${user.personName}"?`,
      confirmText: `Yes, ${action}`,
      isDanger: false,
      action: 'toggle'
    };
    this.isConfirmModalOpen = true;
  }

  // Block User
  blockUser(user: UserDto): void {
    this.selectedUser = user;
    this.confirmModalData = {
      title: 'Block User',
      message: `Are you sure you want to block user "${user.personName}"? This action cannot be undone.`,
      confirmText: 'Yes, Block',
      isDanger: true,
      action: 'block'
    };
    this.isConfirmModalOpen = true;
  }

  // Delete User (using block as delete)
  deleteUser(user: UserDto): void {
    this.selectedUser = user;
    this.confirmModalData = {
      title: 'Delete User',
      message: `Are you sure you want to delete user "${user.personName}"? This action cannot be undone.`,
      confirmText: 'Yes, Delete',
      isDanger: true,
      action: 'delete'
    };
    this.isConfirmModalOpen = true;
  }

  closeConfirmModal(): void {
    this.isConfirmModalOpen = false;
    this.selectedUser = null;
  }

  handleConfirmAction(): void {
    if (!this.selectedUser) return;

    if (this.confirmModalData.action === 'toggle') {
      const isActive = this.selectedUser.isactive === '1' || this.selectedUser.isactive === 'true';
      this.userService.toggleUserStatus(this.selectedUser.username, !isActive).subscribe({
        next: (response: ApiResponse<boolean>) => {
          if (ApiResponseHelper.isSuccess(response)) {
            this.showSuccess(response.message || 'User status updated successfully');
            this.closeConfirmModal();
            this.loadUsers();
          } else {
            this.showError(response.message || 'Failed to update user status');
          }
        },
        error: (error) => {
          this.showError('Error updating user status');
          console.error(error);
        }
      });
    } else if (this.confirmModalData.action === 'block') {
      this.userService.blockUser(this.selectedUser.username).subscribe({
        next: (response: ApiResponse<boolean>) => {
          if (ApiResponseHelper.isSuccess(response)) {
            this.showSuccess(response.message || 'User blocked successfully');
            this.closeConfirmModal();
            this.loadUsers();
          } else {
            this.showError(response.message || 'Failed to block user');
          }
        },
        error: (error) => {
          this.showError('Error blocking user');
          console.error(error);
        }
      });
    } else if (this.confirmModalData.action === 'delete') {
      // Using block as delete
      this.userService.blockUser(this.selectedUser.username).subscribe({
        next: (response: ApiResponse<boolean>) => {
          if (ApiResponseHelper.isSuccess(response)) {
            this.showSuccess(response.message || 'User deleted successfully');
            this.closeConfirmModal();
            this.loadUsers();
          } else {
            this.showError(response.message || 'Failed to delete user');
          }
        },
        error: (error) => {
          this.showError('Error deleting user');
          console.error(error);
        }
      });
    }
  }

  getStatusText(isActive: string): string {
    return (isActive === '1' || isActive === 'true') ? 'Active' : 'Inactive';
  }

  getStatusClass(isActive: string): string {
    return (isActive === '1' || isActive === 'true') ? 'status-active' : 'status-inactive';
  }

  private showSuccess(message: string): void {
    this.success = message;
    setTimeout(() => this.success = null, 3000);
  }

  private showError(message: string): void {
    this.error = message;
    setTimeout(() => this.error = null, 5000);
  }

  refresh(): void {
    this.loadUsers();
  }

  clearError(): void {
    this.error = null;
  }

  get hasItems(): boolean {
    return this.users && this.users.length > 0;
  }

  get startRecord(): number {
    return this.totalItems === 0 ? 0 : (this.pageNumber - 1) * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalItems);
  }
}