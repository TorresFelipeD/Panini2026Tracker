import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlbumStoreService } from '../../../core/services/album.store';
import { ImagesStoreService } from '../../../core/services/images.store';

@Component({
  selector: 'app-images-page',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './images-page.component.html',
  styleUrl: './images-page.component.scss'
})
export class ImagesPageComponent {
  protected readonly albumStore = inject(AlbumStoreService);
  protected readonly imagesStore = inject(ImagesStoreService);

  protected stickerId = '';

  constructor() {
    this.albumStore.load();
    this.imagesStore.load();
  }

  protected upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.stickerId) {
      return;
    }

    this.imagesStore.upload(this.stickerId, file);
    input.value = '';
  }

  protected remove(stickerId: string, displayName: string): void {
    const confirmed = window.confirm(`¿Seguro que quieres eliminar la imagen de "${displayName}"?`);
    if (!confirmed) {
      return;
    }

    this.imagesStore.remove(stickerId);
  }
}
