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

export interface PromptDialogOptions {
  title?: string;
  message: string;
  placeholder?: string;
  defaultValue?: string;
  confirmText?: string;
  cancelText?: string;
  type?: DialogType;
  icon?: string;
  multiline?: boolean;
}

export interface ActiveDialogState {
  options: ConfirmDialogOptions;
  resolve: (value: boolean) => void;
}

export interface ActivePromptState {
  options: PromptDialogOptions;
  value: string;
  resolve: (value: string | null) => void;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  activeDialog = signal<ActiveDialogState | null>(null);
  activePrompt = signal<ActivePromptState | null>(null);
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

  prompt(options: PromptDialogOptions): Promise<string | null> {
    return new Promise<string | null>((resolve) => {
      this.activePrompt.set({
        options: {
          title: options.title || 'Input Required',
          message: options.message,
          placeholder: options.placeholder || '',
          defaultValue: options.defaultValue || '',
          confirmText: options.confirmText || 'Submit',
          cancelText: options.cancelText || 'Cancel',
          type: options.type || 'info',
          icon: options.icon,
          multiline: options.multiline ?? false
        },
        value: options.defaultValue || '',
        resolve
      });
    });
  }

  setPromptValue(value: string) {
    const current = this.activePrompt();
    if (current) {
      this.activePrompt.set({ ...current, value });
    }
  }

  handlePromptConfirm() {
    const current = this.activePrompt();
    if (current) {
      current.resolve(current.value);
      this.activePrompt.set(null);
    }
  }

  handlePromptCancel() {
    const current = this.activePrompt();
    if (current) {
      current.resolve(null);
      this.activePrompt.set(null);
    }
  }
}
