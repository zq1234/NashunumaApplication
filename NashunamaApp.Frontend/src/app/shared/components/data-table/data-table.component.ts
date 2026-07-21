import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  OnInit,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DataTableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  type?: 'text' | 'number' | 'status' | 'badge' | 'date';
  width?: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent implements OnChanges, OnInit {
  
  @Input() data: any[] = [];
  @Input() columns: DataTableColumn[] = [];
  @Input() title = 'Data Table';
  @Input() loading = false;
  @Input() showSearch = true;
  @Input() showActions = true;
  @Input() pageSizeOptions = [5, 10, 25, 50, 100];
  @Input() defaultPageSize = 10;
  @Input() serverSidePaging = false;
  @Input() serverSideSearch = false;
  @Input() totalItems = 0;

  @Output() edit = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();
  @Output() view = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() filterChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();

  // Data
  filteredData: any[] = [];
  pagedData: any[] = [];

  // Search and Filter
  searchText = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;

  // Sorting
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.pageSize = this.defaultPageSize;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['defaultPageSize'] && !changes['defaultPageSize'].isFirstChange()) {
      this.pageSize = this.defaultPageSize;
      this.currentPage = 1;
    }

    if (changes['data'] || changes['defaultPageSize'] || changes['serverSidePaging'] || changes['serverSideSearch'] || changes['totalItems']) {
      if (this.serverSidePaging) {
        this.filteredData = [...this.data];
        this.pagedData = [...this.data];
        this.totalItems = this.totalItems || this.data.length;
        if (this.currentPage > this.totalPages) {
          this.currentPage = this.totalPages || 1;
        }
      } else {
        this.applyFilters();
      }
    }
  }

  applyFilters(): void {
    if (this.serverSidePaging) {
      this.filteredData = [...this.data];
      this.pagedData = [...this.data];
      this.totalItems = this.totalItems || this.data.length;
      return;
    }

    let result = [...this.data];

    // Apply search filter
    if (this.searchText) {
      result = result.filter(row =>
        Object.values(row).some(value =>
          String(value)
            .toLowerCase()
            .includes(this.searchText.toLowerCase())
        )
      );
    }

    // Apply sorting
    if (this.sortColumn) {
      result.sort((a, b) => {
        const valueA = a[this.sortColumn];
        const valueB = b[this.sortColumn];

        if (valueA < valueB) {
          return this.sortDirection === 'asc' ? -1 : 1;
        }
        if (valueA > valueB) {
          return this.sortDirection === 'asc' ? 1 : -1;
        }
        return 0;
      });
    }

    this.filteredData = result;
    this.totalItems = result.length;
    this.updatePagination();
  }

  updatePagination(): void {
    if (this.serverSidePaging) {
      this.pagedData = [...this.filteredData];
      return;
    }

    const start = (this.currentPage - 1) * this.pageSize;
    this.pagedData = this.filteredData.slice(start, start + this.pageSize);
  }

  // Sort
  sort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    if (this.serverSidePaging) {
      this.sortChange.emit({ column, direction: this.sortDirection });
      return;
    }

    this.applyFilters();
  }

  // Apply filter
  applyFilter(filterValue: string): void {
    this.searchText = filterValue;
    this.currentPage = 1;

    if (this.serverSideSearch || this.serverSidePaging) {
      this.filterChange.emit(filterValue);
      return;
    }

    this.applyFilters();
    this.filterChange.emit(filterValue);
  }

  // Clear search
  clearSearch(): void {
    this.searchText = '';
    this.currentPage = 1;
    this.applyFilters();
    this.filterChange.emit('');
  }

  // Page change
  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }
    this.currentPage = page;

    if (this.serverSidePaging) {
      this.pageChange.emit({
        page: this.currentPage,
        pageSize: this.pageSize
      });
      return;
    }

    this.updatePagination();
    this.pageChange.emit({
      page: this.currentPage,
      pageSize: this.pageSize
    });
  }

  // Page size change
  pageSizeChanged(): void {
    this.currentPage = 1;

    if (this.serverSidePaging) {
      this.pageChange.emit({
        page: this.currentPage,
        pageSize: this.pageSize
      });
      return;
    }

    this.updatePagination();
    this.pageChange.emit({
      page: this.currentPage,
      pageSize: this.pageSize
    });
  }

  // Get total pages
  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  // Get pages array
  get pages(): Array<number | 'ellipsis'> {
    const total = this.totalPages;
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const pageWindow = 2;
    const left = Math.max(2, this.currentPage - pageWindow);
    const right = Math.min(total - 1, this.currentPage + pageWindow);
    const pages: Array<number | 'ellipsis'> = [1];

    if (left > 2) {
      pages.push('ellipsis');
    }

    for (let page = left; page <= right; page++) {
      pages.push(page);
    }

    if (right < total - 1) {
      pages.push('ellipsis');
    }

    pages.push(total);
    return pages;
  }

  // Get status helper methods
  getStatusClass(value: any): string {
    const strValue = String(value).toLowerCase();
    if (strValue === 'active' || strValue === '1' || strValue === 'true' || strValue === 'yes') {
      return 'status-active';
    }
    if (strValue === 'inactive' || strValue === '0' || strValue === 'false' || strValue === 'no') {
      return 'status-inactive';
    }
    return 'status-neutral';
  }

  getStatusText(value: any): string {
    const strValue = String(value).toLowerCase();
    if (strValue === 'active' || strValue === '1' || strValue === 'true' || strValue === 'yes') {
      return 'Active';
    }
    if (strValue === 'inactive' || strValue === '0' || strValue === 'false' || strValue === 'no') {
      return 'Inactive';
    }
    return String(value);
  }

  // Check if field should be rendered as status
  isStatusField(field: string): boolean {
    const statusFields = ['status', 'isactive', 'isActive', 'is_active', 'active', 
                         'isActiveStatus', 'statusType', 'userStatus'];
    return statusFields.some(sf => field.toLowerCase().includes(sf.toLowerCase()));
  }

  // Check if field should be rendered as badge
  isBadgeField(field: string): boolean {
    const badgeFields = ['type', 'role', 'category', 'usertype', 'level', 'priority', 
                        'userType', 'designation', 'userRole'];
    return badgeFields.some(bf => field.toLowerCase().includes(bf.toLowerCase()));
  }

  // Get badge color based on value
  getBadgeColor(value: any): string {
    const strValue = String(value).toLowerCase();
    const colorMap: { [key: string]: string } = {
      'admin': 'badge-purple',
      'user': 'badge-blue',
      'manager': 'badge-green',
      'coordinator': 'badge-yellow',
      'super admin': 'badge-red',
      'default': 'badge-gray'
    };
    
    for (const [key, color] of Object.entries(colorMap)) {
      if (strValue.includes(key)) {
        return color;
      }
    }
    return colorMap['default'];
  }

  // Get the value for display
  getDisplayValue(row: any, field: string): string {
    const value = row[field];
    if (value === null || value === undefined) {
      return '-';
    }
    return String(value);
  }

  // Format date
  formatDate(value: any): string {
    if (!value) return '-';
    try {
      const date = new Date(value);
      if (isNaN(date.getTime())) return String(value);
      return date.toLocaleDateString('en-PK', {
        day: '2-digit',
        month: 'short',
        year: 'numeric'
      });
    } catch {
      return String(value);
    }
  }

  // Format number
  formatNumber(value: any): string {
    if (value === null || value === undefined) return '-';
    const num = Number(value);
    if (isNaN(num)) return String(value);
    return num.toLocaleString();
  }

  // Get column type
  getColumnType(col: DataTableColumn): string {
    return col.type || 'text';
  }

  // Refresh data
  refreshData(): void {
    this.applyFilters();
    this.cdr.detectChanges();
  }

  // Get current page items
  get currentPageItems(): any[] {
    return this.pagedData;
  }

  // Check if data is empty
  get hasData(): boolean {
    return this.data && this.data.length > 0;
  }

  // Get start record
  get startRecord(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  // Get end record
  get endRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}