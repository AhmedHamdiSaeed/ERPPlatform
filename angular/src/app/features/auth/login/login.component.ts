import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);

  loading = signal(false);
  errorMessage = signal('');

  loginForm = this.fb.group({
    email: ['ahmed.hamdi@erpplatform.com', [Validators.required, Validators.email]],
    password: ['Admin123!', [Validators.required]],
    rememberMe: [true]
  });

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    setTimeout(() => {
      this.loading.set(false);
      this.router.navigateByUrl('/dashboard');
    }, 800);
  }

  fillDemo(email: string) {
    this.loginForm.patchValue({
      email: email,
      password: 'DemoPassword123!'
    });
  }
}
