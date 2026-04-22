import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { APP_CONFIG } from '../tokens/app-config.token';
import { StickerImageItem } from '../models/app.models';
import { resolveImageUrl } from '../utils/image-url';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class ImagesStoreService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(APP_CONFIG);
  private readonly toastService = inject(ToastService);

  readonly items = signal<StickerImageItem[]>([]);

  load(): void {
    this.http.get<StickerImageItem[]>(`${this.config.apiBaseUrl}/images`).subscribe({
      next: value => this.items.set(
        value.map(item => ({
          ...item,
          imageUrl: resolveImageUrl(this.config.apiBaseUrl, item.imageUrl) ?? item.imageUrl
        }))
      )
    });
  }

  upload(stickerId: string, file: File): void {
    const formData = new FormData();
    formData.append('file', file);

    this.http.post(`${this.config.apiBaseUrl}/images/${stickerId}`, formData).subscribe({
      next: () => {
        this.load();
        this.toastService.success(`Imagen "${file.name}" cargada correctamente.`);
      },
      error: () => {
        this.toastService.error('No se pudo cargar la imagen.');
      }
    });
  }

  remove(stickerId: string): void {
    this.http.delete(`${this.config.apiBaseUrl}/images/${stickerId}`).subscribe({
      next: () => {
        this.load();
        this.toastService.success('Imagen eliminada correctamente.');
      },
      error: () => {
        this.toastService.error('No se pudo eliminar la imagen.');
      }
    });
  }
}
