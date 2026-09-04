import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'appDate',
  standalone: true
})
export class AppDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, mode: 'date' | 'datetime' | 'auto' = 'auto'): string {
    if (!value) return '';

    let str = typeof value === 'string' ? value.trim() : (value instanceof Date ? value.toISOString() : String(value));

    // Handle ISO string dates
    if (str.includes('T')) {
      // Midnight dates: 2026-09-04T00:00:00 -> 2026-09-04
      if (/T00:00:00/i.test(str)) {
        return str.split('T')[0];
      }

      if (mode === 'date') {
        return str.split('T')[0];
      }

      // Format ISO with meaningful time: 2026-09-04 14:30
      const match = str.match(/^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/);
      if (match) {
        return `${match[1]} ${match[2]}`;
      }
    }

    return str;
  }
}
