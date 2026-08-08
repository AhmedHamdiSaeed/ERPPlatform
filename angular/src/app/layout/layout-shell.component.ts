import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../shared/components/header/header.component';
import { SidebarComponent } from '../shared/components/sidebar/sidebar.component';
import { GlobalSearchModalComponent } from '../shared/components/global-search-modal/global-search-modal.component';
import { AiWidgetComponent } from '../shared/components/ai-widget/ai-widget.component';
import { PwaPromptComponent } from '../shared/components/pwa-prompt/pwa-prompt.component';
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
    PwaPromptComponent
  ],
  template: `
    <div class="flex h-screen overflow-hidden bg-[var(--bg-main)]">
      
      <!-- Sidebar -->
      <app-sidebar />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col min-w-0 overflow-hidden transition-all">
        
        <!-- Top Header Bar -->
        <app-header />
        
        <!-- Scrollable Content Region -->
        <main class="flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
          <router-outlet />
        </main>
      </div>

    </div>

    <!-- Global Overlays -->
    <app-global-search-modal />
    <app-ai-widget />
    <app-pwa-prompt />
  `
})
export class LayoutShellComponent {
  state = inject(StateService);
}
