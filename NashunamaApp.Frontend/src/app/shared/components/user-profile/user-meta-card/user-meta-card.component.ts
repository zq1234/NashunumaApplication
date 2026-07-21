// src/app/shared/components/user-profile/user-meta-card/user-meta-card.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../../services/modal.service';
import { ModalComponent } from '../../ui/modal/modal.component';
import { FormsModule } from '@angular/forms';
import { User } from '@core/models/auth.model';
import { AuthService } from '@shared/services/auth.service';
import { UserHelper } from '@core/models/user.model';

@Component({
  selector: 'app-user-meta-card',
  standalone: true,
  imports: [
    CommonModule,
    ModalComponent,
     
    FormsModule
  ],
  templateUrl: './user-meta-card.component.html',
  styles: ``
})
export class UserMetaCardComponent implements OnInit {
  @Input() user: User | null = null;

  isOpen = false;
  editUser = {
    fullName: '',
    email: '',
    phone: '',
    designation: '',
    userType: ''
  };

  constructor(
    public modal: ModalService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    if (this.user) {
      this.populateEditForm();
    }
  }

  populateEditForm(): void {
    if (this.user) {
      this.editUser = {
        fullName: this.user.fullName || this.user.firstName || '',
        email: this.user.email || '',
        phone: this.user.mobileNumber || '',
        designation: this.user.designation || '',
        userType: this.user.userType || this.user.roles?.[0] || ''
      };
    }
  }

  openModal(): void {
    this.populateEditForm();
    this.isOpen = true;
  }

  closeModal(): void {
    this.isOpen = false;
  }

  getUserInitials(): string {
    if (!this.user) return 'U';
    return UserHelper.getInitials ? UserHelper.getInitials(this.user as any) : 
      (this.user.fullName?.charAt(0) || this.user.firstName?.charAt(0) || 'U');
  }

  getUserFullName(): string {
    if (!this.user) return 'User';
    return this.user.fullName || this.user.firstName || 'User';
  }

  getUserRole(): string {
    if (!this.user) return '';
    return this.user.userType || this.user.roles?.[0] || 'User';
  }

  getUserLocation(): string {
    if (!this.user) return '';
    const parts = [this.user.province, this.user.district, this.user.tehsil].filter(Boolean);
    return parts.length > 0 ? parts.join(', ') : 'No location set';
  }

  getUserEmail(): string {
    return this.user?.email || '';
  }

  getUserPhone(): string {
    return this.user?.mobileNumber || '';
  }

  getUserDesignation(): string {
    return this.user?.designation || '';
  }

  handleSave(): void {
    // Here you would call an API to update user profile
    console.log('Saving changes...', this.editUser);
    
    // Example of updating profile
    // this.authService.updateProfile(this.editUser).subscribe({
    //   next: (response: ApiResponse<User>) => {
    //     if (response.isSuccess) {
    //       this.modal.closeModal();
    //       // Show success message
    //     }
    //   },
    //   error: (error) => {
    //     console.error('Error updating profile:', error);
    //   }
    // });
    
    this.modal.closeModal();
  }
}