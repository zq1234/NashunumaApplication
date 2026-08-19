import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@shared/services/auth.service';
import { Subscription } from 'rxjs';
import { MissingStockNotificationDto } from '@core/models/missing-stock.model';
import { RouterModule, Router } from '@angular/router';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';

@Component({
  selector: 'app-missing-stock-notification',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ModalComponent, ButtonComponent],
  templateUrl: './missing-stock-notification.component.html',
  styleUrls: ['./missing-stock-notification.component.scss']
})
export class MissingStockNotificationComponent implements OnInit, OnDestroy {
  notification: MissingStockNotificationDto | null = null;
  private sub: Subscription | null = null;
  openingDefault: string = '0';
  processing: boolean = false;
  progress: { date: string; status: 'pending' | 'success' | 'failed'; message?: string }[] = [];

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.sub = this.auth.missingStockNotification$.subscribe(n => {
      this.notification = n;
    });
  }

  get canShowPopup(): boolean {
    return !!this.notification?.hasMissingEntries && !this.auth.isMissingStockPopupPaused();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private hidePopup(): void {
    this.notification = null;
  }

  navigateToFoodStockForm(date?: string): void {
    if (!date) return;
    this.hidePopup();
    this.router.navigate(['/foodstock/create'], { queryParams: { date } });
  }

  enterMissing(): void {
    // Navigate to create page with first missing date as query param.
    // Hide the popup UI only; do not clear the shared notification state.
    if (this.notification && this.notification.missingDates.length > 0) {
      const dates = this.notification.missingDates.map(d => d.date);
      this.auth.startMissingSequence(dates);
      const next = this.auth.popNextMissingDate();
      if (next) {
        this.navigateToFoodStockForm(next);
      }
      this.hidePopup();
    }
  }

  async autoEnterAll(): Promise<void> {
    if (!this.notification || !this.notification.missingDates || this.notification.missingDates.length === 0) return;
    this.hidePopup();
    // confirm when auto-entering multiple dates
    if (this.notification.missingDates.length > 1) {
      const confirmed = window.confirm(`You are about to auto-enter ${this.notification.missingDates.length} missing dates. This will create minimal stock entries using default Opening Boxes (Wawa) = ${this.openingDefault}. Continue?`);
      if (!confirmed) return;
    }
    this.processing = true;
    this.progress = [];

    for (const d of this.notification.missingDates) {
      const date = d.date;
      this.progress.push({ date, status: 'pending' });
      try {
        const res = await this.auth.saveMissingDate(date, this.openingDefault);
        const idx = this.progress.findIndex(p => p.date === date);
        if (res && res.ok) {
          if (idx !== -1) this.progress[idx].status = 'success';
          if (idx !== -1) this.progress[idx].message = res.message;
        } else {
          if (idx !== -1) this.progress[idx].status = 'failed';
          if (idx !== -1) this.progress[idx].message = res?.message || 'Save failed';
          // stop further processing on failure
          break;
        }
      } catch (err: any) {
        const idx = this.progress.findIndex(p => p.date === date);
        if (idx !== -1) {
          this.progress[idx].status = 'failed';
          this.progress[idx].message = err?.message || String(err);
        }
        break;
      }
    }

    this.processing = false;
    // Refresh missing dates state after processing
    this.auth.refreshMissingDates();
  }

  dismiss(): void {
    // Hide the popup only; keep the pending list data for the sidebar/header.
    this.hidePopup();
  }

  pauseForHours(hours: number | string): void {
    const safeHours = Math.max(1, Math.min(24, Number(hours) || 1));
    this.auth.pauseMissingNotification(safeHours);
    this.hidePopup();
  }

  quickEnter(date: string): void {
    if (!date) return;
    // Attempt quick save directly from modal
    this.hidePopup();
    this.progress = [];
    this.processing = true;
    this.progress.push({ date, status: 'pending' });
    this.auth.saveMissingDate(date, this.openingDefault).then(res => {
      const p = this.progress.find(p => p.date === date);
      if (p) {
        p.status = res && res.ok ? 'success' : 'failed';
        p.message = res?.message;
      }
      this.processing = false;
      this.auth.refreshMissingDates();
    }).catch(err => {
      const p = this.progress.find(p => p.date === date);
      if (p) { p.status = 'failed'; p.message = err?.message || String(err); }
      this.processing = false;
    });
  }
}
