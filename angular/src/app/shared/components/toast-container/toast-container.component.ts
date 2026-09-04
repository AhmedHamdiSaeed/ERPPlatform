import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastItem } from '../../../core/services/toast.service';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './toast-container.component.html'
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
  state = inject(StateService);

  dismiss(id: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.toastService.dismiss(id);
  }

  getIcon(type: string): string {
    switch (type) {
      case 'success': return 'pi-check-circle';
      case 'error': return 'pi-times-circle';
      case 'warning': return 'pi-exclamation-triangle';
      case 'info': return 'pi-info-circle';
      default: return 'pi-bell';
    }
  }
}
