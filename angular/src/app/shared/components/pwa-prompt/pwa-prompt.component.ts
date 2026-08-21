import { Component, inject, signal } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [],
  templateUrl: './pwa-prompt.component.html'
})
export class PwaPromptComponent {
  private toast = inject(ToastService);
  showPrompt = signal(true);

  dismiss() {
    this.showPrompt.set(false);
  }

  installPwa() {
    this.toast.success('ERP Platform Progressive Web Application installed successfully!', 'PWA Installed');
    this.dismiss();
  }
}
