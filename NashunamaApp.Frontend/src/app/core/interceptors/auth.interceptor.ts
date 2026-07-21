// src/app/core/interceptors/auth.interceptor.ts
import { Injectable } from '@angular/core';
import { 
  HttpRequest, 
  HttpHandler, 
  HttpEvent, 
  HttpInterceptor, 
  HttpErrorResponse 
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '@shared/services/auth.service';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Skip token for auth endpoints
    const authUrls = [
      environment.auth.loginUrl,
      environment.auth.registerUrl,
      environment.auth.refreshTokenUrl,
      environment.auth.resetPasswordUrl
    ];

    if (authUrls.some(url => request.url.includes(url))) {
      return next.handle(request);
    }

    const token = this.authService.getToken();

    if (token) {
      request = this.addTokenToRequest(request, token);
    }

    return next.handle(request).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private addTokenToRequest(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshToken = this.authService.getRefreshToken();
      
      if (refreshToken) {
        return this.authService.refreshToken({ refreshToken }).pipe(
          switchMap(response => {
            this.isRefreshing = false;
            if (response.isSuccess && response.data) {
              this.refreshTokenSubject.next(response.data.accessToken);
              return next.handle(this.addTokenToRequest(request, response.data.accessToken));
            }
            return this.handleLogout();
          }),
          catchError(error => {
            this.isRefreshing = false;
            return this.handleLogout();
          })
        );
      }
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => next.handle(this.addTokenToRequest(request, token!)))
    );
  }

  private handleLogout(): Observable<never> {
    this.authService.logout().subscribe();
    return throwError(() => new Error('Session expired'));
  }
}