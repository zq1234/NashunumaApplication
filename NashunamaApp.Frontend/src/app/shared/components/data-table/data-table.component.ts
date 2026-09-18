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
import { FormsModule } from '@angular/forms'; // keep for native select binding
import { SwitchComponent } from '../form/input/switch.component';
import { SelectComponent } from '../form/select/select.component';

export interface DataTableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  type?: 'text' | 'number' | 'status' | 'badge' | 'date' | 'toggle';
  width?: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, FormsModule, SwitchComponent, SelectComponent],
  templateUrl: './data-table.component.html'
})
export class DataTableComponent implements OnChanges, OnInit {

  @Input() data: any[] = [];
  @Input() columns: DataTableColumn[] = [];
  @Input() title = 'Data Table';
  @Input() loading = false;
  @Input() showSearch = true;
  @Input() showActions = true;
  @Input() pageSizeOptions = [10, 25, 50, 100];
  @Input() defaultPageSize = 10;
  @Input() serverSidePaging = false;
  @Input() serverSideSearch = false;
  @Input() totalItems = 0;

  // whether to render the header area (page size, title, search)
  // (kept for potential future use)
  showHeader = true;

  /** Toggle config */
  @Input() toggleField = 'isactive';
  @Input() toggleIdField = 'username';
  @Input() toggleDisabled = false;

  /** Block config */
  @Input() blockField = 'isblocked';
  @Input() blockDisabled = false;

  @Output() edit = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();
  @Output() view = new EventEmitter<any>();
  @Output() toggle = new EventEmitter<any>();
  @Output() block = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() filterChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();

  filteredData: any[] = [];
  pagedData: any[] = [];
  searchText = '';
  currentPage = 1;
  pageSize = 10;
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

    if (this.searchText) {
      result = result.filter(row =>
        Object.values(row).some(value =>
          String(value).toLowerCase().includes(this.searchText.toLowerCase())
        )
      );
    }

    if (this.sortColumn) {
      result.sort((a, b) => {
        const valueA = a[this.sortColumn];
        const valueB = b[this.sortColumn];
        if (valueA < valueB) return this.sortDirection === 'asc' ? -1 : 1;
        if (valueA > valueB) return this.sortDirection === 'asc' ? 1 : -1;
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

  clearSearch(): void {
    this.searchText = '';
    this.currentPage = 1;
    this.applyFilters();
    this.filterChange.emit('');
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    if (this.serverSidePaging) {
      this.pageChange.emit({ page: this.currentPage, pageSize: this.pageSize });
      return;
    }
    this.updatePagination();
    this.pageChange.emit({ page: this.currentPage, pageSize: this.pageSize });

    // Ensure the underlying native select (or select2) reflects the new value visually.
    // Some select implementations return string values; set native select value and trigger change.
    setTimeout(() => {
      try {
        const sel = document.querySelector('select.page-size') as HTMLSelectElement | null;
        if (sel) {
          const v = String(this.pageSize);
          if (sel.value !== v) {
            sel.value = v;
            sel.dispatchEvent(new Event('change', { bubbles: true }));
          }
        }
      } catch { /* ignore errors */ }
    }, 0);
  }

  pageSizeChanged(value?: any): void {
    // accept optional emitted value (from select component) or fallback to bound pageSize
    const newSize = value !== undefined ? value : this.pageSize;
    // coerce pageSize to number in case the select component returns a string
    this.pageSize = Number(newSize) || this.defaultPageSize;
    this.currentPage = 1;
    if (this.serverSidePaging) {
      this.pageChange.emit({ page: this.currentPage, pageSize: this.pageSize });
      return;
    }
    this.updatePagination();
    this.pageChange.emit({ page: this.currentPage, pageSize: this.pageSize });
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  get pages(): Array<number | 'ellipsis'> {
    const total = this.totalPages;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

    const pageWindow = 2;
    const left = Math.max(2, this.currentPage - pageWindow);
    const right = Math.min(total - 1, this.currentPage + pageWindow);
    const pages: Array<number | 'ellipsis'> = [1];

    if (left > 2) pages.push('ellipsis');
    for (let page = left; page <= right; page++) pages.push(page);
    if (right < total - 1) pages.push('ellipsis');
    pages.push(total);
    return pages;
  }

  // ============================================
  // Toggle Helpers
  // ============================================

  isActiveValue(row: any): boolean {
    // allow common fallbacks for the id/field naming
    const value = row[this.toggleField] ?? row['isActive'] ?? row['isactive'];
    if (value === null || value === undefined) return false;
    const strValue = String(value).toLowerCase();
    return strValue === '1' || strValue === 'true' || strValue === 'active' || strValue === 'yes';
  }

  onToggle(row: any): void {
    if (this.toggleDisabled) return;
    // Emit structured payload: id field, row and new state
    const id = row[this.toggleIdField] ?? row['username'] ?? row['userName'];
    const newState = !this.isActiveValue(row);

    // mark row as pending to block further toggles until parent responds
    try {
      row.__togglePending = true;
    } catch {
      // ignore
    }

    this.toggle.emit({ id, row, active: newState });
  }

  // ============================================
  // Block Helpers
  // ============================================

  isBlocked(row: any): boolean {
    const value = row[this.blockField];
    if (value === null || value === undefined) return false;
    const strValue = String(value).toLowerCase();
    return strValue === '1' || strValue === 'true' || strValue === 'blocked' || strValue === 'yes';
  }

  onBlockToggle(row: any): void {
    if (this.blockDisabled) return;
    const id = row[this.toggleIdField];
    const newState = !this.isBlocked(row);
    this.block.emit({ id, row, blocked: newState });
  }

  // ============================================
  // Status / Badge Helpers
  // ============================================

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

  isStatusField(field: string): boolean {
    const statusFields = ['status', 'isactive', 'isActive', 'is_active', 'active',
                         'isActiveStatus', 'statusType', 'userStatus'];
    return statusFields.some(sf => field.toLowerCase().includes(sf.toLowerCase()));
  }

  isBadgeField(field: string): boolean {
    const badgeFields = ['type', 'role', 'category', 'usertype', 'level', 'priority',
                        'userType', 'designation', 'userRole'];
    return badgeFields.some(bf => field.toLowerCase().includes(bf.toLowerCase()));
  }

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
      if (strValue.includes(key)) return color;
    }
    return colorMap['default'];
  }

  getDisplayValue(row: any, field: string): string {
    const value = row[field];
    if (value === null || value === undefined) return '-';
    return String(value);
  }

  formatDate(value: any): string {
    if (!value) return '-';
    try {
      const date = new Date(value);
      if (isNaN(date.getTime())) return String(value);
      return date.toLocaleDateString('en-PK', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch {
      return String(value);
    }
  }

  formatNumber(value: any): string {
    if (value === null || value === undefined) return '-';
    const num = Number(value);
    if (isNaN(num)) return String(value);
    return num.toLocaleString();
  }

  getColumnType(col: DataTableColumn): string {
    return col.type || 'text';
  }

  refreshData(): void {
    this.applyFilters();
    this.cdr.detectChanges();
  }

  get currentPageItems(): any[] {
    return this.pagedData;
  }

  get hasData(): boolean {
    return this.data && this.data.length > 0;
  }

  get startRecord(): number {
    return this.totalItems === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}
