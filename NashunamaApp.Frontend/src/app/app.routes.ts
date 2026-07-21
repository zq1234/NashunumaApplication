// src/app/app.routes.ts

import { Routes } from '@angular/router';
import { AppLayoutComponent } from './shared/layout/app-layout/app-layout.component';

import { SignInComponent } from './pages/auth-pages/sign-in/sign-in.component';
import { NotFoundComponent } from './pages/other-page/not-found/not-found.component';

import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // ==========================
  // Authentication
  // ==========================
  {
    path: 'signin',
    component: SignInComponent,
    title: 'Sign In | BISPAdmin'
  },
  {
    path: 'signup',
    loadComponent: () =>
      import('./pages/auth-pages/sign-up/sign-up.component').then(
        m => m.SignUpComponent
      ),
    title: 'Sign Up | BISPAdmin'
  },

  // ==========================
  // Protected Routes
  // ==========================
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [AuthGuard],

    children: [
      // Default - Now redirects to users instead of dashboard
      {
        path: '',
        redirectTo: 'users',  
        pathMatch: 'full'
      },

      // Users - This will be the default page
      {
        path: 'users',
        loadComponent: () =>
          import('./pages/users/user-list/user-list.component').then(
            m => m.UserListComponent
          ),
        title: 'Users | BISPAdmin'
      },

      // Food Stock
      {
        path: 'foodstock',
        children: [
          {
            path: '',
            loadComponent: () =>
              import(
                './pages/food-stock/food-stock-list/food-stock-list.component'
              ).then(m => m.FoodStockListComponent),
            title: 'Food Stock'
          },
          {
            path: ':id',
            loadComponent: () =>
              import(
                './pages/food-stock/food-stock-detail/food-stock-detail.component'
              ).then(m => m.FoodStockDetailComponent),
            title: 'Food Stock Details'
          }
        ]
      },

      // Profile
      {
        path: 'profile',
        loadComponent: () =>
          import('./pages/profile/profile.component').then(
            m => m.ProfileComponent
          ),
        title: 'Profile'
      },

      // Forms
      {
        path: 'forms',
        loadComponent: () =>
          import('./pages/forms/form-elements/form-elements.component').then(
            m => m.FormElementsComponent
          ),
        title: 'Forms'
      },

      // Tables
      {
        path: 'tables',
        loadComponent: () =>
          import('./pages/tables/basic-tables/basic-tables.component').then(
            m => m.BasicTablesComponent
          ),
        title: 'Tables'
      },

      // Blank
      {
        path: 'blank',
        loadComponent: () =>
          import('./pages/blank/blank.component').then(
            m => m.BlankComponent
          ),
        title: 'Blank'
      },

      // Child 404 - Redirect to users instead of dashboard
      {
        path: '**',
        redirectTo: 'users'   
      }
    ]
  },

  // ==========================
  // Global 404
  // ==========================
  {
    path: '404',
    component: NotFoundComponent,
    title: 'Page Not Found'
  },

  {
    path: '**',
    redirectTo: '404'
  }
];