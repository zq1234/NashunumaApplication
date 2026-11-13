// src/app/features/site-location/site-location-create/site-location-create.component.ts
import {
  Component, Input, Output, EventEmitter, OnInit,
  OnDestroy, inject
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
export class SiteLocationCreateComponent implements OnInit, OnDestroy {
  /** Controls the modal open state — same contract as your MissingStock popup */
  @Input() isOpen = false;

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
    contact: [''],
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
    });
    this.subs.add(s);
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

    const s = this.svc.createSite({
      siteName: (v.siteName ?? '').trim(),
      provinceId: v.provinceId!,
      districtId: v.districtId!,
      tehsilId: v.tehsilId!,
      ucId: v.ucId,
      isMobileSite: v.isMobileSite ? 'Yes' : 'No',
      contact: v.contact,
      headName: v.headName,
      siteTarget: v.siteTarget,
      geoLocation: v.geoLocation,
      latitude: v.latitude,
      longitude: v.longitude,
      address: v.address
    }).subscribe({
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
}