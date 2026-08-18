import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StockService } from '@shared/services/stock.service';

@Component({
  selector: 'app-food-stock',
  templateUrl: './food-stock.component.html',
  styleUrls: ['./food-stock.component.scss']
})
export class FoodStockComponent implements OnInit {
  form: FormGroup;
  statusMessage = '';

  constructor(private fb: FormBuilder, private stockService: StockService) {
    this.form = this.fb.group({
      EnteredOn: [null, Validators.required],
      Unit: [''],
      Remarks: [''],

      OpeningStockBoxesMamta: [''],
      ReceivedStockBoxesMamta: [''],
      DistributedBoxesMamta: [''],
      ClosingStockBoxesMamta: [''],
      OpeningStockSachetsMamta: [''],
      DistributedSachetsMamta: [''],
      ClosingStockSachetsMamta: [''],

      OpeningStockBoxesWawa: ['', Validators.required],
      ReceivedStockBoxesWawa: [''],
      DistributedBoxesWawa: [''],
      ClosingStockBoxesWawa: [''],
      OpeningStockSachetsWawa: [''],
      DistributedSachetsWawa: [''],
      ClosingStockSachetsWawa: [''],

      RutfOpening: [''], RutfReceived: [''], RutfDistributed: [''], RutfClosing: [''],
      IfaOpening: [''], IfaReceived: [''], IfaDistributed: [''], IfaClosing: [''],
      MmsOpening: [''], MmsReceived: [''], MmsDistributed: [''], MmsClosing: ['']
    });
  }

  ngOnInit(): void {
  }

  private formatDateToDDMMYYYY(value: string | Date | null): string {
    if (!value) return '';
    const d = value instanceof Date ? value : new Date(value);
    if (isNaN(d.getTime())) return '';
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}-${month}-${year}`;
  }

  async save() {
    if (this.form.invalid) {
      this.statusMessage = 'Please fill required fields (EnteredOn and OpeningStockBoxesWawa).';
      return;
    }

    const raw = this.form.value;
    const payload: any = { ...raw };

    // convert EnteredOn (date input) to dd-MM-yyyy string
    payload.EnteredOn = this.formatDateToDDMMYYYY(raw.EnteredOn);

    // remove empty properties to keep payload minimal
    Object.keys(payload).forEach(k => {
      if (payload[k] === null || payload[k] === undefined || payload[k] === '') {
        delete payload[k];
      }
    });

    try {
      const res = await this.stockService.saveStock(payload).toPromise();
      this.statusMessage = 'Saved successfully';
    } catch (err: any) {
      this.statusMessage = err?.error?.message || err?.message || 'Save failed';
    }
  }

  async checkDateExists() {
    const entered = this.form.get('EnteredOn')?.value;
    if (!entered) {
      this.statusMessage = 'Select a date first';
      return;
    }
    const dateStr = this.formatDateToDDMMYYYY(entered);
    try {
      const exists = await this.stockService.checkExists(dateStr).toPromise();
      this.statusMessage = exists ? `Record exists for ${dateStr}` : `No record for ${dateStr}`;
    } catch (err: any) {
      this.statusMessage = err?.error?.message || 'Check failed';
    }
  }

  async checkAndSaveMissing(dateStr: string) {
    try {
      const exists = await this.stockService.checkExists(dateStr).toPromise();
      if (exists) {
        this.statusMessage = `Stock already exists for ${dateStr}`;
        return;
      }
      // convert dd-MM-yyyy to ISO date for input
      const parts = dateStr.split('-');
      if (parts.length === 3) {
        const iso = `${parts[2]}-${parts[1]}-${parts[0]}`;
        this.form.get('EnteredOn')?.setValue(iso);
      }
      await this.save();
    } catch (err: any) {
      this.statusMessage = 'Check & save failed';
    }
  }

  async load(id: number) {
    try {
      const res = await this.stockService.getById(id).toPromise();
      if (!res) {
        this.statusMessage = 'No data returned';
        return;
      }
      // backend DTO uses PascalCase; map case-insensitive
      const map = (key: string) => res[key] ?? res[key.charAt(0).toUpperCase() + key.slice(1)] ?? res[key.charAt(0).toLowerCase() + key.slice(1)];

      const entered = res.enteredOn ?? res.EnteredOn ?? '';
      if (entered) {
        const parts = entered.split('-');
        if (parts.length === 3) {
          this.form.get('EnteredOn')?.setValue(`${parts[2]}-${parts[1]}-${parts[0]}`);
        }
      }

      const patch: any = {};
      for (const ctrl of Object.keys(this.form.controls)) {
        if (ctrl === 'EnteredOn') continue; // already set
        const val = map(ctrl);
        if (val !== undefined) patch[ctrl] = val;
      }
      this.form.patchValue(patch);
      this.statusMessage = 'Loaded for edit';
    } catch (err: any) {
      this.statusMessage = 'Load failed';
    }
  }
}
