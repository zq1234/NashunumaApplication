// src/app/shared/components/user-profile/user-info-card/user-info-card.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../../services/modal.service';
import { ModalComponent } from '../../ui/modal/modal.component';
import { FormsModule } from '@angular/forms';
import { User } from '@core/models/auth.model';

@Component({
  selector: 'app-user-info-card',
  standalone: true,
  imports: [
    CommonModule,
    ModalComponent,
    FormsModule
  ],
  templateUrl: './user-info-card.component.html',
  styles: ``
})
export class UserInfoCardComponent implements OnInit {
  @Input() user: User | null = null;

  isOpen = false;
  editInfo = {
    fullName: '',
    email: '',
    phone: '',
    designation: '',
    bio: ''
  };

  constructor(public modal: ModalService) {}

  ngOnInit(): void {
    if (this.user) {
      this.populateEditForm();
    }
  }

  populateEditForm(): void {
    if (this.user) {
      this.editInfo = {
        fullName: this.user.fullName || this.user.firstName || '',
        email: this.user.email || '',
        phone: this.user.mobileNumber || '',
        designation: this.user.designation || '',
        bio: this.user.designation || ''
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

  getUserFullName(): string {
    if (!this.user) return 'User';
    return this.user.fullName || this.user.firstName || 'User';
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

  getUserRole(): string {
    if (!this.user) return '';
    return this.user.userType || this.user.roles?.[0] || 'User';
  }

  getUserStatus(): string {
    if (!this.user) return 'Inactive';
    return this.user.isActive ? 'Active' : 'Inactive';
  }

  getUserStatusClass(): string {
    if (!this.user) return 'status-inactive';
    return this.user.isActive ? 'status-active' : 'status-inactive';
  }

  handleSave(): void {
    console.log('Saving info changes...', this.editInfo);
    this.modal.closeModal();
  }
}