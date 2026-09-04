import { Injectable, inject } from '@angular/core';
import { StateService } from './state.service';
import enDict from '../../../assets/i18n/en.json';
import arDict from '../../../assets/i18n/ar.json';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private state = inject(StateService);

  private translations: Record<string, Record<string, string>> = {
    en: enDict as Record<string, string>,
    ar: arDict as Record<string, string>
  };

  /**
   * Retrieves translation for the given key based on current language.
   * If key is not found in the current language, falls back to English, then defaultVal, then key itself.
   */
  get(key: string, defaultVal?: string): string {
    if (!key) return '';
    const currentLang = this.state.lang() || 'en';
    const dict = this.translations[currentLang] || this.translations['en'];

    if (dict && dict[key] !== undefined) {
      return dict[key];
    }

    // Fallback to English if current was Arabic
    if (currentLang !== 'en' && this.translations['en'] && this.translations['en'][key] !== undefined) {
      return this.translations['en'][key];
    }

    return defaultVal !== undefined ? defaultVal : key;
  }

  instant(key: string, defaultVal?: string): string {
    return this.get(key, defaultVal);
  }
}
