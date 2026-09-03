import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StateService } from './state.service';
import { SessionTimeoutService } from './session-timeout.service';
import { REFRESH_TOKEN_KEY, TOKEN_KEY } from '../constants/session.constants';

export interface LoginResult {
  success: boolean;
  error?: string;
}

interface TokenResponse {
  access_token?: string;
  refresh_token?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private state = inject(StateService);
  private session = inject(SessionTimeoutService);

  private readonly tokenKey = TOKEN_KEY;
  private refreshInFlight?: Promise<boolean>;

  async login(email: string, password: string): Promise<LoginResult> {
    const url = `${environment.apis.default.url}/connect/token`;
    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: 'ERPPlatform_App',
      username: email,
      password: password,
      scope: 'offline_access ERPPlatform'
    }).toString();

    const headers = this.formHeaders();

    try {
      const response = await firstValueFrom(
        this.http.post<TokenResponse>(url, body, { headers })
      );
      if (response && response.access_token) {
        await this.completeLogin(response);
        return { success: true };
      }
      return { success: false, error: 'Invalid authentication response received.' };
    } catch (error: any) {
      console.error('Login failed', error);
      if (error.status === 0) {
        return {
          success: false,
          error: 'Unable to connect to backend server. Please verify HttpApi.Host is running.'
        };
      }
      if (error.error?.error_description) {
        return { success: false, error: error.error.error_description };
      }
      return { success: false, error: 'Invalid email or password. Please try again.' };
    }
  }

  /**
   * Exchanges the refresh token for a new access token.
   * Lets a mobile session stay alive for its full 6 months even though the
   * server access token itself is short lived (~30 min).
   */
  refreshSession(): Promise<boolean> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      return Promise.resolve(false);
    }
    // Share one request between all callers hitting a 401 at the same time.
    if (!this.refreshInFlight) {
      this.refreshInFlight = this.requestNewAccessToken(refreshToken).finally(() => {
        this.refreshInFlight = undefined;
      });
    }
    return this.refreshInFlight;
  }

  private async requestNewAccessToken(refreshToken: string): Promise<boolean> {
    const url = `${environment.apis.default.url}/connect/token`;
    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      client_id: 'ERPPlatform_App',
      refresh_token: refreshToken
    }).toString();

    try {
      const response = await firstValueFrom(
        this.http.post<TokenResponse>(url, body, { headers: this.formHeaders() })
      );
      if (response?.access_token) {
        this.storeTokens(response);
        return true;
      }
      return false;
    } catch (error) {
      console.warn('Session refresh failed', error);
      return false;
    }
  }

  logout(): void {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    this.clearTokens();
    this.session.stop();
    this.revokeOnServer(refreshToken);
    this.state.loadAppConfig();
    this.router.navigateByUrl('/auth/login');
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /** Authenticated = a token exists AND the session lifetime is not over. */
  isAuthenticated(): boolean {
    return !!this.getToken() && !this.session.isExpired();
  }

  private async completeLogin(response: TokenResponse): Promise<void> {
    this.storeTokens(response);
    // Starts the session clock: 3h on desktop, 6 months on phones/tablets.
    this.session.start();
    try {
      await Promise.race([
        this.state.loadAppConfig(),
        new Promise((_, reject) => setTimeout(() => reject(new Error('Timeout loading app configuration')), 8000))
      ]);
    } catch (err) {
      console.warn('App config load warning during login:', err);
    }
  }

  private storeTokens(response: TokenResponse): void {
    if (response.access_token) {
      localStorage.setItem(this.tokenKey, response.access_token);
    }
    if (response.refresh_token) {
      localStorage.setItem(REFRESH_TOKEN_KEY, response.refresh_token);
    }
  }

  private clearTokens(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  /** Best effort: tells the server to invalidate the refresh token on logout. */
  private revokeOnServer(refreshToken: string | null): void {
    if (!refreshToken) {
      return;
    }
    const body = new URLSearchParams({
      token: refreshToken,
      token_type_hint: 'refresh_token',
      client_id: 'ERPPlatform_App'
    }).toString();

    this.http
      .post(`${environment.apis.default.url}/connect/revocation`, body, { headers: this.formHeaders() })
      .subscribe({ error: () => undefined });
  }

  private formHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });
  }
}
