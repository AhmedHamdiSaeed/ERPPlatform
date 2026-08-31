import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SessionTimeoutService } from '../../../core/services/session-timeout.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule],
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

  /** Page the user was heading to before the session ended. */
  private returnUrl = '';
  hasReturnUrl = signal(false);

  loginForm = this.fb.group({
    email: ['ahmed.hamdi@erpplatform.com', [Validators.required, Validators.email]],
    password: ['Admin123!', [Validators.required]],
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

    const success = await this.authService.login(email, password);
    this.loading.set(false);

    if (success) {
      // A fresh 3h session starts here; go back to the page the user wanted.
      const target = this.returnUrl || '/dashboard';
      this.returnUrl = '';
      this.sessionNotice.set('');
      this.router.navigateByUrl(target, { replaceUrl: true });
    } else {
      this.errorMessage.set('Invalid email or password. Please try again.');
    }
  }

  fillDemo(email: string) {
    this.loginForm.patchValue({
      email: email,
      password: 'DemoPassword123!'
    });
  }
}
