import { Injectable, signal, inject } from '@angular/core';
import { StateService } from './state.service';

export interface TimezoneOption {
  value: string;
  label: string;
  offset: string;
}

const TIMEZONE_STORAGE_KEY = 'erp_user_timezone';

@Injectable({
  providedIn: 'root'
})
export class TimezoneService {
  private state = inject(StateService);

  readonly currentTimeZone = signal<string>(this.getInitialTimezone());

  readonly availableTimezones: TimezoneOption[] = [
    { value: 'Africa/Cairo', label: 'Cairo (EET/EEST)', offset: 'UTC+2 / UTC+3' },
    { value: 'Asia/Riyadh', label: 'Riyadh (AST)', offset: 'UTC+3' },
    { value: 'Asia/Dubai', label: 'Dubai (GST)', offset: 'UTC+4' },
    { value: 'Asia/Kuwait', label: 'Kuwait (AST)', offset: 'UTC+3' },
    { value: 'Asia/Amman', label: 'Amman (EEST)', offset: 'UTC+3' },
    { value: 'Europe/London', label: 'London (GMT/BST)', offset: 'UTC+0 / UTC+1' },
    { value: 'Europe/Paris', label: 'Paris (CET/CEST)', offset: 'UTC+1 / UTC+2' },
    { value: 'America/New_York', label: 'New York (EST/EDT)', offset: 'UTC-5 / UTC-4' },
    { value: 'UTC', label: 'UTC (Coordinated Universal Time)', offset: 'UTC+0' }
  ];

  private getInitialTimezone(): string {
    if (typeof localStorage !== 'undefined') {
      const saved = localStorage.getItem(TIMEZONE_STORAGE_KEY);
      if (saved) return saved;
    }

    try {
      return Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';
    } catch {
      return 'UTC';
    }
  }

  setTimeZone(tz: string) {
    this.currentTimeZone.set(tz);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(TIMEZONE_STORAGE_KEY, tz);
    }
  }

  /**
   * Formats a UTC string or Date object into the user's active timezone.
   */
  format(
    value: string | Date | null | undefined,
    mode: 'date' | 'datetime' | 'time' | 'full' | 'auto' = 'auto'
  ): string {
    if (!value) return '';

    let date: Date;
    if (value instanceof Date) {
      date = value;
    } else {
      let dateStr = String(value).trim();
      // Ensure ISO string with no timezone suffix is treated as UTC
      if (dateStr.includes('T') && !dateStr.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(dateStr)) {
        dateStr = `${dateStr}Z`;
      }
      date = new Date(dateStr);
    }

    if (isNaN(date.getTime())) {
      return String(value);
    }

    const timeZone = this.currentTimeZone();
    const isArabic = this.state.isRtl();
    const locale = isArabic ? 'ar-EG' : 'en-US';

    try {
      if (mode === 'date') {
        return new Intl.DateTimeFormat(locale, {
          timeZone,
          year: 'numeric',
          month: '2-digit',
          day: '2-digit'
        }).format(date);
      }

      if (mode === 'time') {
        return new Intl.DateTimeFormat(locale, {
          timeZone,
          hour: '2-digit',
          minute: '2-digit',
          hour12: true
        }).format(date);
      }

      if (mode === 'full') {
        return new Intl.DateTimeFormat(locale, {
          timeZone,
          year: 'numeric',
          month: 'short',
          day: '2-digit',
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit',
          hour12: true
        }).format(date);
      }

      // Default / datetime / auto
      const isMidnight = date.getUTCHours() === 0 && date.getUTCMinutes() === 0 && date.getUTCSeconds() === 0;
      if (mode === 'auto' && isMidnight && typeof value === 'string' && value.includes('T00:00:00')) {
        return new Intl.DateTimeFormat(locale, {
          timeZone,
          year: 'numeric',
          month: '2-digit',
          day: '2-digit'
        }).format(date);
      }

      return new Intl.DateTimeFormat(locale, {
        timeZone,
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      }).format(date);
    } catch {
      // Fallback
      return date.toLocaleString(locale);
    }
  }
}
