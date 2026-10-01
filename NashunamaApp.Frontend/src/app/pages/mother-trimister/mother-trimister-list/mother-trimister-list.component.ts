import { Component, OnInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MotherTrimisterDto } from '@core/models/mother-trimister.model';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DataTableComponent, DataTableColumn } from '@shared/components/data-table/data-table.component';
import { MotherTrimisterService } from '@shared/services/mother-trimister.service';
import { finalize } from 'rxjs/operators';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
//import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '@core/models/api-response.model';

@Component({
  selector: 'app-mother-trimister-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DataTableComponent, PageBreadcrumbComponent],
  templateUrl: './mother-trimister-list.component.html',
  styleUrls: ['./mother-trimister-list.component.scss']
})
export class MotherTrimisterListComponent implements OnInit {
  batchNumber = '';
  lastSearchBatchNumber = '';
  items: MotherTrimisterDto[] = [];
  loading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  // indicates where the current items came from: 'server' | 'upload' | 'export' | null
  resultSource: 'server' | 'upload' | 'export' | null = null;

  columns: DataTableColumn[] = [
     { field: 'srNo', header: 'S.No', sortable: false, width: '60px', type: 'number' },
    { field: 'MotherCnic', header: 'Mother CNIC' },
    { field: 'VisitDate', header: 'Visit Date' },
    { field: 'TrimisterNo', header: 'Trimester' },
    { field: 'SiteName', header: 'Site' },
    { field: 'PhoneNo', header: 'Phone' },
    { field: 'Address', header: 'Address' }
  ];

  private apiUrl = `${environment.apiUrl}/api/LookUp`;

  // Upload UI state
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;
  fileToUpload: File | null = null;
  uploadFormat: 'csv' | 'xlsx' = 'csv';
  uploadLoading = false;
  uploadError: string | null = null;
  uploadMessage: string | null = null;
  downloadSuccessMessage: string | null = null;
  batchNumbersNotFound: string[] = [];
  showUploadPanel = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private mts: MotherTrimisterService) {}

  ngOnInit(): void {}

  clearSelectedFile(): void {
    this.fileToUpload = null;
    this.uploadError = null;
    this.uploadMessage = null;
    this.batchNumbersNotFound = [];

    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.fileToUpload = file;
    this.uploadError = null;
    this.uploadMessage = null;
    this.batchNumbersNotFound = [];

    if (!file) {
      return;
    }

    const allowedTypes = ['.csv', '.xls', '.xlsx', '.txt'];
    const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!allowedTypes.includes(extension)) {
      this.fileToUpload = null;
      this.uploadError = 'Unsupported file type. Please select a CSV, XLS, XLSX, or TXT file.';
      input.value = '';
    }
  }

  get totalPages() {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get canRefresh(): boolean {
    const hasBatch = !!(this.batchNumber || this.lastSearchBatchNumber).trim();
    return !this.loading && !this.uploadLoading && (hasBatch || this.items.length > 0);
  }

  onSubmit(): void {
    if (this.batchNumber && this.batchNumber.trim()) {
      this.search();
    }
  }

  refresh(): void {
    const currentBatch = (this.batchNumber || this.lastSearchBatchNumber || '').trim();

    if (!currentBatch && this.items.length === 0) {
      this.items = [];
      this.totalCount = 0;
      this.resultSource = null;
      return;
    }

    if (currentBatch) {
      this.batchNumber = currentBatch;
      this.lastSearchBatchNumber = currentBatch;
    }

    this.pageNumber = 1;
    this.load();
  }

  toggleUploadPanel(): void {
    this.showUploadPanel = !this.showUploadPanel;
    if (this.showUploadPanel) {
      this.items = [];
      this.totalCount = 0;
      this.resultSource = null;
      this.downloadSuccessMessage = null;
      this.uploadError = null;
      this.uploadMessage = null;
      this.batchNumbersNotFound = [];
      this.fileToUpload = null;
      if (this.fileInput) {
        this.fileInput.nativeElement.value = '';
      }
    } else {
      this.clearSelectedFile();
    }
  }

  search(): void {
    const trimmedBatch = this.batchNumber?.trim() ?? '';
    this.batchNumber = trimmedBatch;
    this.lastSearchBatchNumber = trimmedBatch;
    this.pageNumber = 1;
    this.resultSource = null;
    this.load();
  }

  load(): void {
    const currentBatch = (this.batchNumber || this.lastSearchBatchNumber || '').trim();
    if (!currentBatch) {
      this.items = [];
      this.totalCount = 0;
      this.showUploadPanel = false;
      return;
    }

    this.batchNumber = currentBatch;
    this.lastSearchBatchNumber = currentBatch;

    this.loading = true;
    const params = new HttpParams()
      .set('batchNumber', currentBatch)
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    this.http.get<ApiResponse<any>>(`${this.apiUrl}/by-batch`, { params }).subscribe({
      next: (resp) => {
        this.loading = false;
        if (resp?.isSuccess && resp.data) {
          // normalize common API shapes: array, { items, totalCount }, { Items, TotalCount }, { data: { items } }
          const raw = resp.data as any;
          let list: any[] = [];
          let total = 0;

          if (Array.isArray(raw)) {
            list = raw;
            total = raw.length;
          } else if (Array.isArray(raw.items)) {
            list = raw.items;
            total = raw.totalCount ?? raw.totalcount ?? raw.TotalCount ?? list.length;
          } else if (Array.isArray(raw.Items)) {
            list = raw.Items;
            total = raw.TotalCount ?? raw.Totalcount ?? list.length;
          } else if (raw.data) {
            const inner = raw.data;
            if (Array.isArray(inner)) {
              list = inner;
              total = inner.length;
            } else if (Array.isArray(inner.items)) {
              list = inner.items;
              total = inner.totalCount ?? inner.TotalCount ?? list.length;
            }
          } else {
            // fallback: try to find any array property
            const arrProp = Object.keys(raw).find(k => Array.isArray(raw[k]));
            if (arrProp) {
              list = raw[arrProp];
              total = raw.totalCount ?? raw.TotalCount ?? list.length;
            }
          }

          // normalize property names (API returns camelCase keys)
          const startIndex = (this.pageNumber - 1) * this.pageSize;
          const mapped = list.map((it: any, idx: number) => ({
            srNo: startIndex + idx + 1,
            MotherCnic: it.motherCnic ?? it.MotherCnic ?? it.mothercnic ?? '',
            VisitDate: it.visitDate ?? it.VisitDate ?? it.visitdate ?? '',
            TrimisterNo: it.trimisterNo ?? it.TrimisterNo ?? it.trimisterno ?? '',
            SiteName: it.siteName ?? it.SiteName ?? it.sitename ?? '',
            PhoneNo: it.phoneNo ?? it.PhoneNo ?? it.phoneno ?? '',
            Address: it.address ?? it.Address ?? '',
            ...it
          }));
          this.items = mapped as MotherTrimisterDto[];
          this.totalCount = total || this.items.length;
          this.resultSource = 'server';
          this.showUploadPanel = this.items.length === 0 && !!this.batchNumber.trim();
          // ensure change detection so DataTable picks up new input
          this.cdr.detectChanges();
        } else {
          this.items = [];
          this.totalCount = 0;
          this.showUploadPanel = !!this.batchNumber.trim();
        }
      },
      error: () => {
        this.loading = false;
        this.items = [];
        this.totalCount = 0;
        this.showUploadPanel = !!this.batchNumber.trim();
      }
    });
  }

  prevPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.load();
    }
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.load();
    }
  }

  // Handler for DataTable server-side paging
  onPageChange(evt: { page: number; pageSize: number }): void {
    this.pageNumber = evt.page;
    this.pageSize = evt.pageSize;
    this.load();
  }
  uploadBatch(): void {
    if (!this.fileToUpload) {
      this.uploadError = 'Please select a file to upload.';
      return;
    }
    this.uploadLoading = true;
    this.uploadError = null;
    this.uploadMessage = null;
    this.batchNumbersNotFound = [];

    this.mts.uploadBatchFile(this.fileToUpload, this.uploadFormat).pipe(
      finalize(() => (this.uploadLoading = false))
    ).subscribe({
      next: (outcome: any) => {
        if (outcome && outcome.kind === 'json') {
          const payload = outcome.payload as any;
          this.uploadMessage = payload?.message ?? null;
          const dto = payload?.data ?? payload;
          const inner = dto?.data ?? dto;
          const items = Array.isArray(inner?.items) ? inner.items : (Array.isArray(inner) ? inner : []);

          const mapped = (items as any[]).map((it: any, idx: number) => ({
            srNo: idx + 1,
            MotherCnic: it.motherCnic ?? it.MotherCnic ?? it.mothercnic ?? '',
            VisitDate: it.visitDate ?? it.VisitDate ?? it.visitdate ?? '',
            TrimisterNo: it.trimisterNo ?? it.TrimisterNo ?? it.trimisterno ?? '',
            SiteName: it.siteName ?? it.SiteName ?? it.sitename ?? '',
            PhoneNo: it.phoneNo ?? it.PhoneNo ?? it.phoneno ?? '',
            Address: it.address ?? it.Address ?? '',
            ...it
          }));

          this.items = mapped as MotherTrimisterDto[];
          this.totalCount = (inner?.totalCount) ?? this.items.length;
          this.batchNumbersNotFound = payload?.data?.batchNumbersNotFound ?? [];
          this.resultSource = 'upload';
          this.cdr.detectChanges();
        } else if (outcome && outcome.kind === 'file') {
          const url = URL.createObjectURL(outcome.blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = outcome.filename || 'BatchExport';
          document.body.appendChild(a);
          a.click();
          a.remove();
          URL.revokeObjectURL(url);
          const fileType = outcome.contentType?.includes('spreadsheet') ? 'Excel' : 'CSV';
          this.clearSelectedFile();
          this.uploadMessage = `Downloaded records as ${fileType}.`;
          this.downloadSuccessMessage = 'Data has been downloaded in Excel successfully.';
          this.resultSource = 'export';
        }
      },
      error: (err: any) => {
        this.uploadError = err?.message ?? 'An error occurred while uploading the file.';
      }
    });
  }
}
