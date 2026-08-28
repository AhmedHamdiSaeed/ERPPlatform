import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);

  loading = signal(false);
  errorMessage = signal('');

  loginForm = this.fb.group({
    email: ['ahmed.hamdi@erpplatform.com', [Validators.required, Validators.email]],
    password: ['Admin123!', [Validators.required]],
    rememberMe: [true]
  });

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
      this.router.navigateByUrl('/dashboard');
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
