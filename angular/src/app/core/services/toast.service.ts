import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastItem {
  id: string;
  type: ToastType;
  title: string;
  message: string;
  duration: number;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts = signal<ToastItem[]>([]);

  show(message: string, type: ToastType = 'info', title?: string, duration: number = 4000) {
    const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 4)}`;
    const defaultTitle = title || this.getDefaultTitle(type);
    const timestamp = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    const newToast: ToastItem = {
      id,
      type,
      title: defaultTitle,
      message,
      duration,
      timestamp
    };

    this.toasts.update(list => [newToast, ...list]);

    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }
  }

  success(message: string, title?: string, duration?: number) {
    this.show(message, 'success', title, duration);
  }

  error(message: string, title?: string, duration?: number) {
    this.show(message, 'error', title, duration);
  }

  warning(message: string, title?: string, duration?: number) {
    this.show(message, 'warning', title, duration);
  }

  info(message: string, title?: string, duration?: number) {
    this.show(message, 'info', title, duration);
  }

  dismiss(id: string) {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }

  clearAll() {
    this.toasts.set([]);
  }

  private getDefaultTitle(type: ToastType): string {
    switch (type) {
      case 'success': return 'Success';
      case 'error': return 'Error';
      case 'warning': return 'Warning';
      case 'info': return 'Notification';
    }
  }
}
