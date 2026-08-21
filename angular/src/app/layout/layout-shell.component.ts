import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../shared/components/header/header.component';
import { SidebarComponent } from '../shared/components/sidebar/sidebar.component';
import { GlobalSearchModalComponent } from '../shared/components/global-search-modal/global-search-modal.component';
import { AiWidgetComponent } from '../shared/components/ai-widget/ai-widget.component';
import { PwaPromptComponent } from '../shared/components/pwa-prompt/pwa-prompt.component';
import { ToastContainerComponent } from '../shared/components/toast-container/toast-container.component';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';
import { StateService } from '../core/services/state.service';

@Component({
  selector: 'app-layout-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    SidebarComponent,
    GlobalSearchModalComponent,
    AiWidgetComponent,
    PwaPromptComponent,
    ToastContainerComponent,
    ConfirmDialogComponent
  ],
  templateUrl: './layout-shell.component.html'
})
export class LayoutShellComponent {
  state = inject(StateService);
}
