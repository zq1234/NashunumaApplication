// src/app/pages/users/user-list/user-list.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, finalize } from 'rxjs';
import { UserManagementService } from '@shared/services/user-management.service';
import { LocationService, Province, District, Tehsil } from '@shared/services/location.service';
import { UserDto, UserFilterParams } from '@core/models/user.model';
import { UpdateUserLocationDto, LocationHelper } from '@core/models/user-location.model';
import { ApiResponse, PaginatedResponse, ApiResponseHelper } from '@core/models/api-response.model';
import { DataTableComponent, DataTableColumn } from '@shared/components/data-table/data-table.component';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';
import { AlertComponent } from '@shared/components/ui/alert/alert.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DataTableComponent,
    ModalComponent,
    AlertComponent
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
  loadingTransfer = false;
  loadingStatus = false;
  loadingBlock = false;
  loadingExport = false;
  error: string | null = null;
  success: string | null = null;
  loadingLocations = false;
  
  // Alert
  alertVariant: 'success' | 'error' | 'warning' | 'info' = 'info';
  alertTitle = '';
  alertMessage = '';
  showAlert = false;
  
  // Filters
  searchTerm = '';
  selectedProvinceName: string | null = null;
  selectedDistrictName: string | null = null;
  selectedTehsilName: string | null = null;
  userType = '';
  isActive: boolean | null = null;
  
  // Dropdown options for filters
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
  
  // Transfer form - using LocationHelper
  transferForm: UpdateUserLocationDto = LocationHelper.createDefaultUpdateDto();
  
  // Transfer location dropdown data
  transferProvinces: Province[] = [];
  transferDistricts: District[] = [];
  transferTehsils: Tehsil[] = [];
  transferProvinceName: string | null = null;
  transferDistrictName: string | null = null;
  transferTehsilName: string | null = null;
  
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

  // ============================================
  // Alert Methods
  // ============================================

  private displayAlert(variant: 'success' | 'error' | 'warning' | 'info', title: string, message: string): void {
    this.alertVariant = variant;
    this.alertTitle = title;
    this.alertMessage = message;
    this.showAlert = true;
    
    setTimeout(() => {
      this.showAlert = false;
    }, 5000);
  }

  private showSuccess(message: string): void {
    this.success = message;
    this.displayAlert('success', 'Success', message);
  }

  private showError(message: string): void {
    this.error = message;
    this.displayAlert('error', 'Error', message);
  }

  clearAlert(): void {
    this.showAlert = false;
    this.error = null;
    this.success = null;
  }

  // ============================================
  // Location Data Methods
  // ============================================

  loadLocationData(): void {
    this.loadingLocations = true;
    
    const cachedProvinces = this.locationService.getCachedProvinces();
    if (cachedProvinces && cachedProvinces.length > 0) {
      this.provinces = cachedProvinces;
      this.transferProvinces = cachedProvinces;
      this.loadingLocations = false;
      return;
    }

    this.locationService.getProvinces().subscribe({
      next: (response: ApiResponse<Province[]>) => {
        this.loadingLocations = false;
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.provinces = response.data;
          this.transferProvinces = response.data;
        } else {
          this.showError('Failed to load provinces: ' + response.message);
          this.setDefaultFilterOptions();
        }
      },
      error: (error) => {
        this.loadingLocations = false;
        this.showError('Error loading provinces: ' + error.message);
        this.setDefaultFilterOptions();
      }
    });
  }

  private setDefaultFilterOptions(): void {
    this.provinces = [
      { id: 1, provcode: 1, province: 'Punjab' },
      { id: 2, provcode: 2, province: 'Sindh' },
      { id: 3, provcode: 3, province: 'KPK' },
      { id: 4, provcode: 4, province: 'Balochistan' },
      { id: 5, provcode: 5, province: 'Islamabad' },
      { id: 6, provcode: 6, province: 'Gilgit-Baltistan' },
      { id: 7, provcode: 7, province: 'AJK' }
    ];
    this.transferProvinces = this.provinces;
  }

  // ============================================
  // Filter Methods
  // ============================================

  onProvinceChange(provinceName: string | null): void {
    this.selectedProvinceName = provinceName;
    this.selectedDistrictName = null;
    this.selectedTehsilName = null;
    this.districts = [];
    this.tehsils = [];
    this.districtDisplay = '';
    this.tehsilDisplay = '';

    if (provinceName) {
      this.provinceDisplay = provinceName;
      this.loadDistrictsForFilter(provinceName);
    }
    
    this.pageNumber = 1;
    this.loadUsers();
  }

  private loadDistrictsForFilter(provinceName: string): void {
    const cachedDistricts = this.locationService.getCachedDistricts(provinceName);
    if (cachedDistricts && cachedDistricts.length > 0) {
      this.districts = cachedDistricts;
      return;
    }

    this.locationService.getDistrictsByProvince(provinceName).subscribe({
      next: (response: ApiResponse<District[]>) => {
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.districts = response.data;
        }
      },
      error: (error) => {
        this.showError('Error loading districts: ' + error.message);
      }
    });
  }

  onDistrictChange(districtName: string | null): void {
    this.selectedDistrictName = districtName;
    this.selectedTehsilName = null;
    this.tehsils = [];
    this.tehsilDisplay = '';

    if (districtName) {
      this.districtDisplay = districtName;
      this.loadTehsilsForFilter(districtName);
    }
    
    this.pageNumber = 1;
    this.loadUsers();
  }

  private loadTehsilsForFilter(districtName: string): void {
    const cachedTehsils = this.locationService.getCachedTehsils(districtName);
    if (cachedTehsils && cachedTehsils.length > 0) {
      this.tehsils = cachedTehsils;
      return;
    }

    this.locationService.getTehsilsByDistrict(districtName).subscribe({
      next: (response: ApiResponse<Tehsil[]>) => {
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.tehsils = response.data;
        }
      },
      error: (error) => {
        this.showError('Error loading tehsils: ' + error.message);
      }
    });
  }

  onTehsilChange(tehsilName: string | null): void {
    this.selectedTehsilName = tehsilName;
    this.tehsilDisplay = tehsilName || '';
    this.pageNumber = 1;
    this.loadUsers();
  }

  // ============================================
  // Transfer Location Methods
  // ============================================

  transferLocation(user: UserDto): void {
    this.selectedUser = user;
    this.transferForm = LocationHelper.toUpdateDto(user);
    
    this.transferProvinceName = user.province || null;
    this.transferDistrictName = user.district || null;
    this.transferTehsilName = user.tehsil || null;
    
    if (this.transferProvinceName) {
      this.loadTransferDistricts(this.transferProvinceName);
    }
    if (this.transferDistrictName) {
      this.loadTransferTehsils(this.transferDistrictName);
    }
    
    this.isTransferModalOpen = true;
  }

  private loadTransferDistricts(provinceName: string): void {
    const cachedDistricts = this.locationService.getCachedDistricts(provinceName);
    if (cachedDistricts && cachedDistricts.length > 0) {
      this.transferDistricts = cachedDistricts;
      return;
    }

    this.locationService.getDistrictsByProvince(provinceName).subscribe({
      next: (response: ApiResponse<District[]>) => {
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.transferDistricts = response.data;
        }
      },
      error: (error) => {
        this.showError('Error loading districts for transfer: ' + error.message);
      }
    });
  }

  private loadTransferTehsils(districtName: string): void {
    const cachedTehsils = this.locationService.getCachedTehsils(districtName);
    if (cachedTehsils && cachedTehsils.length > 0) {
      this.transferTehsils = cachedTehsils;
      return;
    }

    this.locationService.getTehsilsByDistrict(districtName).subscribe({
      next: (response: ApiResponse<Tehsil[]>) => {
        if (ApiResponseHelper.isSuccess(response) && response.data) {
          this.transferTehsils = response.data;
        }
      },
      error: (error) => {
        this.showError('Error loading tehsils for transfer: ' + error.message);
      }
    });
  }

  onTransferProvinceChange(provinceName: string | null): void {
    this.transferProvinceName = provinceName;
    this.transferDistrictName = null;
    this.transferTehsilName = null;
    this.transferDistricts = [];
    this.transferTehsils = [];
    
    this.transferForm = {
      ...this.transferForm,
      province: provinceName || '',
      district: '',
      tehsil: ''
    };

    if (provinceName) {
      this.loadTransferDistricts(provinceName);
    }
  }

  onTransferDistrictChange(districtName: string | null): void {
    this.transferDistrictName = districtName;
    this.transferTehsilName = null;
    this.transferTehsils = [];
    
    this.transferForm = {
      ...this.transferForm,
      district: districtName || '',
      tehsil: ''
    };

    if (districtName) {
      this.loadTransferTehsils(districtName);
    }
  }

  onTransferTehsilChange(tehsilName: string | null): void {
    this.transferTehsilName = tehsilName;
    this.transferForm = {
      ...this.transferForm,
      tehsil: tehsilName || ''
    };
  }

  closeTransferModal(): void {
    this.isTransferModalOpen = false;
    this.selectedUser = null;
    this.transferProvinceName = null;
    this.transferDistrictName = null;
    this.transferTehsilName = null;
    this.transferDistricts = [];
    this.transferTehsils = [];
    this.transferForm = LocationHelper.createDefaultUpdateDto();
  }

  handleTransferSubmit(): void {
    if (!this.selectedUser) return;
    
    const location = LocationHelper.fromUser(this.selectedUser);
    if (!location.province) {
      this.showError('Please select a province');
      return;
    }
    if (!location.district) {
      this.showError('Please select a district');
      return;
    }
    
    this.loadingTransfer = true;
    
    this.userService.transferUserLocation(this.selectedUser.username, this.transferForm)
      .pipe(finalize(() => this.loadingTransfer = false))
      .subscribe({
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
          this.showError('Error transferring location: ' + error.message);
        }
      });
  }

  // ============================================
  // User Loading Methods
  // ============================================

  loadUsers(): void {
    this.loading = true;
    this.clearAlert();

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

    this.userService.getPagedUsers(filters)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response: ApiResponse<PaginatedResponse<UserDto>>) => {
          if (ApiResponseHelper.isSuccess(response) && response.data) {
            this.users = response.data.items.map((item, index) => ({
              ...item,
              srNo: (this.pageNumber - 1) * this.pageSize + index + 1
            }));
            this.totalItems = response.data.totalCount;
          } else {
            this.showError(response.message || 'Failed to load users');
          }
        },
        error: (error) => {
          this.showError('Error loading users: ' + error.message);
        }
      });
  }

  // ============================================
  // Helper Methods
  // ============================================

  getActiveCount(): number {
    return this.users.filter(user => user.isactive === '1' || user.isactive === 'true').length;
  }

  getInactiveCount(): number {
    return this.users.filter(user => user.isactive !== '1' && user.isactive !== 'true').length;
  }

  getAdminCount(): number {
    return this.users.filter(user => user.usertype?.toLowerCase() === 'admin').length;
  }

  // ============================================
  // Event Handlers
  // ============================================

  onView(user: UserDto): void {
    this.viewProfile(user);
  }

  onEdit(user: UserDto): void {
    this.transferLocation(user);
  }

  onDelete(user: UserDto): void {
    this.blockUser(user);
  }

  onSearchChange(term: string): void {
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
    this.selectedProvinceName = null;
    this.selectedDistrictName = null;
    this.selectedTehsilName = null;
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

  // ============================================
  // Profile Modal
  // ============================================

  viewProfile(user: UserDto): void {
    this.selectedUser = user;
    this.isProfileModalOpen = true;
  }

  closeProfileModal(): void {
    this.isProfileModalOpen = false;
    this.selectedUser = null;
  }

  // ============================================
  // Confirm Modal
  // ============================================

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

  closeConfirmModal(): void {
    this.isConfirmModalOpen = false;
    this.selectedUser = null;
  }

  handleConfirmAction(): void {
    if (!this.selectedUser) return;

    if (this.confirmModalData.action === 'toggle') {
      this.loadingStatus = true;
      const isActive = this.selectedUser.isactive === '1' || this.selectedUser.isactive === 'true';
      
      this.userService.toggleUserStatus(this.selectedUser.username, !isActive)
        .pipe(finalize(() => this.loadingStatus = false))
        .subscribe({
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
            this.showError('Error updating user status: ' + error.message);
          }
        });
    } else if (this.confirmModalData.action === 'block') {
      this.loadingBlock = true;
      
      this.userService.blockUser(this.selectedUser.username)
        .pipe(finalize(() => this.loadingBlock = false))
        .subscribe({
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
            this.showError('Error blocking user: ' + error.message);
          }
        });
    }
  }

  // ============================================
  // UI Helpers
  // ============================================

  getStatusText(isActive: string): string {
    return (isActive === '1' || isActive === 'true') ? 'Active' : 'Inactive';
  }

  getStatusClass(isActive: string): string {
    return (isActive === '1' || isActive === 'true') ? 'status-active' : 'status-inactive';
  }

  refresh(): void {
    this.loadUsers();
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

  exportUsers(): void {
    this.loadingExport = true;
    const filters = {
      searchTerm: this.searchTerm || undefined,
      province: this.provinceDisplay || undefined,
      district: this.districtDisplay || undefined,
      tehsil: this.tehsilDisplay || undefined,
      userType: this.userType || undefined,
      isActive: this.isActive !== null ? this.isActive : undefined
    };

    this.userService.exportUsers(filters)
      .pipe(finalize(() => this.loadingExport = false))
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `users_export_${new Date().toISOString().split('T')[0]}.xlsx`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.showSuccess('Users exported successfully');
        },
        error: (error) => {
          this.showError('Failed to export users: ' + error.message);
        }
      });
  }
}