import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@shared/services/auth.service';
import { Subscription } from 'rxjs';
import { MissingStockNotificationDto } from '@core/models/missing-stock.model';
import { RouterModule, Router } from '@angular/router';
import { ModalComponent } from '@shared/components/ui/modal/modal.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';

@Component({
  selector: 'app-missing-stock-notification',
  standalone: true,
  imports: [CommonModule, RouterModule, ModalComponent, ButtonComponent],
  templateUrl: './missing-stock-notification.component.html',
  styleUrls: ['./missing-stock-notification.component.scss']
})
export class MissingStockNotificationComponent implements OnInit, OnDestroy {
  notification: MissingStockNotificationDto | null = null;
  private sub: Subscription | null = null;

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

  enterMissing(): void {
    // Navigate to create page with first missing date as query param.
    // Hide the popup UI only; do not clear the shared notification state.
    if (this.notification && this.notification.missingDates.length > 0) {
      const dates = this.notification.missingDates.map(d => d.date);
      this.auth.startMissingSequence(dates);
      const next = this.auth.popNextMissingDate();
      if (next) {
        this.router.navigate(['/foodstock/create'], { queryParams: { date: next } });
      }
      this.notification = null;
    }
  }

  dismiss(): void {
    // Hide the popup only; keep the pending list data for the sidebar/header.
    this.notification = null;
  }

  pauseForHours(hours: number | string): void {
    const safeHours = Math.max(1, Math.min(24, Number(hours) || 1));
    this.auth.pauseMissingNotification(safeHours);
    this.notification = null;
  }

  quickEnter(date: string): void {
    if (!date) return;
    this.notification = null;
    this.router.navigate(['/foodstock/create'], { queryParams: { date } });
  }
}
