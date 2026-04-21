import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { APP_CONFIG } from '../tokens/app-config.token';
import { SystemLogItem } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class LogsStoreService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);

  readonly items = signal<SystemLogItem[]>([]);
  readonly category = signal('');
  readonly level = signal('');
  readonly search = signal('');

  load(): void {
    let params = new HttpParams();
    if (this.category()) {
      params = params.set('category', this.category());
    }
    if (this.level()) {
      params = params.set('level', this.level());
    }
    if (this.search()) {
      params = params.set('search', this.search());
    }

    this.http.get<SystemLogItem[]>(`${this.config.apiBaseUrl}/logs`, { params }).subscribe({
      next: value => this.items.set(value)
    });
  }

  updateFilters(category: string, level: string, search: string): void {
    this.category.set(category);
    this.level.set(level);
    this.search.set(search);
    this.load();
  }

  deleteFiltered(): void {
    this.http.post<number>(`${this.config.apiBaseUrl}/logs/delete`, {
      category: this.category(),
      level: this.level(),
      search: this.search()
    }).subscribe({
      next: () => this.load()
    });
  }
}
