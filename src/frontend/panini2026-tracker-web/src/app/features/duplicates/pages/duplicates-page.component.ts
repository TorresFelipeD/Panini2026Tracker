import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlbumStoreService } from '../../../core/services/album.store';
import { DuplicatesStoreService } from '../../../core/services/duplicates.store';

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
  protected search = '';
  protected countryCodes: string[] = [];
  protected countryPickerOpen = false;

  constructor() {
    this.albumStore.loadCountryCatalog();
    this.albumStore.load();
    this.store.load();
  }

  protected get countryOptions(): { value: string; label: string }[] {
    return this.albumStore.availableCountries().map(country => ({
      value: country.countryCode,
      label: country.countryName
    }));
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

  protected toggleCountryPicker(): void {
    this.countryPickerOpen = !this.countryPickerOpen;
  }

  protected toggleCountry(value: string): void {
    this.countryCodes = this.countryCodes.includes(value)
      ? this.countryCodes.filter(current => current !== value)
      : [...this.countryCodes, value];
    this.store.updateFilter(this.search, this.countryCodes);
  }

  protected closeCountryPicker(): void {
    this.countryPickerOpen = false;
  }

  protected remove(stickerId: string, displayName: string): void {
    const confirmed = window.confirm(`¿Seguro que quieres eliminar la repetida de "${displayName}"?`);
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
}
