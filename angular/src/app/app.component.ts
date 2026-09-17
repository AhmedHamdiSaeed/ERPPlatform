import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslationService } from './core/services/translation.service';
import { StateService } from './core/services/state.service';
import { BackendOfflineComponent } from './shared/components/backend-offline/backend-offline.component';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  imports: [RouterOutlet, BackendOfflineComponent],
})
export class AppComponent {
  private translationService = inject(TranslationService);
  private stateService = inject(StateService);
}

