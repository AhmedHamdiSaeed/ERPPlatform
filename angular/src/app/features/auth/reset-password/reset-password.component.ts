import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  token = signal('');
  tenantName = signal('');
  userEmail = signal('');

  newPassword = signal('');
  confirmPassword = signal('');
  showPassword = signal(false);
  showConfirmPassword = signal(false);

  validatingToken = signal(true);
  tokenValid = signal(true);
  tokenError = signal('');

  loading = signal(false);
  success = signal(false);
  errorMsg = signal('');

  // Password rules validation
  hasMinLength = computed(() => this.newPassword().length >= 6);
  hasUpperCase = computed(() => /[A-Z]/.test(this.newPassword()));
  hasLowerCase = computed(() => /[a-z]/.test(this.newPassword()));
  hasDigitOrSpecial = computed(() => /[0-9!@#$%^&*(),.?":{}|<>]/.test(this.newPassword()));
  passwordsMatch = computed(() => !!this.newPassword() && this.newPassword() === this.confirmPassword());

  isFormValid = computed(() => 
    this.hasMinLength() && 
    this.passwordsMatch() &&
    !this.loading()
  );

  async ngOnInit() {
    const rawToken = this.route.snapshot.queryParams['token'] || 
                     this.route.snapshot.queryParams['resetToken'] || '';
    const tenantParam = this.route.snapshot.queryParams['tenant'] || 
                        this.route.snapshot.queryParams['__tenant'] || '';

    if (tenantParam) {
      this.tenantName.set(tenantParam);
    }

    if (!rawToken || !rawToken.trim()) {
      this.validatingToken.set(false);
      this.tokenValid.set(false);
      this.tokenError.set('No reset token was found in the link. Please request a new password reset link.');
      return;
    }

    this.token.set(rawToken.trim());

    // Validate the token against the backend
    try {
      const res = await this.authService.validateResetToken(rawToken.trim());
      if (res && res.success && res.data?.isValid) {
        this.tokenValid.set(true);
        if (res.data.email) this.userEmail.set(res.data.email);
        if (res.data.tenantName) this.tenantName.set(res.data.tenantName);
      } else {
        this.tokenValid.set(false);
        this.tokenError.set(res.message || 'This password reset link is invalid or has expired.');
      }
    } catch {
      // If network fails, allow user to try submitting rather than hard-blocking
      this.tokenValid.set(true);
    } finally {
      this.validatingToken.set(false);
    }
  }

  togglePasswordVisibility() {
    this.showPassword.set(!this.showPassword());
  }

  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword.set(!this.showConfirmPassword());
  }

  async onSubmit() {
    if (!this.isFormValid()) return;

    this.loading.set(true);
    this.errorMsg.set('');

    try {
      const result = await this.authService.resetPassword(this.token(), this.newPassword(), this.tenantName());

      if (result.success) {
        this.success.set(true);
      } else {
        this.errorMsg.set(result.message || 'Failed to reset password. Please request a new link.');
      }
    } catch (e: any) {
      this.errorMsg.set(e?.message || 'An error occurred while resetting your password.');
    } finally {
      this.loading.set(false);
    }
  }

  goToLogin() {
    this.router.navigate(['/auth/login'], {
      queryParams: this.tenantName() ? { tenant: this.tenantName() } : undefined
    });
  }
}
