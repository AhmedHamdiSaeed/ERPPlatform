import { Component, OnInit, OnDestroy, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';
import { StateService } from '../../../core/services/state.service';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pwa-prompt.component.html'
})
export class PwaPromptComponent implements OnInit, OnDestroy {
  private toast = inject(ToastService);
  public state = inject(StateService);

  showPrompt = signal(false);
  showGuideModal = signal(false);
  isIos = signal(false);
  private deferredPrompt: any = null;

  constructor() {
    effect(() => {
      const req = this.state.pwaInstallRequested();
      if (req > 0) {
        this.installPwa();
      }
    });
  }

  private onBeforeInstallPrompt = (e: Event) => {
    e.preventDefault();
    this.deferredPrompt = e;
    if (!this.isStandalone()) {
      const dismissed = localStorage.getItem('pwa_prompt_dismissed');
      if (!dismissed) {
        this.showPrompt.set(true);
      }
    }
  };

  private onAppInstalled = () => {
    this.showPrompt.set(false);
    this.showGuideModal.set(false);
    this.deferredPrompt = null;
    this.toast.success(
      this.state.isRtl() ? 'تم تثبيت التطبيق بنجاح على جهازك!' : 'ERP Platform installed successfully on your device!',
      this.state.isRtl() ? 'تثبيت التطبيق' : 'PWA Installed'
    );
  };

  ngOnInit() {
    if (typeof window !== 'undefined') {
      const ua = window.navigator.userAgent.toLowerCase();
      this.isIos.set(/iphone|ipad|ipod/.test(ua));

      if (this.isStandalone()) {
        return;
      }

      window.addEventListener('beforeinstallprompt', this.onBeforeInstallPrompt);
      window.addEventListener('appinstalled', this.onAppInstalled);

      const dismissed = localStorage.getItem('pwa_prompt_dismissed');
      if (!dismissed) {
        this.showPrompt.set(true);
      }
    }
  }

  ngOnDestroy() {
    if (typeof window !== 'undefined') {
      window.removeEventListener('beforeinstallprompt', this.onBeforeInstallPrompt);
      window.removeEventListener('appinstalled', this.onAppInstalled);
    }
  }

  isStandalone(): boolean {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(display-mode: standalone)').matches ||
           (window.navigator as any).standalone === true;
  }

  dismiss() {
    this.showPrompt.set(false);
    try {
      localStorage.setItem('pwa_prompt_dismissed', 'true');
    } catch {}
  }

  closeGuide() {
    this.showGuideModal.set(false);
  }

  async installPwa() {
    if (this.deferredPrompt) {
      try {
        this.deferredPrompt.prompt();
        const choice = await this.deferredPrompt.userChoice;
        if (choice && choice.outcome === 'accepted') {
          this.toast.success(
            this.state.isRtl() ? 'جاري تثبيت التطبيق...' : 'Installing application...',
            this.state.isRtl() ? 'تثبيت التطبيق' : 'PWA Install'
          );
        }
        this.deferredPrompt = null;
        this.dismiss();
      } catch (e) {
        this.showGuideModal.set(true);
      }
    } else {
      // Browser didn't provide beforeinstallprompt (e.g., iOS Safari, Firefox, or Chrome desktop with address bar install)
      this.showGuideModal.set(true);
    }
  }
}

