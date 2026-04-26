import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CountryAlbum, SpecialStickerSection, StickerCard } from '../../../core/models/app.models';
import { AlbumStoreService } from '../../../core/services/album.store';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';
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
  protected readonly worldCupGroups = ['FCW', ...Array.from({ length: 12 }, (_, index) => `Grupo ${String.fromCharCode(65 + index)}`), 'Otros'];
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
  protected selectedGroup = 'FCW';
  protected showScrollTopButton = false;
  protected openFilter: 'countryCode' | 'isOwned' | 'hasImage' | 'hasDuplicates' | null = null;

  constructor() {
    this.store.loadCountryCatalog();
    this.store.load();
  }

  protected applyFilters(): void {
    this.restoreDefaultGroupWhenNoFilters();
    this.store.setFilters({
      search: this.search,
      countryCodes: this.countryCodes,
      isOwned: this.isOwned,
      hasImage: this.hasImage,
      hasDuplicates: this.hasDuplicates
    });
  }

  protected clearAllFilters(): void {
    this.search = '';
    this.countryCodes = [];
    this.countrySearch = '';
    this.isOwned = '';
    this.hasImage = '';
    this.hasDuplicates = '';
    this.selectedGroup = 'FCW';
    this.closeFilters();
    this.applyFilters();
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
    this.restoreDefaultGroupWhenNoFilters();
    this.applyFilters();
  }

  protected closeFilters(): void {
    this.openFilter = null;
    this.countrySearch = '';
  }

  protected getFilterLabel(filter: 'isOwned' | 'hasImage' | 'hasDuplicates'): string {
    return this.filterOptions[filter].find(option => option.value === this[filter])?.label ?? this.filterOptions[filter][0].label;
  }

  protected get countryOptions(): { value: string; label: string; flagUrl: string }[] {
    const options = this.store.availableCountries().map(country => ({
      value: country.countryCode,
      label: country.countryName,
      flagUrl: getCountryFlagUrl(country.flagCode)
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

  protected get countryFlagUrl(): string {
    return this.countryCodes.length === 1
      ? this.countryOptions.find(option => option.value === this.countryCodes[0])?.flagUrl ?? ''
      : '';
  }

  protected getCountryFlagUrl(flagCode: string): string {
    return getCountryFlagUrl(flagCode);
  }

  protected toggleCountry(code: string): void {
    this.countryCodes = this.countryCodes.includes(code)
      ? this.countryCodes.filter(current => current !== code)
      : [...this.countryCodes, code];

    this.syncSelectedGroupWithCountryFilter();
    this.restoreDefaultGroupWhenNoFilters();
    this.closeFilters();
    this.applyFilters();
  }

  protected clearCountries(): void {
    this.countryCodes = [];
    this.restoreDefaultGroupWhenNoFilters();
    this.applyFilters();
  }

  protected selectGroup(group: string): void {
    if (this.areGroupsDisabled) {
      return;
    }

    this.selectedGroup = group;
  }

  protected scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  protected get hasFilterDrivenView(): boolean {
    return Boolean(this.search.trim())
      || this.countryCodes.length > 0
      || Boolean(this.isOwned)
      || Boolean(this.hasImage)
      || Boolean(this.hasDuplicates);
  }

  protected get areGroupsDisabled(): boolean {
    return this.hasFilterDrivenView;
  }

  protected get visibleCountries(): CountryAlbum[] {
    if (this.hasFilterDrivenView) {
      return [...this.filteredCountries]
        .sort((a, b) => a.displayOrder - b.displayOrder || a.countryName.localeCompare(b.countryName));
    }

    if (this.selectedGroup === 'FCW' || this.selectedGroup === 'Otros') {
      return [];
    }

    const normalizedGroup = this.selectedGroup.replace('Grupo ', '');
    return this.filteredCountries
      .filter(country => country.group === normalizedGroup)
      .sort((a, b) => a.displayOrderGroup - b.displayOrderGroup || a.displayOrder - b.displayOrder || a.countryName.localeCompare(b.countryName));
  }

  protected get visibleSpecialSections(): SpecialStickerSection[] {
    if (this.hasFilterDrivenView) {
      return this.filteredSpecialSections;
    }

    if (this.selectedGroup === 'FCW') {
      return this.filteredSpecialSections.filter(section => section.key === 'fcw');
    }

    if (this.selectedGroup === 'Otros') {
      return this.filteredSpecialSections.filter(section => section.key === 'other');
    }

    return [];
  }

  protected get leadingSpecialSections(): SpecialStickerSection[] {
    return this.visibleSpecialSections.filter(section => section.key === 'fcw');
  }

  protected get trailingSpecialSections(): SpecialStickerSection[] {
    return this.visibleSpecialSections.filter(section => section.key !== 'fcw');
  }

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    this.showScrollTopButton = window.scrollY > 520;
  }

  private syncSelectedGroupWithCountryFilter(): void {
    if (this.countryCodes.length > 1) {
      this.selectedGroup = 'FCW';
      return;
    }

    if (this.countryCodes.length !== 1) {
      return;
    }

    const selectedCountry = this.store.availableCountries()
      .find(country => country.countryCode === this.countryCodes[0]);

    if (!selectedCountry?.group) {
      return;
    }

    this.selectedGroup = `Grupo ${selectedCountry.group}`;
  }

  private restoreDefaultGroupWhenNoFilters(): void {
    if (!this.hasFilterDrivenView && this.countryCodes.length === 0) {
      this.selectedGroup = 'FCW';
    }
  }

  private get filteredCountries(): CountryAlbum[] {
    const overview = this.store.overview();
    if (!overview) {
      return [];
    }

    return overview.countries
      .map(country => {
        const stickers = country.stickers.filter(sticker => this.matchesSticker(sticker, country));
        const owned = stickers.filter(sticker => sticker.isOwned).length;
        const total = stickers.length;

        return {
          ...country,
          stickers,
          owned,
          total,
          missing: total - owned,
          completionPercentage: total === 0 ? 0 : Number(((owned / total) * 100).toFixed(1))
        };
      })
      .filter(country => country.stickers.length > 0);
  }

  private get filteredSpecialSections(): SpecialStickerSection[] {
    const overview = this.store.overview();
    if (!overview) {
      return [];
    }

    return overview.specialSections
      .map(section => {
        const stickers = section.stickers.filter(sticker => this.matchesSticker(sticker));
        const owned = stickers.filter(sticker => sticker.isOwned).length;
        const total = stickers.length;

        return {
          ...section,
          stickers,
          owned,
          total,
          missing: total - owned,
          completionPercentage: total === 0 ? 0 : Number(((owned / total) * 100).toFixed(1))
        };
      })
      .filter(section => section.stickers.length > 0);
  }

  private matchesSticker(sticker: StickerCard, country?: CountryAlbum): boolean {
    const normalizedSearch = this.search.trim().toLowerCase();
    const matchesSearch = !normalizedSearch || [
      sticker.stickerCode,
      sticker.displayName,
      sticker.type,
      sticker.countryCode,
      sticker.countryName,
      country?.group ? `grupo ${country.group}` : ''
    ]
      .join(' ')
      .toLowerCase()
      .includes(normalizedSearch);

    const matchesCountry = this.countryCodes.length === 0
      || this.countryCodes.includes(sticker.countryCode);
    const matchesOwned = this.isOwned === ''
      || String(sticker.isOwned) === this.isOwned;
    const matchesImage = this.hasImage === ''
      || String(sticker.hasImage) === this.hasImage;
    const matchesDuplicates = this.hasDuplicates === ''
      || String(sticker.duplicateCount > 0) === this.hasDuplicates;

    return matchesSearch && matchesCountry && matchesOwned && matchesImage && matchesDuplicates;
  }
}
