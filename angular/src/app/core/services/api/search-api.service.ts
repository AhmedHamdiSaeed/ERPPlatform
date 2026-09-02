import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export interface SearchResultItem {
  title: string;
  subtitle: string;
  link: string;
  icon: string;
  badge: string;
}

export interface SearchResultGroup {
  category: string;
  items: SearchResultItem[];
}

interface SearchResultItemDto {
  title: string;
  subtitle: string;
  link: string;
  icon: string;
  badge: string;
}

interface SearchResultGroupDto {
  category: string;
  items: SearchResultItemDto[];
}

/** Minimum characters before the backend will run a search. */
export const SEARCH_MIN_LENGTH = 2;

@Injectable({ providedIn: 'root' })
export class SearchApiService extends ErpApiService {
  async search(query: string, maxPerCategory = 5): Promise<SearchResultGroup[]> {
    const trimmed = (query ?? '').trim();
    if (trimmed.length < SEARCH_MIN_LENGTH) return [];

    const params = `query=${encodeURIComponent(trimmed)}&maxPerCategory=${maxPerCategory}`;
    const groups = await this.getList<SearchResultGroupDto>(`search?${params}`);

    return groups.map(g => ({
      category: g.category,
      items: (g.items ?? []).map(i => ({
        title: i.title,
        subtitle: i.subtitle,
        link: i.link,
        icon: i.icon,
        badge: i.badge
      }))
    }));
  }
}
