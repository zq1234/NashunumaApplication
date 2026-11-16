// src/app/features/site-location/site-location-create/site-location-create.component.ts
import {
  Component, Input, Output, EventEmitter, OnInit, OnChanges,
  OnDestroy, SimpleChanges, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule, ReactiveFormsModule, FormBuilder, Validators
} from '@angular/forms';
import { Subscription } from 'rxjs';

import { SiteLocationService } from '@shared/services/site-location.service';
import {
  SiteLocation,
  ProvinceLookup,
  DistrictLookup,
  TehsilLookup,
  UcLookup
} from '@core/models/site-location.model';
import { ApiResponseHelper } from '@core/models/api-response.model';

import { ModalComponent } from '@shared/components/ui/modal/modal.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';

@Component({
  selector: 'app-site-location-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ModalComponent,
    ButtonComponent
  ],
  templateUrl: './site-location-create.component.html',
  styleUrls: ['./site-location-create.component.css']
})
export class SiteLocationCreateComponent implements OnInit, OnChanges, OnDestroy {
  /** Controls the modal open state — same contract as your MissingStock popup */
  @Input() isOpen = false;
  @Input() site: SiteLocation | null = null;

  /** Emitted when the dialog should close (dismiss or cancel) */
  @Output() close = new EventEmitter<void>();

  /** Emitted after a successful create, so parent can refresh the list */
  @Output() created = new EventEmitter<SiteLocation>();

  private readonly svc = inject(SiteLocationService);
  private readonly fb = inject(FormBuilder);

  // ---------- State ----------
  saving = false;
  errorMessage: string | null = null;

  provinces: ProvinceLookup[] = [];
  districts: DistrictLookup[] = [];
  tehsils: TehsilLookup[] = [];
  ucs: UcLookup[] = [];

  private subs = new Subscription();

  // ---------- Form ----------
  form = this.fb.group({
    siteName: ['', [Validators.required, Validators.maxLength(200)]],
    provinceId: [null as number | null, Validators.required],
    districtId: [null as number | null, Validators.required],
    tehsilId: [null as number | null, Validators.required],
    ucId: [null as number | null],
    isMobileSite: [false],
    isActive: [true],
    contact: ['', Validators.pattern(/^03\d{2}-\d{7}$/)],
    headName: [''],
    siteTarget: [null as number | null],
    geoLocation: [''],
    latitude: [null as number | null],
    longitude: [null as number | null],
    address: ['']
  });

  ngOnInit(): void {
    // Load provinces once — safe to do in init even when modal is closed
    const s = this.svc.getProvinces().subscribe(res => {
      this.provinces = ApiResponseHelper.isSuccess(res) ? res.data : [];
      if (this.isOpen && this.site) this.populateForEdit(this.site);
    });
    this.subs.add(s);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.isOpen) return;
    if (changes['site'] || changes['isOpen']) {
      if (this.site) {
        if (this.provinces.length) this.populateForEdit(this.site);
      } else {
        this.reset();
      }
    }
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ---------- Cascading lookups ----------

  onProvinceChange(): void {
    const provinceId = this.form.value.provinceId;
    this.districts = []; this.tehsils = []; this.ucs = [];
    this.form.patchValue(
      { districtId: null, tehsilId: null, ucId: null },
      { emitEvent: false }
    );
    if (!provinceId) return;

    const s = this.svc.getDistricts(provinceId).subscribe(r => {
      this.districts = ApiResponseHelper.isSuccess(r) ? r.data : [];
    });
    this.subs.add(s);
  }

  onDistrictChange(): void {
    const districtId = this.form.value.districtId;
    this.tehsils = []; this.ucs = [];
    this.form.patchValue(
      { tehsilId: null, ucId: null },
      { emitEvent: false }
    );
    if (!districtId) return;

    const s = this.svc.getTehsils(districtId).subscribe(r => {
      this.tehsils = ApiResponseHelper.isSuccess(r) ? r.data : [];
    });
    this.subs.add(s);
  }

  onTehsilChange(): void {
    const tehsilId = this.form.value.tehsilId;
    this.ucs = [];
    this.form.patchValue({ ucId: null }, { emitEvent: false });
    if (!tehsilId) return;

    const s = this.svc.getUcs(tehsilId).subscribe(r => {
      this.ucs = ApiResponseHelper.isSuccess(r) ? r.data : [];
    });
    this.subs.add(s);
  }

  // ---------- Actions ----------

  dismiss(): void {
    if (this.saving) return;
    this.close.emit();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.saving = true;
    this.errorMessage = null;

    const request = {
      siteName: (v.siteName ?? '').trim(),
      provinceId: v.provinceId!,
      districtId: v.districtId!,
      tehsilId: v.tehsilId!,
      ucId: v.ucId,
      isMobileSite: v.isMobileSite ? '1' : '0',
      contact: this.mobileDigits(v.contact),
      headName: v.headName,
      siteTarget: v.siteTarget,
      geoLocation: v.geoLocation,
      latitude: v.latitude,
      longitude: v.longitude,
      address: v.address,
      isActive: v.isActive ?? true
    };

    const request$ = this.site
      ? this.svc.updateSite(this.site.id, request)
      : this.svc.createSite(request);

    const s = request$.subscribe({
      next: res => {
        this.saving = false;
        if (ApiResponseHelper.isSuccess(res) && res.data) {
          this.created.emit(res.data);
          this.reset();
          this.close.emit();
        } else {
          this.errorMessage = ApiResponseHelper.getErrorMessage(res);
        }
      },
      error: err => {
        this.saving = false;
        this.errorMessage = err?.message ?? 'Failed to create site';
      }
    });

    this.subs.add(s);
  }

  private reset(): void {
    this.form.reset({
      siteName: '',
      provinceId: null,
      districtId: null,
      tehsilId: null,
      ucId: null,
      isMobileSite: false,
      isActive: true,
      contact: '',
      headName: '',
      siteTarget: null,
      geoLocation: '',
      latitude: null,
      longitude: null,
      address: ''
    });
    this.districts = [];
    this.tehsils = [];
    this.ucs = [];
    this.errorMessage = null;
  }

  private populateForEdit(site: SiteLocation): void {
    this.errorMessage = null;
    this.svc.getDistricts(site.provinceId!).subscribe(response => {
      if (!ApiResponseHelper.isSuccess(response) || this.site?.id !== site.id) return;
      this.districts = response.data;

      this.svc.getTehsils(site.districtId!).subscribe(tehsilResponse => {
        if (!ApiResponseHelper.isSuccess(tehsilResponse) || this.site?.id !== site.id) return;
        this.tehsils = tehsilResponse.data;

        this.svc.getUcs(site.tehsilId!).subscribe(ucResponse => {
          if (!ApiResponseHelper.isSuccess(ucResponse) || this.site?.id !== site.id) return;
          this.ucs = ucResponse.data;
          this.form.patchValue({
            siteName: site.siteName ?? '',
            provinceId: site.provinceId,
            districtId: site.districtId,
            tehsilId: site.tehsilId,
            ucId: site.ucId,
            isMobileSite: this.isYes(site.isMobileSite),
            // isActive: site.isActive,
            contact: site.contact ?? '',
            headName: site.headName ?? '',
            siteTarget: site.siteTarget,
            geoLocation: site.geoLocation ?? '',
            latitude: site.latitude,
            longitude: site.longitude,
            address: site.address ?? ''
          });
        });
      });
    });
  }

  private isYes(value: string | null): boolean {
    return value?.toLowerCase() === 'yes' || value === '1' || value?.toLowerCase() === 'true';
  }

  formatContactInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 11);
    input.value = digits.length > 4
      ? `${digits.slice(0, 4)}-${digits.slice(4)}`
      : digits;
    this.form.controls.contact.setValue(input.value);
  }

  private mobileDigits(value: string | null): string | null {
    const digits = value?.replace(/\D/g, '') ?? '';
    return digits || null;
  }
}