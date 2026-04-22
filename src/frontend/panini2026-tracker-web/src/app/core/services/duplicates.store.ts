import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { APP_CONFIG } from '../tokens/app-config.token';
import { DuplicateItem } from '../models/app.models';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class DuplicatesStoreService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly toastService = inject(ToastService);

  readonly items = signal<DuplicateItem[]>([]);
  readonly search = signal('');
  readonly countryCodes = signal<string[]>([]);

  load(): void {
    let params = new HttpParams();
    if (this.search()) {
      params = params.set('search', this.search());
    }
    this.countryCodes().forEach(countryCode => {
      params = params.append('countryCodes', countryCode);
    });

    this.http.get<DuplicateItem[]>(`${this.config.apiBaseUrl}/duplicates`, { params }).subscribe({
      next: value => this.items.set(value)
    });
  }

  updateFilter(search: string, countryCodes: string[]): void {
    this.search.set(search);
    this.countryCodes.set(countryCodes);
    this.load();
  }

  save(stickerId: string, quantity: number): void {
    this.http.put(`${this.config.apiBaseUrl}/duplicates/${stickerId}`, { quantity }).subscribe({
      next: () => {
        this.load();
        this.toastService.success(`Cantidad de repetidas actualizada a ${Math.max(0, quantity)}.`);
      },
      error: () => {
        this.toastService.error('No se pudo actualizar la cantidad de repetidas.');
      }
    });
  }

  remove(stickerId: string): void {
    this.http.delete(`${this.config.apiBaseUrl}/duplicates/${stickerId}`).subscribe({
      next: () => {
        this.load();
        this.toastService.success('Repetida eliminada correctamente.');
      },
      error: () => {
        this.toastService.error('No se pudo eliminar la repetida.');
      }
    });
  }
}
