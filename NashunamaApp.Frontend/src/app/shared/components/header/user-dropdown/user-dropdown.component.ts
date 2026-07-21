// src/app/shared/components/header/user-dropdown/user-dropdown.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DropdownComponent } from '../../ui/dropdown/dropdown.component';
import { DropdownItemTwoComponent } from '../../ui/dropdown/dropdown-item/dropdown-item.component-two';
import { AuthService } from '@shared/services/auth.service';
import { User } from '@core/models/auth.model';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-user-dropdown',
  templateUrl: './user-dropdown.component.html',
  styleUrls: ['./user-dropdown.component.scss'],
  imports: [CommonModule, RouterModule, DropdownComponent, DropdownItemTwoComponent]
})
export class UserDropdownComponent implements OnInit, OnDestroy {
  isOpen = false;
  currentUser: User | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  closeDropdown(): void {
    this.isOpen = false;
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/signin']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Even if logout fails on server, clear local session
        // Use the public method if available, or handle differently
        this.router.navigate(['/signin']);
      }
    });
    this.closeDropdown();
  }

  getUserInitials(): string {
    if (!this.currentUser) return 'U';
    const name = this.currentUser.fullName || this.currentUser.firstName || this.currentUser.username || '';
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  getUserDisplayName(): string {
    if (!this.currentUser) return 'User';
    return this.currentUser.fullName || this.currentUser.firstName || this.currentUser.username || 'User';
  }

  getUserEmail(): string {
    if (!this.currentUser) return '';
    return this.currentUser.email || '';
  }

  getUserRole(): string {
    if (!this.currentUser) return '';
    return this.currentUser.userType || this.currentUser.roles?.[0] || 'User';
  }

  getUserAvatar(): string {
    // You can add avatar logic here if needed
    return '/images/user/avatar.png';
  }
}