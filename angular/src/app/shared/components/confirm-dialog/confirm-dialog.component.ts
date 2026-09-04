import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogService } from '../../../core/services/dialog.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './confirm-dialog.component.html'
})
export class ConfirmDialogComponent {
  dialogService = inject(DialogService);

  @HostListener('window:keydown.escape')
  onEscape() {
    if (this.dialogService.activeDialog()) {
      this.dialogService.handleCancel();
    }
  }

  getIcon(type?: string): string {
    switch (type) {
      case 'danger': return 'pi-exclamation-triangle';
      case 'warning': return 'pi-exclamation-circle';
      case 'success': return 'pi-check-circle';
      case 'info': return 'pi-info-circle';
      default: return 'pi-question-circle';
    }
  }
}
