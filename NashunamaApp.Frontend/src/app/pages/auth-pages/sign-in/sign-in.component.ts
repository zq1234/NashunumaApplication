// src/app/pages/auth/sign-in/sign-in.component.ts
import { Component } from '@angular/core';
import { SigninFormComponent } from '../../../shared/components/auth/signin-form/signin-form.component';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [SigninFormComponent],
 template: `
    <div class="auth-page">
      <div class="auth-container">
        <div class="auth-card">
          <app-signin-form />
        </div>
      </div>
    </div>
  `,
     styles: [`@import '../auth-page.scss'`]
})
export class SignInComponent {}