import { Injectable, signal } from '@angular/core';

export type DialogType = 'danger' | 'warning' | 'info' | 'success';

export interface ConfirmDialogOptions {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: DialogType;
  icon?: string;
}

export interface ActiveDialogState {
  options: ConfirmDialogOptions;
  resolve: (value: boolean) => void;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  activeDialog = signal<ActiveDialogState | null>(null);
  loading = signal(false);

  confirm(options: ConfirmDialogOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.activeDialog.set({
        options: {
          title: options.title || 'Confirm Action',
          message: options.message,
          confirmText: options.confirmText || 'Confirm',
          cancelText: options.cancelText || 'Cancel',
          type: options.type || 'warning',
          icon: options.icon
        },
        resolve
      });
    });
  }

  handleConfirm() {
    const current = this.activeDialog();
    if (current) {
      current.resolve(true);
      this.activeDialog.set(null);
    }
  }

  handleCancel() {
    const current = this.activeDialog();
    if (current) {
      current.resolve(false);
      this.activeDialog.set(null);
    }
  }
}
