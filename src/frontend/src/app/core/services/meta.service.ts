import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { APP_CONFIG } from '../tokens/app-config.token';
import { AppMeta } from '../models/app.models';

@Injectable({ providedIn: 'root' })
export class MetaService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);

  readonly meta = signal<AppMeta | null>(null);

  load(): void {
    this.http.get<AppMeta>(`${this.config.apiBaseUrl}/meta`).subscribe({
      next: value => this.meta.set(value)
    });
  }
}
