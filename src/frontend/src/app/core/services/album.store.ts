import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { AlbumOverview, StickerDetail } from '../models/app.models';
import { APP_CONFIG } from '../tokens/app-config.token';
import { resolveImageUrl } from '../utils/image-url';
import { ToastService } from './toast.service';

export interface AlbumFilters {
  search: string;
  countryCodes: string[];
  isOwned: string;
  hasImage: string;
  hasDuplicates: string;
}

const defaultFilters: AlbumFilters = {
  search: '',
  countryCodes: [],
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
  readonly availableCountries = signal<{ countryCode: string; countryName: string; flagCode: string; group: string; displayOrder: number; displayOrderGroup: number }[]>([]);
  readonly selectedSticker = signal<StickerDetail | null>(null);
  readonly loading = signal(false);

  readonly allStickers = computed(() => {
    const overview = this.overview();
    if (!overview) {
      return [];
    }

    return [
      ...overview.countries.flatMap(country => country.stickers),
      ...overview.specialSections.flatMap(section => section.stickers)
    ];
  });

  load(): void {
    this.loading.set(true);
    let params = new HttpParams();
    const filters = this.filters();

    Object.entries(filters).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach(item => {
          params = params.append(key, item);
        });
      } else if (value !== '') {
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
        if (this.availableCountries().length === 0) {
          this.availableCountries.set(
            value.countries
              .map(country => ({
                countryCode: country.countryCode,
                countryName: country.countryName,
                flagCode: country.flagCode,
                group: country.group,
                displayOrder: country.displayOrder,
                displayOrderGroup: country.displayOrderGroup
              }))
              .sort((a, b) => a.displayOrder - b.displayOrder || a.countryName.localeCompare(b.countryName))
          );
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadCountryCatalog(): void {
    if (this.availableCountries().length > 0) {
      return;
    }

    this.http.get<AlbumOverview>(`${this.config.apiBaseUrl}/album`).subscribe({
      next: value => {
        this.availableCountries.set(
          value.countries
            .map(country => ({
              countryCode: country.countryCode,
              countryName: country.countryName,
              flagCode: country.flagCode,
              group: country.group,
              displayOrder: country.displayOrder,
              displayOrderGroup: country.displayOrderGroup
            }))
            .sort((a, b) => a.displayOrder - b.displayOrder || a.countryName.localeCompare(b.countryName))
        );
      }
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

  saveSticker(payload: {
    stickerId: string;
    isOwned: boolean;
    duplicateCount: number;
    notes: string;
    birthday: string;
    height: string;
    weight: string;
    team: string;
  }): void {
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
