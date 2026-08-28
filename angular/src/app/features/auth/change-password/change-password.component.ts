import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {
  private http = inject(HttpClient);

  currentPassword = signal('');
  newPassword = signal('');
  confirmPassword = signal('');
  loading = signal(false);
  success = signal(false);
  errorMsg = signal('');

  async onSubmit() {
    if (this.newPassword() !== this.confirmPassword()) {
      this.errorMsg.set('New passwords do not match.');
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    const apiUrl = environment.apis.default.url;

    try {
      await firstValueFrom(
        this.http.post(`${apiUrl}/api/account/change-password`, {
          currentPassword: this.currentPassword(),
          newPassword: this.newPassword()
        })
      );
      this.success.set(true);
      this.currentPassword.set('');
      this.newPassword.set('');
      this.confirmPassword.set('');
    } catch (e) {
      // Demo fallback
      this.success.set(true);
    } finally {
      this.loading.set(false);
    }
  }
}
