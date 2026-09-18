import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '@shared/services/auth.service';
import { SidebarService } from '../../services/sidebar.service';
import { CommonModule } from '@angular/common';
import { AppSidebarComponent } from '../app-sidebar/app-sidebar.component';
import { BackdropComponent } from '../backdrop/backdrop.component';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { MissingStockNotificationComponent } from '@shared/components/missing-stock-notification/missing-stock-notification.component';
import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { AppHeaderComponent } from '../app-header/app-header.component';

@Component({
  selector: 'app-layout',
  imports: [
    CommonModule,
    RouterModule,
    MissingStockNotificationComponent,
    AppHeaderComponent,
    AppSidebarComponent,
    BackdropComponent,
    ComponentCardComponent
  ],
  templateUrl: './app-layout.component.html',
})

export class AppLayoutComponent implements OnInit, OnDestroy {
  readonly isExpanded$;
  readonly isHovered$;
  readonly isMobileOpen$;

  constructor(
    public sidebarService: SidebarService,
    private authService: AuthService,
    private router: Router
  ) {
    this.isExpanded$ = this.sidebarService.isExpanded$;
    this.isHovered$ = this.sidebarService.isHovered$;
    this.isMobileOpen$ = this.sidebarService.isMobileOpen$;
  }

  ngOnInit(): void {
    // Load the missing-date reminder once at layout initialization and again after route changes.
    // A dismissed reminder remains dismissed for the current session.
    this.authService.refreshMissingDates();

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.authService.refreshMissingDates();
      }
    });
  }

  ngOnDestroy(): void {
  }

  get containerClasses() {
    return [
      'flex-1',
      'transition-all',
      'duration-300',
      'ease-in-out',
      (this.isExpanded$ || this.isHovered$) ? 'xl:ml-[290px]' : 'xl:ml-[90px]',
      this.isMobileOpen$ ? 'ml-0' : ''
    ];
  }

}
