import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { FoodStockService } from '@shared/services/food-stock.service';
import { FoodStock, FoodStockFilter } from '../../../core/models/food-stock.model';
import { ApiResponse, PaginatedResponse } from '../../../core/models/api-response.model';
import { DataTableComponent, DataTableColumn } from '@shared/components/data-table/data-table.component';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';
import { AuthService } from '@shared/services/auth.service';
import { MissingStockDateDto } from '@core/models/missing-stock.model';

@Component({
  selector: 'app-food-stock-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DataTableComponent,
    ModalComponent,
    PageBreadcrumbComponent,
    ComponentCardComponent,
    ButtonComponent
  ],
  templateUrl: './food-stock-list.component.html',
  styleUrls: ['./food-stock-list.component.scss']
})
export class FoodStockListComponent implements OnInit, OnDestroy {
  // Data
  foodStocks: FoodStock[] = [];
  totalItems = 0;
  totalSites = 0;
  notUpdated = 0;
  lowStockSites = 0;
  missingStockDates: MissingStockDateDto[] = [];
  
  // UI State
  loading = false;
  error: string | null = null;
  success: string | null = null;
  
  // Filters
  searchTerm = '';
  mobileFilter: 'all' | 'hide' | 'only' = 'all';
  
  // Pagination
  pageNumber = 1;
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50, 100];
  
  // Search debounce
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  
  // Modal states
  isDeleteModalOpen = false;
  selectedItem: FoodStock | null = null;
  isDetailModalOpen = false;
  selectedDetailStock: FoodStock | null = null;
  deleteConfirmText = '';
  
  // Column Definitions for DataTableComponent
  columns: DataTableColumn[] = [
    { field: 'srNo', header: 'S.No', sortable: false, width: '60px', type: 'number' },
    { field: 'province', header: 'Province', sortable: true, width: '120px' },
    { field: 'district', header: 'District', sortable: true, width: '120px' },
    { field: 'tehsil', header: 'Tehsil', sortable: true, width: '120px' },
    { field: 'siteName', header: 'Site Name', sortable: true, width: '200px' },
    { field: 'dated', header: 'Dated', sortable: true, width: '130px', type: 'date' },
    { field: 'wawa', header: 'Wawa', sortable: true, width: '80px', type: 'number' },
    { field: 'mamta', header: 'Mamta', sortable: true, width: '80px', type: 'number' },
    { field: 'rutf', header: 'RUTF', sortable: true, width: '80px', type: 'number' },
    { field: 'ifa', header: 'IFA', sortable: true, width: '80px' }
  ];

  constructor(
    private foodStockService: FoodStockService,
    public router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.setupSearchDebounce();
    this.authService.missingStockNotification$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(notification => {
      this.missingStockDates = notification?.missingDates.filter(item => item.isMissing) ?? [];
    });
    this.authService.refreshMissingDates();
    this.loadFoodStocks();
    this.loadSummary();
    
    // Subscribe to loading state from service
    this.foodStockService.loading$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(loading => {
      this.loading = loading;
    });
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
      this.loadFoodStocks();
    });
  }

  loadFoodStocks(): void {
    this.error = null;

    const filter: FoodStockFilter = {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    if (this.mobileFilter === 'only') {
      filter.isMobileSite = '1';
    } else if (this.mobileFilter === 'hide') {
      filter.isMobileSite = '0';
    }

    this.foodStockService.getFoodStocks(
      this.pageNumber,
      this.pageSize,
      filter
    ).subscribe({
      next: (response: ApiResponse<PaginatedResponse<FoodStock>>) => {
        if (response.isSuccess && response.data) {
          this.foodStocks = response.data.items.map((item, index) => {
            const normalized = { ...item, ...this.normalizeStockFields(item) };
            return {
              ...normalized,
              srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
              wawa: this.calculateWawa(normalized),
              mamta: this.calculateMamta(normalized),
              rutf: this.calculateRutf(normalized),
              ifa: normalized.ifaClosing || 'N/A',
              dated: this.formatDate(normalized.enteredOn),
              activityTime: this.formatTime(normalized.enteredOn)
            };
          });
          this.totalItems = response.data.totalCount;
        } else {
          this.error = response.message || 'Failed to load food stocks';
        }
      },
      error: (error) => {
        this.error = error.message || 'An error occurred while loading food stocks';
      }
    });
  }

  loadSummary(): void {
    this.foodStockService.getSummaryStats()
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.isSuccess && response.data) {
            this.totalSites = response.data.totalSites || 0;
            this.notUpdated = response.data.notUpdated || 0;
            this.lowStockSites = response.data.lowStockSites || 0;
          }
        },
        error: (error) => {
          console.error('Error loading summary:', error);
        }
      });
  }

  calculateWawa(item: FoodStock): number {
    const opening = parseInt(item.openingStockBoxesWawa || '0');
    const received = parseInt(item.receivedStockBoxesWawa || '0');
    const distributed = parseInt(item.distributedBoxesWawa || '0');
    return opening + received - distributed;
  }

  calculateMamta(item: FoodStock): number {
    const opening = parseInt(item.openingStockBoxesMamta || '0');
    const received = parseInt(item.receivedStockBoxesMamta || '0');
    const distributed = parseInt(item.distributedBoxesMamta || '0');
    return opening + received - distributed;
  }

  calculateRutf(item: FoodStock): number {
    const opening = parseInt(item.rutfOpening || '0');
    const received = parseInt(item.rutfReceived || '0');
    const distributed = parseInt(item.rutfDistributed || '0');
    return opening + received - distributed;
  }

  private normalizeStockFields(item: FoodStock): Partial<FoodStock> {
    const source = item as unknown as Record<string, unknown>;
    const aliases: Record<string, string[]> = {
      openingStockBoxesMamta: ['OpeningStockBoxesMamta', 'OPENING_STOCK_BOXES_MAMTA'],
      receivedStockBoxesMamta: ['ReceivedStockBoxesMamta', 'RECEIVED_STOCK_BOXES_MAMTA'],
      distributedBoxesMamta: ['DistributedBoxesMamta', 'DISTRIBUTED_BOXES_MAMTA'],
      closingStockBoxesMamta: ['ClosingStockBoxesMamta', 'CLOSING_STOCK_BOXES_MAMTA'],
      openingStockBoxesWawa: ['OpeningStockBoxesWawa', 'OPENING_STOCK_BOXES_WAWA'],
      receivedStockBoxesWawa: ['ReceivedStockBoxesWawa', 'RECEIVED_STOCK_BOXES_WAWA'],
      distributedBoxesWawa: ['DistributedBoxesWawa', 'DISTRIBUTED_BOXES_WAWA'],
      closingStockBoxesWawa: ['ClosingStockBoxesWawa', 'CLOSING_STOCK_BOXES_WAWA'],
      openingStockSachetsMamta: ['OpeningStockSachetsMamta', 'OPENING_STOCK_SACHETS_MAMTA'],
      receivedStockSachetsMamta: ['ReceivedStockSachetsMamta', 'RECEIVED_STOCK_SACHETS_MAMTA'],
      distributedSachetsMamta: ['DistributedSachetsMamta', 'DISTRIBUTED_SACHETS_MAMTA'],
      closingStockSachetsMamta: ['ClosingStockSachetsMamta', 'CLOSING_STOCK_SACHETS_MAMTA'],
      openingStockSachetsWawa: ['OpeningStockSachetsWawa', 'OPENING_STOCK_SACHETS_WAWA'],
      receivedStockSachetsWawa: ['ReceivedStockSachetsWawa', 'RECEIVED_STOCK_SACHETS_WAWA'],
      distributedSachetsWawa: ['DistributedSachetsWawa', 'DISTRIBUTED_SACHETS_WAWA'],
      closingStockSachetsWawa: ['ClosingStockSachetsWawa', 'CLOSING_STOCK_SACHETS_WAWA'],
      rutfOpening: ['RutfOpening', 'RUTF_OPENING'],
      rutfReceived: ['RutfReceived', 'RUTF_RECEIVED'],
      rutfDistributed: ['RutfDistributed', 'RUTF_DISTRIBUTED'],
      rutfClosing: ['RutfClosing', 'RUTF_CLOSING'],
      ifaOpening: ['IfaOpening', 'IFA_OPENING'],
      ifaReceived: ['IfaReceived', 'IFA_RECEIVED'],
      ifaDistributed: ['IfaDistributed', 'IFA_DISTRIBUTED'],
      ifaClosing: ['IfaClosing', 'IFA_CLOSING']
    };

    const normalized: Record<string, unknown> = {};
    for (const [field, keys] of Object.entries(aliases)) {
      const value = source[field] ?? keys.map(key => source[key]).find(candidate => candidate !== null && candidate !== undefined);
      if (value !== undefined) normalized[field] = value;
    }
    return normalized as Partial<FoodStock>;
  }

  private parseFlexibleDate(dateString: string | null): Date | null {
    if (!dateString) return null;

    const value = dateString.trim();
    if (!value) return null;

    const isoMatch = value.match(/^\d{4}-\d{2}-\d{2}(?:[T\s].*)?$/i);
    if (isoMatch) {
      const normalized = value.includes('T') || value.includes(' ') ? value : `${value}T00:00:00`;
      const parsed = new Date(normalized);
      return Number.isNaN(parsed.getTime()) ? null : parsed;
    }

    const dmyMatch = value.match(/^\d{1,2}[/-]\d{1,2}[/-]\d{4}$/);
    if (dmyMatch) {
      const [day, month, year] = value.split(/[/-]/).map(part => Number(part));
      const parsed = new Date(year, month - 1, day);
      return Number.isNaN(parsed.getTime()) ? null : parsed;
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  formatDate(dateString: string | null): string {
    const date = this.parseFlexibleDate(dateString);
    if (!date) return dateString || '-';

    return date.toLocaleDateString('en-PK', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  formatTime(dateString: string | null): string {
    const date = this.parseFlexibleDate(dateString);
    if (!date) return dateString || '-';

    return date.toLocaleTimeString('en-PK', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  onSearchChange(term: string): void {
    this.searchSubject.next(term);
  }

  // Handle view from datatable
  onView(item: FoodStock): void {
    this.openDetails(item);
  }

  addNew(): void {
    this.router.navigate(['/foodstock/create']);
  }

  openDetails(item: FoodStock): void {
    if (item.siteId !== null && item.siteId !== undefined && String(item.siteId).trim()) {
      this.router.navigate(['/foodstock/site', item.siteId], {
        queryParams: {
          siteName: item.siteName || null,
          province: item.province || null,
          district: item.district || null,
          tehsil: item.tehsil || null
        }
      });
      return;
    }

    if (item.id !== null && item.id !== undefined) {
      this.router.navigate(['/foodstock', item.id]);
    }
  }

  openDetailModal(item: FoodStock): void {
    this.selectedDetailStock = item;
    this.isDetailModalOpen = true;
  }

  closeDetailModal(): void {
    this.isDetailModalOpen = false;
    this.selectedDetailStock = null;
  }

  // Handle edit from datatable
  onEdit(item: FoodStock): void {
    console.log('Edit food stock:', item);
    this.router.navigate(['/foodstock/edit', item.id]);
  }

  // Handle delete from datatable
  onDelete(item: FoodStock): void {
    this.selectedItem = item;
    this.isDeleteModalOpen = true;
  }

  // Handle page change from datatable
  onPageChange(event: { page: number; pageSize: number }): void {
    this.pageNumber = event.page;
    this.pageSize = event.pageSize;
    this.loadFoodStocks();
  }

  // Handle filter change from datatable
  onFilterChange(searchTerm: string): void {
    // This is called when search changes in datatable
    // We already handle search via onSearchChange
  }

  setMobileFilter(filter: 'all' | 'hide' | 'only'): void {
    this.mobileFilter = filter;
    this.pageNumber = 1;
    this.loadFoodStocks();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.mobileFilter = 'all';
    this.pageNumber = 1;
    this.searchSubject.next('');
    this.loadFoodStocks();
  }

  // Delete confirmation
  confirmDelete(): void {
    if (!this.selectedItem || !this.selectedItem.id) return;

    this.foodStockService.deleteFoodStock(this.selectedItem.id).subscribe({
      next: (response: ApiResponse<boolean>) => {
        if (response.isSuccess) {
          this.success = 'Food stock deleted successfully';
          this.isDeleteModalOpen = false;
          this.selectedItem = null;
          this.loadFoodStocks();
          this.loadSummary();
          setTimeout(() => this.success = null, 3000);
        } else {
          this.error = response.message || 'Failed to delete food stock';
          this.isDeleteModalOpen = false;
        }
      },
      error: (error) => {
        this.error = error.message || 'Failed to delete food stock';
        this.isDeleteModalOpen = false;
      }
    });
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.selectedItem = null;
  }

  exportToExcel(): void {
    const filter: Partial<FoodStockFilter> = {
      searchTerm: this.searchTerm || undefined
    };

    this.foodStockService.exportFoodStocks('excel', filter).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `food-stock-export-${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        this.success = 'Export completed successfully';
        setTimeout(() => this.success = null, 5000);
      },
      error: (error) => {
        this.error = 'Failed to export data. Please try again.';
        console.error('Export error:', error);
      }
    });
  }

  refresh(): void {
    this.foodStockService.refresh();
    this.loadFoodStocks();
    this.loadSummary();
  }

  clearError(): void {
    this.error = null;
  }

  get hasItems(): boolean {
    return this.foodStocks && this.foodStocks.length > 0;
  }

  get startRecord(): number {
    return this.totalItems === 0 ? 0 : (this.pageNumber - 1) * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalItems);
  }

  getDetailValue(value: string | number | null | undefined): string {
    return value === null || value === undefined || value === '' ? '0' : String(value);
  }

  getNumber(value: string | number | null | undefined): number {
    const parsed = Number(value ?? 0);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  formatDetailDate(value: string | null | undefined): string {
    if (!value) return '-';

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return value;

    return parsed.toLocaleDateString('en-PK', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  formatDetailTime(value: string | null | undefined): string {
    if (!value) return '-';

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return value;

    return parsed.toLocaleTimeString('en-PK', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
