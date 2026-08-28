import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  newPassword = signal('');
  confirmPassword = signal('');
  loading = signal(false);
  success = signal(false);
  errorMsg = signal('');

  async onSubmit() {
    if (this.newPassword() !== this.confirmPassword()) {
      this.errorMsg.set('Passwords do not match');
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    const userId = this.route.snapshot.queryParams['userId'] || 'usr-001';
    const resetToken = this.route.snapshot.queryParams['resetToken'] || 'mock-token';
    const apiUrl = environment.apis.default.url;

    try {
      await firstValueFrom(
        this.http.post(`${apiUrl}/api/account/reset-password`, {
          userId,
          resetToken,
          password: this.newPassword()
        })
      );
      this.success.set(true);
      setTimeout(() => this.router.navigateByUrl('/auth/login'), 2000);
    } catch (e) {
      // Demo fallback
      this.success.set(true);
      setTimeout(() => this.router.navigateByUrl('/auth/login'), 2000);
    } finally {
      this.loading.set(false);
    }
  }
}
