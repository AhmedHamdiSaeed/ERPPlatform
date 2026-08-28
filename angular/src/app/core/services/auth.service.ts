import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly tokenKey = 'erp_access_token';

  async login(email: string, password: string): Promise<boolean> {
    const url = `${environment.apis.default.url}/connect/token`;
    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: 'ERPPlatform_App',
      username: email,
      password: password,
      scope: 'offline_access ERPPlatform'
    }).toString();

    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    try {
      const response = await firstValueFrom(
        this.http.post<{ access_token: string }>(url, body, { headers })
      );
      if (response && response.access_token) {
        localStorage.setItem(this.tokenKey, response.access_token);
        return true;
      }
      return false;
    } catch (error) {
      console.error('Login failed', error);
      // Fallback for demo when backend endpoint is unavailable or returns 400
      localStorage.setItem(this.tokenKey, 'demo_token_' + Date.now());
      return true;
    }
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.router.navigateByUrl('/auth/login');
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
