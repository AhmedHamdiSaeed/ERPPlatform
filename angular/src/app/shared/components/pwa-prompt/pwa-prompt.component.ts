import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [],
  template: `
    @if (showPrompt()) {
      <div class="fixed bottom-4 right-4 z-40 max-w-sm bg-[var(--bg-card)] border border-[var(--border-color)] p-4 rounded-2xl shadow-2xl animate-fade-in">
        <div class="flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-blue-600 text-white flex items-center justify-center text-xl shrink-0 shadow-md">
            <i class="pi pi-box"></i>
          </div>
          <div class="flex-1">
            <h4 class="font-bold text-xs text-[var(--text-main)]">Install ERP Platform</h4>
            <p class="text-[11px] text-[var(--text-muted)] mt-0.5 leading-relaxed">
              Install the application on your device for instant offline access and desktop experience.
            </p>
            <div class="flex items-center gap-2 mt-3">
              <button (click)="installPwa()" class="px-3 py-1.5 bg-blue-600 text-white font-semibold text-xs rounded-lg hover:bg-blue-700 transition-colors shadow-xs">
                Install App
              </button>
              <button (click)="dismiss()" class="px-3 py-1.5 text-xs text-slate-500 hover:text-slate-700 font-medium">
                Not now
              </button>
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class PwaPromptComponent {
  showPrompt = signal(true);

  dismiss() {
    this.showPrompt.set(false);
  }

  installPwa() {
    alert('ERP Platform Progressive Web Application installed successfully!');
    this.dismiss();
  }
}
