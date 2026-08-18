// src/app/pages/profile/profile.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ComponentCardComponent } from '../../shared/components/common/component-card/component-card.component';
import { UserMetaCardComponent } from '../../shared/components/user-profile/user-meta-card/user-meta-card.component';
import { UserInfoCardComponent } from '../../shared/components/user-profile/user-info-card/user-info-card.component';
import { UserAddressCardComponent } from '../../shared/components/user-profile/user-address-card/user-address-card.component';
import { AuthService } from '@shared/services/auth.service';
import { User } from '@core/models/auth.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    PageBreadcrumbComponent,
    ComponentCardComponent,
    UserMetaCardComponent,
    UserInfoCardComponent,
    UserAddressCardComponent
  ],
  templateUrl: './profile.component.html',
  styles: ``
})
export class ProfileComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  private destroy$ = new Subject<void>();

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });

    // If user is not loaded, try to fetch profile
    if (!this.currentUser) {
      this.authService.getProfile().subscribe();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}