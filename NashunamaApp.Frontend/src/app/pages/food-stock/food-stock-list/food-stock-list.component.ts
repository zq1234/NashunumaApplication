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
  
  // UI State
  loading = false;
  error: string | null = null;
  success: string | null = null;
  
  // Filters
  searchTerm = '';
  showAll = false;
  hideMobile = false;
  onlyMobile = false;
  
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
  deleteConfirmText = '';
  
  // Column Definitions for DataTableComponent
  columns: DataTableColumn[] = [
    { field: 'srNo', header: 'S.No', sortable: false, width: '60px', type: 'number' },
    { field: 'siteName', header: 'Site Name', sortable: true, width: '200px' },
    { field: 'province', header: 'Province', sortable: true, width: '120px' },
    { field: 'district', header: 'District', sortable: true, width: '120px' },
    { field: 'tehsil', header: 'Tehsil', sortable: true, width: '120px' },
    { field: 'wawa', header: 'Wawa', sortable: true, width: '80px', type: 'number' },
    { field: 'mamta', header: 'Mamta', sortable: true, width: '80px', type: 'number' },
    { field: 'rutf', header: 'RUTF', sortable: true, width: '80px', type: 'number' },
    { field: 'dated', header: 'Dated', sortable: true, width: '130px', type: 'date' },
    { field: 'enteredBy', header: 'Entered By', sortable: true, width: '120px' },
    { field: 'activityTime', header: 'Activity Time', sortable: true, width: '130px' }
  ];

  constructor(
    private foodStockService: FoodStockService,
    public router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.setupSearchDebounce();
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

    // Apply checkbox filters
    if (this.onlyMobile) {
      filter.isMobileSite = '1';
    } else if (this.hideMobile) {
      filter.isMobileSite = '0';
    }

    this.foodStockService.getFoodStocks(
      this.pageNumber,
      this.pageSize,
      filter
    ).subscribe({
      next: (response: ApiResponse<PaginatedResponse<FoodStock>>) => {
        if (response.isSuccess && response.data) {
          this.foodStocks = response.data.items.map((item, index) => ({
            ...item,
            srNo: (this.pageNumber - 1) * this.pageSize + index + 1,
            wawa: this.calculateWawa(item),
            mamta: this.calculateMamta(item),
            rutf: this.calculateRutf(item),
            dated: this.formatDate(item.enteredOn),
            activityTime: this.formatTime(item.enteredOn)
          }));
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

  formatDate(dateString: string | null): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-PK', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
      });
    } catch {
      return dateString;
    }
  }

  formatTime(dateString: string | null): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleTimeString('en-PK', {
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateString;
    }
  }

  onSearchChange(term: string): void {
    this.searchSubject.next(term);
  }

  // Handle view from datatable
  onView(item: FoodStock): void {
    this.router.navigate(['/foodstock', item.id]);
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

  toggleShowAll(): void {
    this.showAll = !this.showAll;
    if (this.showAll) {
      this.hideMobile = false;
      this.onlyMobile = false;
    }
    this.pageNumber = 1;
    this.loadFoodStocks();
  }

  toggleHideMobile(): void {
    this.hideMobile = !this.hideMobile;
    if (this.hideMobile) {
      this.onlyMobile = false;
      this.showAll = false;
    }
    this.pageNumber = 1;
    this.loadFoodStocks();
  }

  toggleOnlyMobile(): void {
    this.onlyMobile = !this.onlyMobile;
    if (this.onlyMobile) {
      this.hideMobile = false;
      this.showAll = false;
    }
    this.pageNumber = 1;
    this.loadFoodStocks();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.showAll = false;
    this.hideMobile = false;
    this.onlyMobile = false;
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
}
