import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from '../../core/services/translation.service';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false
})
export class TranslatePipe implements PipeTransform {
  private translation = inject(TranslationService);

  transform(key: string, defaultVal?: string): string {
    return this.translation.get(key, defaultVal);
  }
}
