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
  protected countryCodes: string[] = [];
  protected countrySearch = '';
  protected isOwned = '';
  protected hasImage = '';
  protected hasDuplicates = '';
  protected openFilter: 'countryCode' | 'isOwned' | 'hasImage' | 'hasDuplicates' | null = null;

  constructor() {
    this.store.loadCountryCatalog();
    this.store.load();
  }

  protected applyFilters(): void {
    this.store.setFilters({
      search: this.search,
      countryCodes: this.countryCodes,
      isOwned: this.isOwned,
      hasImage: this.hasImage,
      hasDuplicates: this.hasDuplicates
    });
  }

  protected toggleFilter(filter: 'countryCode' | 'isOwned' | 'hasImage' | 'hasDuplicates'): void {
    this.openFilter = this.openFilter === filter ? null : filter;
    if (this.openFilter !== 'countryCode') {
      this.countrySearch = '';
    }
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
    this.countrySearch = '';
  }

  protected getFilterLabel(filter: 'isOwned' | 'hasImage' | 'hasDuplicates'): string {
    return this.filterOptions[filter].find(option => option.value === this[filter])?.label ?? this.filterOptions[filter][0].label;
  }

  protected get countryOptions(): { value: string; label: string }[] {
    const options = this.store.availableCountries().map(country => ({
      value: country.countryCode,
      label: country.countryName
    }));

    const search = this.countrySearch.trim().toLowerCase();
    if (!search) {
      return options;
    }

    return options.filter(option => option.label.toLowerCase().includes(search));
  }

  protected get countryLabel(): string {
    if (this.countryCodes.length === 0) {
      return 'País';
    }

    if (this.countryCodes.length === 1) {
      return this.countryOptions.find(option => option.value === this.countryCodes[0])?.label ?? 'País';
    }

    return `${this.countryCodes.length} países`;
  }

  protected toggleCountry(code: string): void {
    this.countryCodes = this.countryCodes.includes(code)
      ? this.countryCodes.filter(current => current !== code)
      : [...this.countryCodes, code];
    this.applyFilters();
  }

  protected clearCountries(): void {
    this.countryCodes = [];
    this.applyFilters();
  }
}
