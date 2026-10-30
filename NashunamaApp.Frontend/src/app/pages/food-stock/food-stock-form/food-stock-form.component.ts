import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FoodStockService } from '@shared/services/food-stock.service';
import { FoodStock, CreateFoodStockDto, UpdateFoodStockDto } from '@core/models/food-stock.model';
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
  isEdit = false;
  id: number | null = null;
  private previousStockRequest = 0;

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
    private foodStockService: FoodStockService
    , private authService: AuthService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.params['id'];
    if (idParam) {
      this.isEdit = true;
      this.id = Number(idParam);
      this.loadStock(this.id);
    }

    // Prefill from query params (e.g., ?date=25-07-2026 or ?siteId=5)
    this.route.queryParams.subscribe(params => {
      const date = params['date'];
      const siteId = params['siteId'] ?? params['siteid'];

      if (date && !this.isEdit) {
        // Accept date format as provided (e.g., dd-MM-yyyy)
        this.stock.enteredOn = date;
      }

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
    });
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
    this.foodStockService.getFoodStocks(1, 100, {
      siteId,
      pageNumber: 1,
      pageSize: 100,
      sortBy: 'enteredOn',
      sortDescending: true
    }).subscribe({
      next: response => {
        if (requestId !== this.previousStockRequest) return;
        this.previousStockLoading = false;

        if (!response.isSuccess || !response.data) {
          this.previousStockMessage = response.message || 'Could not load previous day stock.';
          return;
        }

        const previousDate = this.getPreviousDateKey(targetDate);
  const previous = response.data.items.find(record => {
          const recordSiteId = String((record as any).siteId ?? (record as any).SiteId ?? '');
          const recordDate = this.toDateKey((record as any).enteredOn ?? (record as any).EnteredOn ?? '');
          return recordSiteId === siteId && recordDate === previousDate;
        });

        if (!previous) {
          this.setOpeningStocksToZero();
          this.previousStockMessage = 'No previous-day stock record was found. Opening stocks were set to zero.';
          return;
        }

        for (const balance of this.stockBalances) {
          const closingField = balance.closing;
          const pascalClosingField = closingField.charAt(0).toUpperCase() + closingField.slice(1);
          const closingValue = (previous as any)[closingField] ?? (previous as any)[pascalClosingField] ?? '0';
          (this.stock as Record<string, any>)[balance.opening] = String(closingValue ?? '0');
        }
        this.recalculateClosingStocks();
        this.previousStockMessage = `Previous day's closing stock (${previousDate}) was used as today's opening stock.`;
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

  private getPreviousDateKey(dateKey: string): string {
    const [year, month, day] = dateKey.split('-').map(Number);
    const previousDate = new Date(Date.UTC(year, month - 1, day - 1));
    return `${previousDate.getUTCFullYear()}-${String(previousDate.getUTCMonth() + 1).padStart(2, '0')}-${String(previousDate.getUTCDate()).padStart(2, '0')}`;
  }

  private handleSaveResponse(resp: ApiResponse<any>): void {
    this.loading = false;
    if (resp.isSuccess) {
      this.success = resp.message || 'Saved successfully';

      // After successful save, if there is an auto-sequence of missing dates, navigate to next
      const next = this.authService.popNextMissingDate();
      if (next) {
        // small delay to allow UI update before navigating
        setTimeout(() => this.router.navigate(['/foodstock/create'], { queryParams: { date: next } }), 600);
      } else {
        // No more pending dates - refresh notification to check if any remain server-side,
        // then go back to list. This keeps the modal visible until the backend reports no missing dates.
        this.authService.refreshMissingDates();
        setTimeout(() => this.router.navigate(['/foodstock']), 800);
      }
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
