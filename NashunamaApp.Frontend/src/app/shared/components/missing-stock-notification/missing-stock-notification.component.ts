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

  private hidePopup(): void {
    this.notification = null;
  }

  navigateToFoodStockForm(date?: string): void {
    if (!date) return;
    this.hidePopup();
    this.router.navigate(['/foodstock/create'], { queryParams: { date } });
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

}
