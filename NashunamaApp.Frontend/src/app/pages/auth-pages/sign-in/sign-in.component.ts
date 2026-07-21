// src/app/pages/auth/sign-in/sign-in.component.ts
import { Component } from '@angular/core';
import { SigninFormComponent } from '../../../shared/components/auth/signin-form/signin-form.component';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [SigninFormComponent],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div class="flex items-center justify-center min-h-screen p-4">
        <div class="w-full max-w-md">
          <app-signin-form />
        </div>
      </div>
    </div>
  `,
  styles: ``
})
export class SignInComponent {}