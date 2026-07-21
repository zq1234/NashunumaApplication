// src/app/shared/components/auth/signin-form/signin-form.component.ts
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '@shared/services/auth.service';
import { LoginDto } from '@core/models/auth.model';


@Component({
  selector: 'app-signin-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './signin-form.component.html',
  styles: ``
})
export class SigninFormComponent {
  email = '';
  password = '';
  isLoading = false;
  errorMessage = '';
  showPassword = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSignIn() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter both email and password';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const loginDto: LoginDto = {
      email: this.email,
      password: this.password
    };

    this.authService.login(loginDto).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess && response.data) {
          // Navigate to dashboard after successful login
          this.router.navigate(['/dashboard']);
        } else {
          this.errorMessage = response.message || 'Login failed. Please try again.';
          if (response.errors && response.errors.length > 0) {
            this.errorMessage = response.errors.join(', ');
          }
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Login failed. Please try again.';
      }
    });
  }
}