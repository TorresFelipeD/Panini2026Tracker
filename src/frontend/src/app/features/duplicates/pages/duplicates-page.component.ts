import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DuplicateItem } from '../../../core/models/app.models';
import { AlbumStoreService } from '../../../core/services/album.store';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { DuplicatesStoreService } from '../../../core/services/duplicates.store';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';

interface DuplicateGroup {
  key: string;
  label: string;
  flagUrl: string;
  items: DuplicateItem[];
}

@Component({
  selector: 'app-duplicates-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './duplicates-page.component.html',
  styleUrl: './duplicates-page.component.scss'
})
export class DuplicatesPageComponent {
  protected readonly albumStore = inject(AlbumStoreService);
  protected readonly store = inject(DuplicatesStoreService);
  protected readonly getCountryFlagUrl = getCountryFlagUrl;
  private readonly confirmDialogService = inject(ConfirmDialogService);
  protected search = '';
  protected countryCodes: string[] = [];
  protected countrySearch = '';
  protected countryPickerOpen = false;
  protected showScrollTopButton = false;

  constructor() {
    this.albumStore.loadCountryCatalog();
    this.albumStore.load();
    this.store.load();
  }

  protected get countryOptions(): { value: string; label: string; flagUrl: string }[] {
    const options = [
      ...this.albumStore.availableCountries().map(country => ({
        value: country.countryCode,
        label: country.countryName,
        flagUrl: getCountryFlagUrl(country.flagCode)
      })),
      { value: 'fcw', label: 'FCW', flagUrl: '' },
      { value: 'otros', label: 'Otros', flagUrl: '' }
    ];

    const search = this.countrySearch.trim().toLowerCase();
    if (!search) {
      return options;
    }

    return options.filter(option => option.label.toLowerCase().includes(search));
  }

  protected get groupedItems(): DuplicateGroup[] {
    const groups = new Map<string, DuplicateGroup>();

    for (const item of this.store.items()) {
      const key = item.countryCode || (item.type === 'fcw' ? 'fcw' : 'otros');
      const label = item.countryName || (item.type === 'fcw' ? 'FCW' : 'Otros');
      const flagUrl = item.flagCode ? getCountryFlagUrl(item.flagCode) : '';
      const current = groups.get(key);

      if (current) {
        current.items.push(item);
        continue;
      }

      groups.set(key, {
        key,
        label,
        flagUrl,
        items: [item]
      });
    }

    return Array.from(groups.values());
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

  protected toggleCountryPicker(): void {
    this.countryPickerOpen = !this.countryPickerOpen;
    if (!this.countryPickerOpen) {
      this.countrySearch = '';
    }
  }

  protected toggleCountry(value: string): void {
    this.countryCodes = this.countryCodes.includes(value)
      ? this.countryCodes.filter(current => current !== value)
      : [...this.countryCodes, value];
    this.store.updateFilter(this.search, this.countryCodes);
  }

  protected closeCountryPicker(): void {
    this.countryPickerOpen = false;
    this.countrySearch = '';
  }

  protected clearCountries(): void {
    this.countryCodes = [];
    this.store.updateFilter(this.search, this.countryCodes);
  }

  protected async remove(stickerId: string, displayName: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm({
      title: 'Eliminar repetida',
      message: `¿Seguro que quieres eliminar la repetida de "${displayName}"?`,
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      variant: 'danger'
    });
    if (!confirmed) {
      return;
    }

    this.store.remove(stickerId);
  }

  protected increment(qty: HTMLInputElement): void {
    qty.stepUp();
  }

  protected decrement(qty: HTMLInputElement): void {
    qty.stepDown();
  }

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    this.showScrollTopButton = window.scrollY > 520;
  }

  protected scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
