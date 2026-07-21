// src/app/core/models/datatable.model.ts
export interface ColumnDefinition {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'number' | 'date' | 'currency' | 'actions' | 'boolean' | 'status' | 'badge' | 'image' | 'link' | 'progress';
  format?: (value: any) => string;
  className?: string;
  hidden?: boolean;
}

export interface DataTableConfig {
  pageSize?: number;
  pageSizeOptions?: number[];
  showSearch?: boolean;
  showExport?: boolean;
  showRefresh?: boolean;
  showView?: boolean;
  showEdit?: boolean;
  showDelete?: boolean;
  title?: string;
  emptyMessage?: string;
  loadingMessage?: string;
}