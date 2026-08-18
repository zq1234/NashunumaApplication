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
  error: string | null = null;
  success: string | null = null;
  isEdit = false;
  id: number | null = null;

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
    this.loading = true;
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

    if (messages.length) {
      return { isValid: false, message: messages.join('; ') };
    }
    return { isValid: true, message: null };
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
  }

  cancel(): void {
    this.router.navigate(['/foodstock']);
  }
}
