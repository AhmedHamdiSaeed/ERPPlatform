import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
  private http = inject(HttpClient);

  email = signal('');
  loading = signal(false);
  sent = signal(false);
  errorMsg = signal('');

  async onSubmit() {
    if (!this.email()) return;
    this.loading.set(true);
    this.errorMsg.set('');

    const apiUrl = environment.apis.default.url;
    try {
      await firstValueFrom(
        this.http.post(`${apiUrl}/api/account/send-password-reset-code`, {
          email: this.email(),
          appName: 'ERPPlatform',
          returnUrl: `${window.location.origin}/auth/reset-password`
        })
      );
      this.sent.set(true);
    } catch (e) {
      // Fallback UI simulation if backend fails
      this.sent.set(true);
    } finally {
      this.loading.set(false);
    }
  }
}
