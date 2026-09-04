import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StateService } from './state.service';
import { SessionTimeoutService } from './session-timeout.service';
import { REFRESH_TOKEN_KEY, TOKEN_KEY } from '../constants/session.constants';

export const TENANT_KEY = '__tenant';

export interface LoginResult {
  success: boolean;
  error?: string;
}

export interface ApiResult<T> {
  success: boolean;
  message?: string;
  statusCode: number;
  errors?: string[];
  data?: T;
}

export interface LoginResponseData {
  token: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
  user: {
    id: string;
    userName?: string;
    name?: string;
    surname?: string;
    email?: string;
    phoneNumber?: string;
    roles: string[];
    permissions: string[];
    tenantId?: string;
    tenantName?: string;
    logoUrl?: string;
  };
}

export interface RefreshResponseData {
  token: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
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

  constructor() {
    this.extractTenantFromUrl();
  }

  /**
   * Automatically extracts tenant name from URL query parameters (?tenant=... or ?__tenant=...)
   * or from subdomain (e.g. acme.erpplatform.com).
   */
  extractTenantFromUrl(): string | null {
    if (typeof window === 'undefined') return null;

    try {
      const params = new URLSearchParams(window.location.search);
      const tenantParam = params.get('tenant') || params.get('__tenant') || params.get('tenantName');
      if (tenantParam && tenantParam.trim()) {
        this.setTenant(tenantParam.trim());
        return tenantParam.trim();
      }

      // Check subdomain if not localhost/ip
      const host = window.location.hostname;
      const parts = host.split('.');
      if (parts.length > 2 && !host.includes('localhost') && !/^\d+\.\d+\.\d+\.\d+$/.test(host)) {
        const subdomain = parts[0];
        if (subdomain && subdomain !== 'www' && subdomain !== 'app') {
          this.setTenant(subdomain);
          return subdomain;
        }
      }
    } catch {
      /* ignore SSR or private browsing errors */
    }

    return this.getTenant();
  }

  getTenant(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem(TENANT_KEY);
  }

  setTenant(tenantName: string): void {
    if (typeof localStorage !== 'undefined') {
      if (tenantName) {
        localStorage.setItem(TENANT_KEY, tenantName.trim());
      } else {
        localStorage.removeItem(TENANT_KEY);
      }
    }
  }

  /**
   * Universal Login method for Web and Mobile:
   * Sends email/username/phone, password, and tenant (from URL/storage) to /api/auth/login.
   */
  async login(loginIdentifier: string, password: string, tenantName?: string): Promise<LoginResult> {
    const activeTenant = tenantName?.trim() || this.extractTenantFromUrl() || this.getTenant() || '';
    const url = `${environment.apis.default.url}/api/auth/login`;

    const headers = this.buildHeaders(activeTenant);
    const body = {
      login: loginIdentifier,
      password: password,
      tenantName: activeTenant || null
    };

    try {
      const response = await firstValueFrom(
        this.http.post<ApiResult<LoginResponseData>>(url, body, { headers })
      );

      if (response && response.success && response.data?.token) {
        await this.completeLogin(response.data, activeTenant);
        return { success: true };
      }

      return {
        success: false,
        error: response?.message || 'Invalid authentication response received.'
      };
    } catch (error: any) {
      console.error('Login error', error);
      if (error.status === 0) {
        return {
          success: false,
          error: 'Unable to connect to backend server. Please verify HttpApi.Host is running.'
        };
      }

      const errorMsg = error.error?.message || error.error?.errors?.[0] || 'Invalid credentials. Please try again.';
      return { success: false, error: errorMsg };
    }
  }

  /**
   * Exchanges the refresh token for a new access token via /api/auth/refresh.
   */
  refreshSession(): Promise<boolean> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      return Promise.resolve(false);
    }

    if (!this.refreshInFlight) {
      this.refreshInFlight = this.requestNewAccessToken(refreshToken).finally(() => {
        this.refreshInFlight = undefined;
      });
    }
    return this.refreshInFlight;
  }

  private async requestNewAccessToken(refreshToken: string): Promise<boolean> {
    const activeTenant = this.getTenant() || '';
    const url = `${environment.apis.default.url}/api/auth/refresh`;
    const headers = this.buildHeaders(activeTenant);

    try {
      const response = await firstValueFrom(
        this.http.post<ApiResult<RefreshResponseData>>(url, { refreshToken }, { headers })
      );

      if (response && response.success && response.data?.token) {
        localStorage.setItem(this.tokenKey, response.data.token);
        if (response.data.refreshToken) {
          localStorage.setItem(REFRESH_TOKEN_KEY, response.data.refreshToken);
        }
        return true;
      }
      return false;
    } catch (error) {
      console.warn('Session refresh failed', error);
      return false;
    }
  }

  logout(): void {
    this.clearTokens();
    this.session.stop();
    this.state.setGuestUser();
    this.router.navigateByUrl('/auth/login');
  }

  getToken(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken() && !this.session.isExpired();
  }

  private async completeLogin(data: LoginResponseData, tenantName: string): Promise<void> {
    localStorage.setItem(this.tokenKey, data.token);
    if (data.refreshToken) {
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
    }
    if (tenantName) {
      this.setTenant(tenantName);
    }

    // Starts the session clock
    this.session.start();

    // Populate user in StateService
    if (data.user) {
      const userRoles = data.user.roles || [];
      const isAdmin = userRoles.some(r => r.toLowerCase() === 'admin');

      this.state.setCurrentUser({
        id: data.user.id,
        name: data.user.name || data.user.userName || 'User',
        email: data.user.email || '',
        role: isAdmin ? 'Admin' : (userRoles[0] as any) || 'Employee',
        avatar: data.user.logoUrl || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
        permissions: data.user.permissions?.length ? data.user.permissions : (isAdmin ? ['*'] : []),
        tenantId: data.user.tenantId,
        tenantName: data.user.tenantName || tenantName,
        tenantLogo: data.user.logoUrl
      });
    }

    // Load additional ABP application config in background without blocking navigation
    this.state.loadAppConfig().catch(err => console.warn('App config load warning:', err));
  }

  private clearTokens(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    }
  }

  private buildHeaders(tenantName?: string): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    if (tenantName && tenantName.trim()) {
      headers = headers
        .set('X-Tenant-Name', tenantName.trim())
        .set('__tenant', tenantName.trim());
    }

    return headers;
  }
}
