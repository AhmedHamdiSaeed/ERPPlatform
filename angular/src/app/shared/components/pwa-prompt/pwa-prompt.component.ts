import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';
import { StateService } from '../../../core/services/state.service';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [],
  templateUrl: './pwa-prompt.component.html'
})
export class PwaPromptComponent implements OnInit, OnDestroy {
  private toast = inject(ToastService);
  public state = inject(StateService);

  showPrompt = signal(false);
  private deferredPrompt: any = null;

  private onBeforeInstallPrompt = (e: Event) => {
    e.preventDefault();
    this.deferredPrompt = e;
    const dismissed = localStorage.getItem('pwa_prompt_dismissed');
    if (!dismissed) {
      this.showPrompt.set(true);
    }
  };

  ngOnInit() {
    window.addEventListener('beforeinstallprompt', this.onBeforeInstallPrompt);
    const dismissed = localStorage.getItem('pwa_prompt_dismissed');
    if (!dismissed) {
      this.showPrompt.set(true);
    }
  }

  ngOnDestroy() {
    window.removeEventListener('beforeinstallprompt', this.onBeforeInstallPrompt);
  }

  dismiss() {
    this.showPrompt.set(false);
    try {
      localStorage.setItem('pwa_prompt_dismissed', 'true');
    } catch {}
  }

  async installPwa() {
    if (this.deferredPrompt) {
      this.deferredPrompt.prompt();
      const choice = await this.deferredPrompt.userChoice;
      if (choice.outcome === 'accepted') {
        this.toast.success(
          this.state.isRtl() ? 'تم تثبيت التطبيق بنجاح!' : 'App installed successfully!',
          this.state.isRtl() ? 'تثبيت التطبيق' : 'PWA Installed'
        );
      }
      this.deferredPrompt = null;
    } else {
      this.toast.success(
        this.state.isRtl() ? 'تم تثبيت منصة ERP بنجاح على جهازك!' : 'ERP Platform installed successfully!',
        this.state.isRtl() ? 'تثبيت التطبيق' : 'PWA Installed'
      );
    }
    this.dismiss();
  }
}
