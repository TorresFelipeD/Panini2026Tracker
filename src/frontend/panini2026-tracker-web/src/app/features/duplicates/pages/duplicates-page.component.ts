import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlbumStoreService } from '../../../core/services/album.store';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { DuplicatesStoreService } from '../../../core/services/duplicates.store';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';

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
  private readonly confirmDialogService = inject(ConfirmDialogService);
  protected search = '';
  protected countryCodes: string[] = [];
  protected countrySearch = '';
  protected countryPickerOpen = false;

  constructor() {
    this.albumStore.loadCountryCatalog();
    this.albumStore.load();
    this.store.load();
  }

  protected get countryOptions(): { value: string; label: string; flagUrl: string }[] {
    const options = this.albumStore.availableCountries().map(country => ({
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
}
