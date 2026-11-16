import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { ApiResponseHelper } from '@core/models/api-response.model';
import { SiteLocation } from '@core/models/site-location.model';
import { SiteLocationService } from '@shared/services/site-location.service';
import { ButtonComponent } from '@shared/components/ui/button/button.component';
import { SiteLocationCreateComponent } from '@shared/components/site-location/site-location-create/site-location-create.component';

@Component({
  selector: 'app-site-location-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, SiteLocationCreateComponent],
  templateUrl: './site-location-list.component.html',
  styleUrls: ['./site-location-list.component.css'],
})
export class SiteLocationListComponent implements OnInit, OnDestroy {
  private readonly siteLocationService = inject(SiteLocationService);
  private readonly destroy$ = new Subject<void>();

  sites: SiteLocation[] = [];
  loading = false;
  errorMessage: string | null = null;

  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  searchTerm = '';
  isCreateModalOpen = false;
  selectedSite: SiteLocation | null = null;
  deletingSiteId: number | null = null;

  ngOnInit(): void {
    this.loadSites();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSites(): void {
    this.loading = true;
    this.errorMessage = null;
    this.siteLocationService.getSites(this.pageNumber, this.pageSize, { search: this.searchTerm || undefined })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.loading = false;
          if (ApiResponseHelper.isSuccess(response) && response.data) {
            this.sites = response.data.items ?? [];
            this.totalCount = response.data.totalCount ?? 0;
            this.totalPages = response.data.totalPages ?? 0;
            return;
          }

          this.errorMessage = ApiResponseHelper.getErrorMessage(response);
          this.sites = [];
        },
        error: error => {
          this.loading = false;
          this.errorMessage = error?.message ?? 'Unable to load site locations.';
          this.sites = [];
        }
      });
  }

  onSearch(): void {
    this.pageNumber = 1;
    this.loadSites();
  }

  openCreateModal(): void {
    this.selectedSite = null;
    this.isCreateModalOpen = true;
  }

  openEditModal(site: SiteLocation): void {
    this.selectedSite = site;
    this.isCreateModalOpen = true;
  }

  closeCreateModal(): void {
    this.isCreateModalOpen = false;
  }

  onSiteCreated(): void {
    this.closeCreateModal();
    this.selectedSite = null;
    this.pageNumber = 1;
    this.loadSites();
  }

  deleteSite(site: SiteLocation): void {
    const name = site.siteName || 'this site';
    if (!window.confirm(`Delete ${name}? This action cannot be undone.`)) return;

    this.errorMessage = null;
    this.deletingSiteId = site.id;
    this.siteLocationService.deleteSite(site.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: response => {
          this.deletingSiteId = null;
          if (ApiResponseHelper.isSuccess(response)) {
            if (this.sites.length === 1 && this.pageNumber > 1) this.pageNumber--;
            this.loadSites();
          } else {
            this.errorMessage = ApiResponseHelper.getErrorMessage(response);
          }
        },
        error: error => {
          this.deletingSiteId = null;
          this.errorMessage = error?.message ?? 'Unable to delete site location.';
        }
      });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }

    this.pageNumber = page;
    this.loadSites();
  }

  getLocationText(site: SiteLocation): string {
    return [site.province, site.district, site.tehsil, site.uc]
      .filter(Boolean)
      .join(' / ');
  }

  getStatusText(site: SiteLocation): string {
    return site.isActive ? 'Active' : 'Inactive';
  }

  getStatusClass(site: SiteLocation): string {
    return site.isActive
      ? 'bg-green-100 text-green-700 dark:bg-green-500/10 dark:text-green-400'
      : 'bg-red-100 text-red-700 dark:bg-red-500/10 dark:text-red-400';
  }
}
