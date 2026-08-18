import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { MissingStockNotificationDto } from '@core/models/missing-stock.model';
import { AuthService } from '@shared/services/auth.service';
import { DropdownComponent } from '../../ui/dropdown/dropdown.component';
import { DropdownItemComponent } from '../../ui/dropdown/dropdown-item/dropdown-item.component';

@Component({
  selector: 'app-notification-dropdown',
  templateUrl: './notification-dropdown.component.html',
  imports:[CommonModule,RouterModule,DropdownComponent,DropdownItemComponent]
})
export class NotificationDropdownComponent implements OnInit, OnDestroy {
  isOpen = false;
  notifying = false;
  notification: MissingStockNotificationDto | null = null;
  private sub: Subscription | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.sub = this.auth.missingStockNotification$.subscribe(value => {
      this.notification = value;
      this.notifying = !!value?.hasMissingEntries;
    });
  }

  get pendingCount(): number {
    return this.missingDates.length;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  toggleDropdown() {
    this.isOpen = !this.isOpen;
  }

  closeDropdown() {
    this.isOpen = false;
  }

  openMissingDate(date: string): void {
    if (!date) return;
    this.auth.acknowledgeMissingNotification();
    this.closeDropdown();
    this.router.navigate(['/foodstock/create'], { queryParams: { date } });
  }

  dismissNotification(): void {
    // Close only the dropdown UI; keep the shared pending task data intact.
    this.closeDropdown();
  }

  get missingDates() {
    return this.notification?.missingDates ?? [];
  }
}