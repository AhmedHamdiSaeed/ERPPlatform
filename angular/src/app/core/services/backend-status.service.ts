import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BackendStatusService {
  private http = inject(HttpClient);

  readonly isOffline = signal<boolean>(false);
  readonly isChecking = signal<boolean>(false);
  readonly failedUrl = signal<string>('');
  readonly errorMessage = signal<string>('');
  readonly statusCode = signal<number>(0);
  readonly retryCount = signal<number>(0);
  readonly backendBaseUrl = signal<string>(environment.apis.default.url);

  reportError(url: string, status: number, message: string) {
    // Avoid re-triggering while actively checking
    if (this.isChecking()) {
      return;
    }

    this.failedUrl.set(url);
    this.statusCode.set(status);
    this.errorMessage.set(message || 'Unable to establish connection to the backend server.');
    this.isOffline.set(true);
  }

  async checkConnection(): Promise<boolean> {
    this.isChecking.set(true);
    this.retryCount.update(c => c + 1);

    try {
      const pingUrl = `${this.backendBaseUrl()}/api/abp/application-configuration?_t=${Date.now()}`;
      await firstValueFrom(this.http.get(pingUrl, { responseType: 'text' }));
      this.isOffline.set(false);
      this.errorMessage.set('');
      this.isChecking.set(false);
      return true;
    } catch (err: any) {
      // If status is 200 or 401 (meaning backend is alive and responded with auth challenge)
      if (err && (err.status === 200 || err.status === 401)) {
        this.isOffline.set(false);
        this.errorMessage.set('');
        this.isChecking.set(false);
        return true;
      }

      this.isChecking.set(false);
      return false;
    }
  }

  dismiss() {
    this.isOffline.set(false);
  }
}
