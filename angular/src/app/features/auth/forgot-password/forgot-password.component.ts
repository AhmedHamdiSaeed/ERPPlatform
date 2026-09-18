import { Component, inject, signal, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent implements OnDestroy {
  private authService = inject(AuthService);

  email = signal('');
  loading = signal(false);
  sent = signal(false);
  errorMsg = signal('');
  cooldownSeconds = signal(0);
  private timer: any = null;

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  async onSubmit() {
    const trimmedEmail = this.email().trim();
    if (!trimmedEmail) return;

    if (this.cooldownSeconds() > 0) return;

    this.loading.set(true);
    this.errorMsg.set('');

    try {
      const result = await this.authService.requestPasswordReset(trimmedEmail);

      if (result.statusCode === 429) {
        this.errorMsg.set(result.message || 'Too many password reset requests. Please wait a few minutes before trying again.');
        return;
      }

      // Anti-enumeration guarantee: always show success screen
      this.sent.set(true);
      this.startCooldown(60);
    } catch (e: any) {
      // Fallback anti-enumeration display
      this.sent.set(true);
      this.startCooldown(60);
    } finally {
      this.loading.set(false);
    }
  }

  onResend() {
    if (this.cooldownSeconds() > 0) return;
    this.sent.set(false);
    this.onSubmit();
  }

  private startCooldown(seconds: number) {
    this.cooldownSeconds.set(seconds);
    if (this.timer) clearInterval(this.timer);

    this.timer = setInterval(() => {
      const next = this.cooldownSeconds() - 1;
      if (next <= 0) {
        this.cooldownSeconds.set(0);
        clearInterval(this.timer);
      } else {
        this.cooldownSeconds.set(next);
      }
    }, 1000);
  }
}
