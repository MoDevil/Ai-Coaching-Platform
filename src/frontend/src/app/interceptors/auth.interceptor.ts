import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.authService.logout();
        } else if (error.status === 0) {
          // Network error — server unreachable
          const banner = document.getElementById('_global_error_banner');
          if (banner) {
            banner.textContent = 'Cannot reach server. Check your connection or restart the backend.';
            banner.style.display = 'block';
            setTimeout(() => { banner.style.display = 'none'; }, 6000);
          }
        } else if (error.status >= 500) {
          const banner = document.getElementById('_global_error_banner');
          if (banner) {
            banner.textContent = 'Server error. Please try again. If this persists, restart the backend.';
            banner.style.display = 'block';
            setTimeout(() => { banner.style.display = 'none'; }, 6000);
          }
        }
        return throwError(() => error);
      })
    );
  }
}
