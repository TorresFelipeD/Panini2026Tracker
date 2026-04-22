import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { AlbumOverview, StickerDetail } from '../models/app.models';
import { APP_CONFIG } from '../tokens/app-config.token';
import { resolveImageUrl } from '../utils/image-url';
import { ToastService } from './toast.service';

export interface AlbumFilters {
  search: string;
  countryCode: string;
  isOwned: string;
  hasImage: string;
  hasDuplicates: string;
}

const defaultFilters: AlbumFilters = {
  search: '',
  countryCode: '',
  isOwned: '',
  hasImage: '',
  hasDuplicates: ''
};

@Injectable({ providedIn: 'root' })
export class AlbumStoreService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly toastService = inject(ToastService);

  readonly filters = signal<AlbumFilters>(defaultFilters);
  readonly overview = signal<AlbumOverview | null>(null);
  readonly selectedSticker = signal<StickerDetail | null>(null);
  readonly loading = signal(false);

  readonly allStickers = computed(() => this.overview()?.countries.flatMap(country => country.stickers) ?? []);

  load(): void {
    this.loading.set(true);
    let params = new HttpParams();
    const filters = this.filters();

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== '') {
        params = params.set(key, value);
      }
    });

    this.http.get<AlbumOverview>(`${this.config.apiBaseUrl}/album`, { params }).subscribe({
      next: value => {
        this.overview.set({
          ...value,
          countries: value.countries.map(country => ({
            ...country,
            stickers: country.stickers.map(sticker => ({
              ...sticker,
              imageUrl: resolveImageUrl(this.config.apiBaseUrl, sticker.imageUrl)
            }))
          }))
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  setFilters(filters: Partial<AlbumFilters>): void {
    this.filters.update(current => ({ ...current, ...filters }));
    this.load();
  }

  openSticker(stickerId: string): void {
    this.http.get<StickerDetail>(`${this.config.apiBaseUrl}/stickers/${stickerId}`).subscribe({
      next: value => this.selectedSticker.set({
        ...value,
        imageUrl: resolveImageUrl(this.config.apiBaseUrl, value.imageUrl)
      })
    });
  }

  closeSticker(): void {
    this.selectedSticker.set(null);
  }

  saveSticker(payload: { stickerId: string; isOwned: boolean; duplicateCount: number; notes: string }): void {
    this.http.put<StickerDetail>(`${this.config.apiBaseUrl}/stickers/${payload.stickerId}/state`, payload).subscribe({
      next: value => {
        this.selectedSticker.set({
          ...value,
          imageUrl: resolveImageUrl(this.config.apiBaseUrl, value.imageUrl)
        });
        this.load();
        this.toastService.success(`Lámina ${value.stickerCode} actualizada correctamente.`);
      },
      error: () => {
        this.toastService.error('No se pudieron guardar los cambios de la lámina.');
      }
    });
  }
}
