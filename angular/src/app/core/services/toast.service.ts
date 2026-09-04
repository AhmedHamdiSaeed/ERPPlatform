import { Injectable, signal, inject } from '@angular/core';
import { TranslationService } from './translation.service';
import { StateService } from './state.service';

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
  private translation = inject(TranslationService, { optional: true });
  private state = inject(StateService, { optional: true });

  toasts = signal<ToastItem[]>([]);

  show(message: string, type: ToastType = 'info', title?: string, duration: number = 4000) {
    const id = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 4)}`;
    const isAr = this.state ? this.state.lang() === 'ar' : false;
    const defaultTitle = title || this.getDefaultTitle(type, isAr);
    const timestamp = new Date().toLocaleTimeString(isAr ? 'ar-EG' : 'en-US', { hour: '2-digit', minute: '2-digit' });

    // Localize message and title according to current language
    const localizedMessage = this.translation ? this.translation.get(message) : message;
    const localizedTitle = this.translation ? this.translation.get(defaultTitle) : defaultTitle;

    const newToast: ToastItem = {
      id,
      type,
      title: localizedTitle,
      message: localizedMessage,
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

  private getDefaultTitle(type: ToastType, isAr: boolean = false): string {
    if (isAr) {
      switch (type) {
        case 'success': return 'نجاح';
        case 'error': return 'خطأ';
        case 'warning': return 'تنبيه';
        case 'info': return 'إشعار';
      }
    }
    switch (type) {
      case 'success': return 'Success';
      case 'error': return 'Error';
      case 'warning': return 'Warning';
      case 'info': return 'Notification';
    }
  }
}
