import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MotherTrimisterDto } from '@core/models/mother-trimister.model';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DataTableComponent, DataTableColumn } from '@shared/components/data-table/data-table.component';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '@core/models/api-response.model';

@Component({
  selector: 'app-mother-trimister-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DataTableComponent, PageBreadcrumbComponent, ComponentCardComponent],
  templateUrl: './mother-trimister-list.component.html',
  styleUrls: ['./mother-trimister-list.component.scss']
})
export class MotherTrimisterListComponent implements OnInit {
  batchNumber = '';
  items: MotherTrimisterDto[] = [];
  loading = false;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;

  columns: DataTableColumn[] = [
    { field: 'MotherCnic', header: 'Mother CNIC' },
    { field: 'VisitDate', header: 'Visit Date' },
    { field: 'TrimisterNo', header: 'Trimester' },
    { field: 'SiteName', header: 'Site' },
    { field: 'PhoneNo', header: 'Phone' },
    { field: 'Address', header: 'Address' }
  ];

  private apiUrl = `${environment.apiUrl}/api/LookUp`;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  get totalPages() {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  search(): void {
    this.pageNumber = 1;
    this.load();
  }

  load(): void {
    if (!this.batchNumber) {
      this.items = [];
      this.totalCount = 0;
      return;
    }

    this.loading = true;
    const params = new HttpParams()
      .set('batchNumber', this.batchNumber)
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
          const mapped = list.map((it: any) => ({
            MotherCnic: it.motherCnic ?? it.MotherCnic ?? it.mothercnic ?? '',
            VisitDate: it.visitDate ?? it.VisitDate ?? it.visitdate ?? '',
            TrimisterNo: it.trimisterNo ?? it.TrimisterNo ?? it.trimisterno ?? '',
            SiteName: it.siteName ?? it.SiteName ?? it.sitename ?? '',
            PhoneNo: it.phoneNo ?? it.PhoneNo ?? it.phoneno ?? '',
            Address: it.address ?? it.Address ?? '',
            // keep original properties as fallback
            ...it
          }));

          this.items = mapped as MotherTrimisterDto[];
          this.totalCount = total || this.items.length;
          // ensure change detection so DataTable picks up new input
          this.cdr.detectChanges();
        } else {
          this.items = [];
          this.totalCount = 0;
        }
      },
      error: () => {
        this.loading = false;
        this.items = [];
        this.totalCount = 0;
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
}
