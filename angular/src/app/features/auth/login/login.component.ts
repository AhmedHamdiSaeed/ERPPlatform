import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SessionTimeoutService } from '../../../core/services/session-timeout.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, TranslatePipe],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);
  private session = inject(SessionTimeoutService);

  loading = signal(false);
  errorMessage = signal('');
  sessionNotice = signal('');
  tenantName = signal('');

  /** Page the user was heading to before the session ended. */
  private returnUrl = '';
  hasReturnUrl = signal(false);

  loginForm = this.fb.group({
    email: ['admin@erpplatform.com', [Validators.required]],
    password: ['Admin123!', [Validators.required]],
    tenant: [''],
    rememberMe: [true]
  });

  ngOnInit(): void {
    const queryParams = this.route.snapshot.queryParamMap;
    this.returnUrl = this.session.consumeReturnUrl(queryParams.get('returnUrl'));
    this.hasReturnUrl.set(!!this.returnUrl);

    if (queryParams.get('reason') === 'timeout') {
      this.sessionNotice.set(
        'Your session has expired for security reasons. Sign in again to go back to where you were.'
      );
    }

    // Extract tenant from URL query or subdomain or local storage
    const detectedTenant = queryParams.get('tenant') || queryParams.get('__tenant') || queryParams.get('tenantName') || this.authService.getTenant() || '';
    if (detectedTenant) {
      this.tenantName.set(detectedTenant);
      this.loginForm.patchValue({ tenant: detectedTenant });
      this.authService.setTenant(detectedTenant);
    }
  }

  async onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const email = this.loginForm.value.email!;
    const password = this.loginForm.value.password!;
    const tenant = this.loginForm.value.tenant || this.tenantName() || undefined;

    try {
      const result = await this.authService.login(email, password, tenant);

      if (result.success) {
        const target = this.returnUrl || '/dashboard';
        this.returnUrl = '';
        this.sessionNotice.set('');
        this.router.navigateByUrl(target, { replaceUrl: true });
      } else {
        this.errorMessage.set(result.error || 'Invalid credentials. Please try again.');
      }
    } catch (err: any) {
      console.error('Unexpected login error', err);
      this.errorMessage.set('An unexpected error occurred during login. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }

  private readonly demoCredentials: Record<string, string> = {
    'admin@erpplatform.com': 'Admin123!',
    'ahmed.hamdi@erpplatform.com': 'Admin123!',
    'admin': 'Admin123!',
    '+201000000000': 'Admin123!',
    '01000000000': 'Admin123!',
    'sara.mansour@erpplatform.com': 'Manager123!',
    'omar.khaled@erpplatform.com': 'Employee123!',
    'lina.nasser@erpplatform.com': 'Staff123!',
    'admin@abp.io': '1q2w3E*'
  };

  fillDemo(email: string) {
    this.loginForm.patchValue({
      email: email,
      password: this.demoCredentials[email] ?? this.loginForm.value.password ?? ''
    });
  }

  async loginAs(email: string) {
    this.fillDemo(email);
    await this.onSubmit();
  }
}
