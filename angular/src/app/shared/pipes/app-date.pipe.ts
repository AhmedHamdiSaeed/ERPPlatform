import { Pipe, PipeTransform, inject } from '@angular/core';
import { TimezoneService } from '../../core/services/timezone.service';

@Pipe({
  name: 'appDate',
  standalone: true
})
export class AppDatePipe implements PipeTransform {
  private timezoneService = inject(TimezoneService);

  transform(
    value: string | Date | null | undefined,
    mode: 'date' | 'datetime' | 'time' | 'full' | 'auto' = 'auto'
  ): string {
    return this.timezoneService.format(value, mode);
  }
}
