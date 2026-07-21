// src/app/shared/components/user-profile/user-address-card/user-address-card.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../../services/modal.service';
import { ModalComponent } from '../../ui/modal/modal.component';
import { FormsModule } from '@angular/forms';
import { User } from '@core/models/auth.model';

@Component({
  selector: 'app-user-address-card',
  standalone: true,
  imports: [
    CommonModule,
    ModalComponent,
    FormsModule
  ],
  templateUrl: './user-address-card.component.html',
  styles: ``
})
export class UserAddressCardComponent implements OnInit {
  @Input() user: User | null = null;

  isOpen = false;
  editAddress = {
    province: '',
    district: '',
    tehsil: '',
    siteName: '',
    siteAddress: ''
  };

  constructor(public modal: ModalService) {}

  ngOnInit(): void {
    if (this.user) {
      this.populateEditForm();
    }
  }

  populateEditForm(): void {
    if (this.user) {
      this.editAddress = {
        province: this.user.province || '',
        district: this.user.district || '',
        tehsil: this.user.tehsil || '',
        siteName: this.user.siteName || '',
        siteAddress: (this.user as any).siteAddress || ''
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

  getProvince(): string {
    return this.user?.province || 'Not set';
  }

  getDistrict(): string {
    return this.user?.district || 'Not set';
  }

  getTehsil(): string {
    return this.user?.tehsil || 'Not set';
  }

  getSiteName(): string {
    return this.user?.siteName || 'Not assigned';
  }

  getSiteAddress(): string {
    return (this.user as any)?.siteAddress || 'No address available';
  }

  getFullLocation(): string {
    if (!this.user) return 'No location set';
    const parts = [this.user.province, this.user.district, this.user.tehsil].filter(Boolean);
    return parts.length > 0 ? parts.join(', ') : 'No location set';
  }

  handleSave(): void {
    console.log('Saving address changes...', this.editAddress);
    this.modal.closeModal();
  }
}