import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlbumStoreService } from '../../../core/services/album.store';
import { StickerCardComponent } from '../components/sticker-card.component';
import { StickerDetailModalComponent } from '../components/sticker-detail-modal.component';

@Component({
  selector: 'app-album-page',
  standalone: true,
  imports: [CommonModule, FormsModule, StickerCardComponent, StickerDetailModalComponent],
  templateUrl: './album-page.component.html',
  styleUrl: './album-page.component.scss'
})
export class AlbumPageComponent {
  protected readonly store = inject(AlbumStoreService);
  protected readonly filterOptions = {
    isOwned: [
      { value: '', label: 'Estado' },
      { value: 'true', label: 'Obtenidas' },
      { value: 'false', label: 'Faltantes' }
    ],
    hasImage: [
      { value: '', label: 'Imagen' },
      { value: 'true', label: 'Con imagen' },
      { value: 'false', label: 'Sin imagen' }
    ],
    hasDuplicates: [
      { value: '', label: 'Repetidas' },
      { value: 'true', label: 'Con repetidas' },
      { value: 'false', label: 'Sin repetidas' }
    ]
  } as const;

  protected search = '';
  protected countryCode = '';
  protected isOwned = '';
  protected hasImage = '';
  protected hasDuplicates = '';
  protected openFilter: 'isOwned' | 'hasImage' | 'hasDuplicates' | null = null;

  constructor() {
    this.store.load();
  }

  protected applyFilters(): void {
    this.store.setFilters({
      search: this.search,
      countryCode: this.countryCode,
      isOwned: this.isOwned,
      hasImage: this.hasImage,
      hasDuplicates: this.hasDuplicates
    });
  }

  protected toggleFilter(filter: 'isOwned' | 'hasImage' | 'hasDuplicates'): void {
    this.openFilter = this.openFilter === filter ? null : filter;
  }

  protected selectFilter(
    filter: 'isOwned' | 'hasImage' | 'hasDuplicates',
    value: string
  ): void {
    this[filter] = value;
    this.openFilter = null;
    this.applyFilters();
  }

  protected closeFilters(): void {
    this.openFilter = null;
  }

  protected getFilterLabel(filter: 'isOwned' | 'hasImage' | 'hasDuplicates'): string {
    return this.filterOptions[filter].find(option => option.value === this[filter])?.label ?? this.filterOptions[filter][0].label;
  }
}
