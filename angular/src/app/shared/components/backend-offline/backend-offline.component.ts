import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackendStatusService } from '../../../core/services/backend-status.service';
import { StateService } from '../../../core/services/state.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-backend-offline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './backend-offline.component.html'
})
export class BackendOfflineComponent {
  readonly backendStatus = inject(BackendStatusService);
  readonly state = inject(StateService);
  private router = inject(Router);

  readonly isStandalone = signal(
    this.router.url.includes('/offline') || this.router.url.includes('/server-error')
  );

  async onRetry() {
    const success = await this.backendStatus.checkConnection();
    if (success) {
      window.location.reload();
    }
  }

  onDismiss() {
    this.backendStatus.dismiss();
  }
}
