import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastItem } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-container.component.html'
})
export class ToastContainerComponent {
  toastService = inject(ToastService);

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
