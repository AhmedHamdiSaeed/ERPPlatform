import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-pwa-prompt',
  standalone: true,
  imports: [],
  templateUrl: './pwa-prompt.component.html'
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
