import { Injectable, inject, signal } from '@angular/core';
import { SearchApiService, SearchResultGroup, SEARCH_MIN_LENGTH } from './api/search-api.service';

@Injectable({
  providedIn: 'root'
})
export class GlobalSearchService {
  private searchApi = inject(SearchApiService);

  /** True while a request is in flight, so the palette can show a spinner. */
  isSearching = signal(false);

  /** Populated when the last search failed, so the UI can show a retry hint. */
  lastError = signal<string | null>(null);

  /**
   * Runs a server-side global search. Returns an empty list for queries shorter
   * than SEARCH_MIN_LENGTH so we don't hit the API on every keystroke.
   */
  async search(query: string): Promise<SearchResultGroup[]> {
    const trimmed = (query ?? '').trim();
    if (trimmed.length < SEARCH_MIN_LENGTH) {
      this.lastError.set(null);
      return [];
    }

    this.isSearching.set(true);
    this.lastError.set(null);

    try {
      return await this.searchApi.search(trimmed);
    } catch (err) {
      console.error('Global search failed', err);
      this.lastError.set('Search is unavailable right now.');
      return [];
    } finally {
      this.isSearching.set(false);
    }
  }
}

export { SEARCH_MIN_LENGTH };
export type { SearchResultGroup, SearchResultItem } from './api/search-api.service';
