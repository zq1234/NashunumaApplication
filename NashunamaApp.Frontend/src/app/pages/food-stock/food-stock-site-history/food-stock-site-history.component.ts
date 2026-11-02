import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FoodStockService } from '@shared/services/food-stock.service';
import { FoodStock } from '@core/models/food-stock.model';
import { ApiResponse } from '@core/models/api-response.model';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';

@Component({
  selector: 'app-food-stock-site-history',
  standalone: true,
  imports: [CommonModule, PageBreadcrumbComponent, ButtonComponent, ModalComponent],
  templateUrl: './food-stock-site-history.component.html'
})
export class FoodStockSiteHistoryComponent implements OnInit {
  siteId = '';
  siteName = '';
  province = '';
  district = '';
  tehsil = '';
  records: FoodStock[] = [];
  selectedRecord: FoodStock | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private foodStockService: FoodStockService
  ) {}

  ngOnInit(): void {
    this.siteId = this.route.snapshot.paramMap.get('siteId') ?? '';
    this.route.queryParamMap.subscribe(params => {
      this.siteName = params.get('siteName') ?? '';
      this.province = params.get('province') ?? '';
      this.district = params.get('district') ?? '';
      this.tehsil = params.get('tehsil') ?? '';
    });

    if (!this.siteId) {
      this.error = 'No site ID provided.';
      return;
    }

    this.loadHistory();
  }

  loadHistory(): void {
    this.loading = true;
    this.error = null;
    this.foodStockService.getFoodStocksBySiteId(this.siteId).subscribe({
      next: (response: ApiResponse<FoodStock[]>) => {
        this.loading = false;
        if (response.isSuccess && response.data) {
          this.records = response.data.map(record => this.normalizeRecord(record));
          const firstRecord = this.records[0];
          this.siteName ||= firstRecord?.siteName ?? '';
          this.province ||= firstRecord?.province ?? '';
          this.district ||= firstRecord?.district ?? '';
          this.tehsil ||= firstRecord?.tehsil ?? '';
        } else {
          this.error = response.message || 'Failed to load food-stock history.';
        }
      },
      error: error => {
        this.loading = false;
        this.error = error?.message || 'Failed to load food-stock history.';
      }
    });
  }

  showRecord(record: FoodStock): void {
    this.selectedRecord = record;
  }

  closeRecord(): void {
    this.selectedRecord = null;
  }

  goBack(): void {
    this.router.navigate(['/foodstock']);
  }

  editRecord(record: FoodStock): void {
    if (record.id !== null && record.id !== undefined) {
      this.router.navigate(['/foodstock/edit', record.id]);
    }
  }

  formatDate(value: string | null | undefined): string {
    if (!value) return '-';
    const date = this.parseDate(value);
    return date ? date.toLocaleDateString('en-PK', { day: '2-digit', month: 'short', year: 'numeric' }) : value;
  }

  formatTime(value: string | null | undefined): string {
    if (!value) return '-';
    const date = this.parseDate(value);
    return date ? date.toLocaleTimeString('en-PK', { hour: '2-digit', minute: '2-digit' }) : value;
  }

  value(value: string | number | null | undefined): string {
    return value === null || value === undefined || value === '' ? '0' : String(value);
  }

  getRecordValue(record: FoodStock, group: string, product: string): string {
    const groupPrefix: Record<string, string> = {
      Received: 'received',
      Opening: 'opening',
      Distributed: 'distributed',
      Closing: 'closing'
    };
    const productSuffix: Record<string, string> = {
      'Mamta Boxes': 'StockBoxesMamta',
      'Wawa Boxes': 'StockBoxesWawa',
      'Mamta Sachets': 'StockSachetsMamta',
      'Wawa Sachets': 'StockSachetsWawa',
      RUTF: 'Rutf',
      IFA: 'Ifa'
    };
    const prefix = groupPrefix[group];
    const suffix = productSuffix[product];
    if (!prefix || !suffix) return '0';

    const field = product === 'RUTF'
      ? `rutf${group}`
      : product === 'IFA'
        ? `ifa${group}`
        : `${prefix}${suffix}`;
    return this.value((record as any)[field]);
  }

  private normalizeRecord(source: FoodStock): FoodStock {
    const row = source as any;
    const read = (...keys: string[]) => {
      for (const key of keys) {
        if (row[key] !== undefined && row[key] !== null) return row[key];
      }
      return null;
    };

    return {
      ...source,
      id: read('id', 'Id'),
      siteId: String(read('siteId', 'SiteId') ?? this.siteId),
      siteName: read('siteName', 'SiteName'),
      province: read('province', 'Province'),
      district: read('district', 'District'),
      tehsil: read('tehsil', 'Tehsil'),
      enteredBy: read('enteredBy', 'EnteredBy', 'ENTERED_BY'),
      enteredOn: read('enteredOn', 'EnteredOn', 'ENTERED_ON'),
      activityTime: read('activityTime', 'ActivityTime', 'Activity_Time'),
      remarks: read('remarks', 'Remarks', 'REMARKS'),
      receivedStockBoxesMamta: read('receivedStockBoxesMamta', 'ReceivedStockBoxesMamta', 'RECEIVED_STOCK_BOXES_MAMTA'),
      receivedStockBoxesWawa: read('receivedStockBoxesWawa', 'ReceivedStockBoxesWawa', 'RECEIVED_STOCK_BOXES_WAWA'),
      openingStockBoxesMamta: read('openingStockBoxesMamta', 'OpeningStockBoxesMamta', 'OPENING_STOCK_BOXES_MAMTA'),
      openingStockBoxesWawa: read('openingStockBoxesWawa', 'OpeningStockBoxesWawa', 'OPENING_STOCK_BOXES_WAWA'),
      distributedBoxesMamta: read('distributedBoxesMamta', 'DistributedBoxesMamta', 'DISTRIBUTED_BOXES_MAMTA'),
      distributedBoxesWawa: read('distributedBoxesWawa', 'DistributedBoxesWawa', 'DISTRIBUTED_BOXES_WAWA'),
      closingStockBoxesMamta: read('closingStockBoxesMamta', 'ClosingStockBoxesMamta', 'CLOSING_STOCK_BOXES_MAMTA'),
      closingStockBoxesWawa: read('closingStockBoxesWawa', 'ClosingStockBoxesWawa', 'CLOSING_STOCK_BOXES_WAWA'),
      openingStockSachetsMamta: read('openingStockSachetsMamta', 'OpeningStockSachetsMamta', 'OPENING_STOCK_SACHETS_MAMTA'),
      receivedStockSachetsMamta: read('receivedStockSachetsMamta', 'ReceivedStockSachetsMamta', 'RECEIVED_STOCK_SACHETS_MAMTA'),
      distributedSachetsMamta: read('distributedSachetsMamta', 'DistributedSachetsMamta', 'DISTRIBUTED_SACHETS_MAMTA'),
      closingStockSachetsMamta: read('closingStockSachetsMamta', 'ClosingStockSachetsMamta', 'CLOSING_STOCK_SACHETS_MAMTA'),
      openingStockSachetsWawa: read('openingStockSachetsWawa', 'OpeningStockSachetsWawa', 'OPENING_STOCK_SACHETS_WAWA'),
      receivedStockSachetsWawa: read('receivedStockSachetsWawa', 'ReceivedStockSachetsWawa', 'RECEIVED_STOCK_SACHETS_WAWA'),
      distributedSachetsWawa: read('distributedSachetsWawa', 'DistributedSachetsWawa', 'DISTRIBUTED_SACHETS_WAWA'),
      closingStockSachetsWawa: read('closingStockSachetsWawa', 'ClosingStockSachetsWawa', 'CLOSING_STOCK_SACHETS_WAWA'),
      rutfReceived: read('rutfReceived', 'RutfReceived', 'RUTF_RECEIVED'),
      rutfOpening: read('rutfOpening', 'RutfOpening', 'RUTF_OPENING'),
      rutfDistributed: read('rutfDistributed', 'RutfDistributed', 'RUTF_DISTRIBUTED'),
      rutfClosing: read('rutfClosing', 'RutfClosing', 'RUTF_CLOSING'),
      ifaReceived: read('ifaReceived', 'IfaReceived', 'IFA_RECEIVED'),
      ifaOpening: read('ifaOpening', 'IfaOpening', 'IFA_OPENING'),
      ifaDistributed: read('ifaDistributed', 'IfaDistributed', 'IFA_DISTRIBUTED'),
      ifaClosing: read('ifaClosing', 'IfaClosing', 'IFA_CLOSING'),
      mmsReceived: read('mmsReceived', 'MmsReceived', 'MMS_RECEIVED'),
      mmsOpening: read('mmsOpening', 'MmsOpening', 'MMS_OPENING'),
      mmsDistributed: read('mmsDistributed', 'MmsDistributed', 'MMS_DISTRIBUTED'),
      mmsClosing: read('mmsClosing', 'MmsClosing', 'MMS_CLOSING')
    };
  }

  private parseDate(value: string): Date | null {
    const dayFirst = value.match(/^(\d{1,2})[-/](\d{1,2})[-/](\d{4})(?:[T\s](.*))?$/);
    if (dayFirst) {
      const [, day, month, year, time = '00:00:00'] = dayFirst;
      const date = new Date(`${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}T${time}`);
      return Number.isNaN(date.getTime()) ? null : date;
    }
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }
}