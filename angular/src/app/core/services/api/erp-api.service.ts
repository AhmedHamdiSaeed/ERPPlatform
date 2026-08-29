import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ErpApiService {
  protected http = inject(HttpClient);
  protected readonly apiRoot = `${environment.apis.default.url}/api/app`;

  protected apiPrefix(): string {
    return this.apiRoot;
  }

  protected getList<T>(route: string): Promise<T[]> {
    return firstValueFrom(
      this.http.get<{ items: T[] }>(`${this.apiPrefix()}/${route}`)
    ).then(res => res.items ?? []);
  }

  protected get<T>(route: string): Promise<T> {
    return firstValueFrom(this.http.get<T>(`${this.apiPrefix()}/${route}`));
  }

  protected post<T>(route: string, body: unknown): Promise<T> {
    return firstValueFrom(this.http.post<T>(`${this.apiPrefix()}/${route}`, body));
  }

  protected put<T>(route: string, body: unknown): Promise<T> {
    return firstValueFrom(this.http.put<T>(`${this.apiPrefix()}/${route}`, body));
  }

  protected delete(route: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.apiPrefix()}/${route}`));
  }
}

export function toDateString(value?: string | Date): string {
  if (!value) return '';
  const date = typeof value === 'string' ? new Date(value) : value;
  return isNaN(date.getTime()) ? '' : date.toISOString().split('T')[0];
}

export interface AbpEntity {
  id: string;
}
