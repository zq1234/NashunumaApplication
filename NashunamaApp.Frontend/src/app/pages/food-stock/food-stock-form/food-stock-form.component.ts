import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FoodStockService } from '@shared/services/food-stock.service';
import { SiteLocationService } from '@shared/services/site-location.service';
import { FoodStock, CreateFoodStockDto, UpdateFoodStockDto } from '@core/models/food-stock.model';
import { SiteLocation } from '@core/models/site-location.model';
import { normalizeMissingStockNotification } from '@core/models/missing-stock.model';
import { AuthService } from '@shared/services/auth.service';
import { ApiResponse } from '@core/models/api-response.model';
import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { InputFieldComponent } from '@shared/components/form/input/input-field.component';
import { LabelComponent } from '@shared/components/form/label/label.component';
import { DatePickerComponent } from '@shared/components/form/date-picker/date-picker.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';

@Component({
  selector: 'app-food-stock-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageBreadcrumbComponent,
    ComponentCardComponent,
    InputFieldComponent,
    LabelComponent,
    DatePickerComponent,
    ButtonComponent
  ],
  templateUrl: './food-stock-form.component.html',
  styleUrls: ['./food-stock-form.component.scss']
})
export class FoodStockFormComponent implements OnInit {
  stock: Partial<CreateFoodStockDto & UpdateFoodStockDto> = {};
  loading = false;
  previousStockLoading = false;
  error: string | null = null;
  success: string | null = null;
  previousStockMessage: string | null = null;
  latestEnteredDate: string | null = null;
  missingStockDates: string[] = [];
  sites: SiteLocation[] = [];
  sitesLoading = false;
  isEdit = false;
  id: number | null = null;
  private previousStockRequest = 0;
  private dateConstraintsRequest = 0;

  private readonly stockBalances = [
    { label: 'Mamta boxes', opening: 'openingStockBoxesMamta', received: 'receivedStockBoxesMamta', distributed: 'distributedBoxesMamta', closing: 'closingStockBoxesMamta' },
    { label: 'Mamta sachets', opening: 'openingStockSachetsMamta', received: 'receivedStockSachetsMamta', distributed: 'distributedSachetsMamta', closing: 'closingStockSachetsMamta' },
    { label: 'Wawa boxes', opening: 'openingStockBoxesWawa', received: 'receivedStockBoxesWawa', distributed: 'distributedBoxesWawa', closing: 'closingStockBoxesWawa' },
    { label: 'Wawa sachets', opening: 'openingStockSachetsWawa', received: 'receivedStockSachetsWawa', distributed: 'distributedSachetsWawa', closing: 'closingStockSachetsWawa' },
    { label: 'RUTF', opening: 'rutfOpening', received: 'rutfReceived', distributed: 'rutfDistributed', closing: 'rutfClosing' },
    { label: 'IFA', opening: 'ifaOpening', received: 'ifaReceived', distributed: 'ifaDistributed', closing: 'ifaClosing' },
    { label: 'MMS', opening: 'mmsOpening', received: 'mmsReceived', distributed: 'mmsDistributed', closing: 'mmsClosing' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private foodStockService: FoodStockService,
    private authService: AuthService,
    private siteLocationService: SiteLocationService
  ) {}

  ngOnInit(): void {
    this.loadSites();
    const idParam = this.route.snapshot.params['id'];
    if (idParam) {
      this.isEdit = true;
      this.id = Number(idParam);
      this.loadStock(this.id);
    }

    // Prefill site from an optional query parameter; entry dates come from missing-date data.
    this.route.queryParams.subscribe(params => {
      const siteId = params['siteId'] ?? params['siteid'];

      if (siteId && !this.isEdit) {
        this.stock.siteId = siteId?.toString();
      }

      // If no siteId provided via query and not editing, try to prefill from current user
      if (!this.stock.siteId && !this.isEdit) {
        const user = this.authService.getCurrentUser();
        if (user && user.siteId !== undefined && user.siteId !== null) {
          this.stock.siteId = user.siteId?.toString();
        }
      }

      if (!this.isEdit && this.stock.siteId && this.stock.enteredOn) {
        this.loadPreviousDayStock(this.stock.siteId, this.stock.enteredOn);
      }
      if (!this.isEdit && this.stock.siteId) {
        this.loadDateConstraints(this.stock.siteId);
      }
    });
  }

  private loadSites(): void {
    this.sitesLoading = true;
    const user = this.authService.getCurrentUser();
    if (!user?.province) {
      this.loadSitesByFilters(null, null, null);
      return;
    }

    this.siteLocationService.getProvinces().subscribe({
      next: response => {
        const province = response.data?.find(item => this.sameLocationName(item.province, user.province));
        if (!response.isSuccess || !province) {
          this.failToLoadSites('Could not resolve your province to a location ID.');
          return;
        }

        if (!user.district) {
          this.loadSitesByFilters(province.id, null, null);
          return;
        }

        this.siteLocationService.getDistricts(province.id).subscribe({
          next: districtResponse => {
            const district = districtResponse.data?.find(item => this.sameLocationName(item.district, user.district));
            if (!districtResponse.isSuccess || !district) {
              this.failToLoadSites('Could not resolve your district to a location ID.');
              return;
            }

            if (!user.tehsil) {
              this.loadSitesByFilters(province.id, district.id, null);
              return;
            }

            this.siteLocationService.getTehsils(district.id).subscribe({
              next: tehsilResponse => {
                const tehsil = tehsilResponse.data?.find(item => this.sameLocationName(item.tehsil, user.tehsil));
                if (!tehsilResponse.isSuccess || !tehsil) {
                  this.failToLoadSites('Could not resolve your tehsil to a location ID.');
                  return;
                }
                this.loadSitesByFilters(province.id, district.id, tehsil.id);
              },
              error: error => this.failToLoadSites(error?.message ?? 'Unable to load tehsils.')
            });
          },
          error: error => this.failToLoadSites(error?.message ?? 'Unable to load districts.')
        });
      },
      error: error => this.failToLoadSites(error?.message ?? 'Unable to load provinces.')
    });
  }

  private loadSitesByFilters(provinceId: number | null, districtId: number | null, tehsilId: number | null): void {
    this.siteLocationService.getSitesByFilters(provinceId, districtId, tehsilId).subscribe({
      next: response => {
        this.sitesLoading = false;
        if (!response.isSuccess || !response.data) {
          this.failToLoadSites(response.message || 'Unable to load site locations.');
          return;
        }

        const data = response.data as unknown as { items?: SiteLocation[]; Items?: SiteLocation[] };
        this.sites = Array.isArray(response.data)
          ? response.data
          : Array.isArray(data.items)
            ? data.items
            : Array.isArray(data.Items)
              ? data.Items
              : [];

        if (!this.isEdit && this.stock.siteId && !this.sites.some(site => this.sameSiteId(site.id, this.stock.siteId))) {
          this.error = 'Your assigned site is not available in the selected location.';
        }
      },
      error: error => this.failToLoadSites(error?.message ?? 'Unable to load site locations.')
    });
  }

  private failToLoadSites(message: string): void {
    this.sitesLoading = false;
    this.sites = [];
    this.error = message;
  }

  private sameLocationName(first: string | null | undefined, second: string | null | undefined): boolean {
    return (first ?? '').trim().toLocaleLowerCase() === (second ?? '').trim().toLocaleLowerCase();
  }

  onSiteChange(siteId: string): void {
    this.stock.siteId = siteId || null;
    this.previousStockRequest++;
    this.previousStockLoading = false;
    this.previousStockMessage = null;

    if (this.isEdit) return;

    for (const balance of this.stockBalances) {
      (this.stock as Record<string, any>)[balance.opening] = '';
    }
    this.recalculateClosingStocks();

    if (siteId) {
      this.loadDateConstraints(siteId);
      if (this.stock.enteredOn) this.loadPreviousDayStock(siteId, this.stock.enteredOn);
    }
  }

  loadStock(id: number): void {
    this.loading = true;
    this.foodStockService.getFoodStockById(id).subscribe({
      next: (resp: ApiResponse<FoodStock>) => {
        this.loading = false;
        if (resp.isSuccess && resp.data) {
          // Cast to any to avoid strict type mismatch between domain DTO and form model
          this.stock = { ...(resp.data as any) } as any;
        } else {
          this.error = resp.message || 'Failed to load record';
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.message || 'Failed to load record';
      }
    });
  }

  save(): void {
    this.error = null;
    if (this.previousStockLoading) {
      this.error = 'Please wait while previous day stock is checked.';
      return;
    }
    this.loading = true;
    this.recalculateClosingStocks();
    // client-side validation
    const validation = this.validateFields();
    if (!validation.isValid) {
      this.loading = false;
      this.error = validation.message ?? 'Validation failed';
      return;
    }

    // If editing, prefer using backend /save endpoint to allow manual update
    // (backend must support update via save for this to actually update)
    if (this.isEdit) {
      // mark as manual update so backend can identify it
      (this.stock as any).isManualUpdate = '1';
      this.foodStockService.createFoodStock(this.stock as CreateFoodStockDto).subscribe({
        next: (resp) => this.handleSaveResponse(resp),
        error: (err) => {
          this.loading = false;
          this.error = err?.message || 'Save (edit) failed';
        }
      });
    } else {
      this.foodStockService.createFoodStock(this.stock as CreateFoodStockDto).subscribe({
        next: (resp) => this.handleSaveResponse(resp),
        error: (err) => {
          this.loading = false;
          this.error = err?.message || 'Create failed';
        }
      });
    }
  }

  private validateFields(): { isValid: boolean; message: string | null } {
    const messages: string[] = [];
    // EnteredOn must be present
    if (!this.stock.enteredOn || this.stock.enteredOn === '') {
      messages.push('EnteredOn date is required');
    }
    // OpeningStockBoxesWawa required by backend
    if (!this.stock.openingStockBoxesWawa || this.stock.openingStockBoxesWawa === '') {
      messages.push('OpeningStockBoxesWawa is required');
    }
    // SiteId should be available (auto-filled from auth)
    if (!this.stock.siteId || this.stock.siteId === '') {
      // Try to autofill from auth now as a last resort
      const user = this.authService.getCurrentUser();
      if (user && user.siteId !== undefined && user.siteId !== null) {
        this.stock.siteId = user.siteId?.toString();
      } else {
        messages.push('SiteId is required');
      }
    }

    for (const balance of this.stockBalances) {
      const opening = this.getNumericValue(balance.opening);
      const received = balance.received ? this.getNumericValue(balance.received) : 0;
      const distributed = this.getNumericValue(balance.distributed);
      const available = opening + received;

      const quantityFields = [balance.opening, balance.received, balance.distributed].filter(Boolean) as string[];
      for (const field of quantityFields) {
        const rawValue = (this.stock as Record<string, any>)[field];
        if (rawValue !== null && rawValue !== undefined && rawValue !== '') {
          const quantity = Number(rawValue);
          if (!Number.isFinite(quantity) || quantity < 0 || !Number.isInteger(quantity)) {
            messages.push(`${balance.label} quantities must be non-negative whole numbers.`);
            break;
          }
        }
      }

      if (distributed > available) {
        messages.push(`${balance.label} distributed quantity cannot exceed opening plus received stock (${available}).`);
      }
    }

    if (messages.length) {
      return { isValid: false, message: messages.join('; ') };
    }
    return { isValid: true, message: null };
  }

  onStockValueChange(field: string, value: string | number): void {
    (this.stock as Record<string, any>)[field] = value.toString();
    this.recalculateClosingStocks();
  }

  getDistributionError(distributedField: string): string | null {
    const balance = this.stockBalances.find(item => item.distributed === distributedField);
    if (!balance) return null;

    const opening = this.getNumericValue(balance.opening);
    const received = balance.received ? this.getNumericValue(balance.received) : 0;
    const distributed = this.getNumericValue(balance.distributed);
    const available = opening + received;

    return distributed > available
      ? `${balance.label} distribution cannot exceed available stock (${available}).`
      : null;
  }

  private recalculateClosingStocks(): void {
    for (const balance of this.stockBalances) {
      const fields = [balance.opening, balance.distributed, balance.received].filter(Boolean) as string[];
      const hasValues = fields.some(field => this.stock[field as keyof typeof this.stock] !== null && this.stock[field as keyof typeof this.stock] !== undefined && this.stock[field as keyof typeof this.stock] !== '');
      if (!hasValues) {
        (this.stock as Record<string, any>)[balance.closing] = '';
        continue;
      }

      const opening = this.getNumericValue(balance.opening);
      const received = balance.received ? this.getNumericValue(balance.received) : 0;
      const distributed = this.getNumericValue(balance.distributed);
      (this.stock as Record<string, any>)[balance.closing] = String(opening + received - distributed);
    }
  }

  private getNumericValue(field: string): number {
    const value = Number((this.stock as Record<string, any>)[field] ?? 0);
    return Number.isFinite(value) ? value : 0;
  }

  private loadPreviousDayStock(siteId: string, enteredOn: string): void {
    const requestId = ++this.previousStockRequest;
    const targetDate = this.toDateKey(enteredOn);
    if (!targetDate) return;

    this.previousStockLoading = true;
    this.previousStockMessage = null;
    this.foodStockService.getFoodStocksBySiteId(siteId).subscribe({
      next: response => {
        if (requestId !== this.previousStockRequest) return;
        this.previousStockLoading = false;

        if (!response.isSuccess || !response.data) {
          this.previousStockMessage = response.message || 'Could not load previous day stock.';
          return;
        }

        const previous = response.data
          .map(record => ({
            record,
            date: this.toDateKey(String((record as any).enteredOn ?? (record as any).EnteredOn ?? '')),
            siteId: String((record as any).siteId ?? (record as any).SiteId ?? '')
          }))
          .filter(item => item.siteId === siteId && !!item.date && item.date < targetDate)
          .sort((a, b) => b.date!.localeCompare(a.date!))[0];

        if (!previous) {
          this.setOpeningStocksToZero();
          this.previousStockMessage = 'No earlier stock record was found. Opening stocks were set to zero.';
          return;
        }

        for (const balance of this.stockBalances) {
          const closingField = balance.closing;
          const pascalClosingField = closingField.charAt(0).toUpperCase() + closingField.slice(1);
          const closingValue = (previous.record as any)[closingField] ?? (previous.record as any)[pascalClosingField] ?? '0';
          (this.stock as Record<string, any>)[balance.opening] = String(closingValue ?? '0');
        }
        this.recalculateClosingStocks();
        this.previousStockMessage = `Closing stock from the latest earlier record (${previous.date}) was used as opening stock.`;
      },
      error: () => {
        if (requestId !== this.previousStockRequest) return;
        this.previousStockLoading = false;
        this.previousStockMessage = 'Could not load previous day stock.';
      }
    });
  }

  private setOpeningStocksToZero(): void {
    for (const balance of this.stockBalances) {
      (this.stock as Record<string, any>)[balance.opening] = '0';
    }
    this.recalculateClosingStocks();
  }

  private toDateKey(value: string): string | null {
    const trimmed = value.trim();
    const isoDate = trimmed.match(/^(\d{4}-\d{2}-\d{2})/);
    if (isoDate) return isoDate[1];

    const dayFirst = trimmed.match(/^(\d{1,2})[-/](\d{1,2})[-/](\d{4})$/);
    if (!dayFirst) return null;
    return `${dayFirst[3]}-${dayFirst[2].padStart(2, '0')}-${dayFirst[1].padStart(2, '0')}`;
  }

  private loadDateConstraints(siteId: string): void {
    const requestId = ++this.dateConstraintsRequest;

    this.foodStockService.getFoodStocksBySiteId(siteId).subscribe({
      next: response => {
        if (requestId !== this.dateConstraintsRequest || !response.isSuccess || !response.data) return;

        const dates = response.data
          .map(record => this.toDateKey(String((record as any).enteredOn ?? (record as any).EnteredOn ?? '')))
          .filter((date): date is string => !!date);
        this.latestEnteredDate = dates.sort().at(-1) ?? null;
      }
    });

    this.foodStockService.getMissingStockDates().subscribe({
      next: response => {
        if (requestId !== this.dateConstraintsRequest || !response.isSuccess) return;
        const notification = normalizeMissingStockNotification(response.data);
        this.missingStockDates = (notification?.missingDates ?? [])
          .filter(item => item.isMissing)
          .map(item => this.toDateKey(item.date))
          .filter((date): date is string => !!date)
          .filter(date => !this.isWeekend(date))
          .sort();

        const notificationSiteId = String(notification?.siteId ?? '');
        const isSelectedSiteNotification = !notificationSiteId || this.sameSiteId(notificationSiteId, siteId);
        if (!this.isEdit && !this.stock.enteredOn && isSelectedSiteNotification && this.missingStockDates.length) {
          this.stock.enteredOn = this.missingStockDates[0];
          this.loadPreviousDayStock(siteId, this.stock.enteredOn);
        }
      }
    });
  }

  private sameSiteId(first: number | string | null | undefined, second: number | string | null | undefined): boolean {
    const firstValue = String(first ?? '').trim();
    const secondValue = String(second ?? '').trim();
    if (!firstValue || !secondValue) return false;

    const firstNumber = Number(firstValue);
    const secondNumber = Number(secondValue);
    return Number.isFinite(firstNumber) && Number.isFinite(secondNumber)
      ? firstNumber === secondNumber
      : firstValue === secondValue;
  }

  private isWeekend(dateKey: string): boolean {
    const [year, month, day] = dateKey.split('-').map(Number);
    const dayOfWeek = new Date(year, month - 1, day).getDay();
    return dayOfWeek === 0 || dayOfWeek === 6;
  }

  private handleSaveResponse(resp: ApiResponse<any>): void {
    this.loading = false;
    if (resp.isSuccess) {
      this.success = resp.message || 'Saved successfully';

      this.authService.refreshMissingDates();
      setTimeout(() => this.router.navigate(['/foodstock']), 800);
    } else {
      this.error = resp.message || 'Save failed';
    }
  }

  onEnteredOnChange(event: { dateStr?: string } | any): void {
    this.stock.enteredOn = event?.dateStr || '';
    if (!this.isEdit && this.stock.siteId && this.stock.enteredOn) {
      this.loadPreviousDayStock(this.stock.siteId, this.stock.enteredOn);
    }
  }

  cancel(): void {
    this.router.navigate(['/foodstock']);
  }
}
