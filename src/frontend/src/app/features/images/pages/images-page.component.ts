import { CommonModule, DatePipe } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StickerCard, StickerImageItem } from '../../../core/models/app.models';
import { AlbumStoreService } from '../../../core/services/album.store';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ImagesStoreService } from '../../../core/services/images.store';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';

interface ImageGroup {
  key: string;
  label: string;
  flagUrl: string | null;
  items: StickerImageItem[];
}

interface ImageGroupOption {
  value: string;
  label: string;
  flagUrl: string | null;
}

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
  private readonly confirmDialogService = inject(ConfirmDialogService);

  protected stickerId = '';
  protected selectedFile: File | null = null;
  protected pickerOpen = false;
  protected groupFilterOpen = false;
  protected pickerSearch = '';
  protected imageSearch = '';
  protected imageGroupFilter = '';
  protected imageGroupSearch = '';
  protected readonly pageSizeOptions = [5, 10, 20];
  protected pageSize = 5;
  protected currentPage = 1;
  protected showScrollTopButton = false;

  constructor() {
    this.albumStore.load();
    this.imagesStore.load();
  }

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    this.showScrollTopButton = window.scrollY > 520;
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  protected togglePicker(): void {
    this.groupFilterOpen = false;
    this.pickerOpen = !this.pickerOpen;
  }

  protected toggleGroupFilter(): void {
    this.pickerOpen = false;
    this.groupFilterOpen = !this.groupFilterOpen;
    if (!this.groupFilterOpen) {
      this.imageGroupSearch = '';
    }
  }

  protected selectSticker(sticker: StickerCard): void {
    this.stickerId = sticker.stickerId;
    this.pickerOpen = false;
    this.pickerSearch = '';
  }

  protected closePicker(): void {
    this.pickerOpen = false;
  }

  protected closeGroupFilter(): void {
    this.groupFilterOpen = false;
    this.imageGroupSearch = '';
  }

  protected closeOverlays(): void {
    this.pickerOpen = false;
    this.groupFilterOpen = false;
    this.imageGroupSearch = '';
  }

  protected async upload(input: HTMLInputElement): Promise<void> {
    if (!this.selectedFile || !this.stickerId) {
      return;
    }

    const existingImage = this.imagesStore.items().find(item => item.stickerId === this.stickerId);
    if (existingImage) {
      const confirmed = await this.confirmDialogService.confirm({
        title: 'Sobrescribir imagen',
        message: `La lámina "${existingImage.displayName}" ya tiene una imagen asociada. ¿Quieres reemplazarla?`,
        confirmLabel: 'Sobrescribir',
        cancelLabel: 'Cancelar',
        variant: 'danger'
      });

      if (!confirmed) {
        return;
      }
    }

    this.imagesStore.upload(this.stickerId, this.selectedFile);
    this.albumStore.load();
    this.selectedFile = null;
    input.value = '';
  }

  protected async remove(stickerId: string, displayName: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm({
      title: 'Eliminar imagen',
      message: `¿Seguro que quieres eliminar la imagen de "${displayName}"?`,
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      variant: 'danger'
    });
    if (!confirmed) {
      return;
    }

    this.imagesStore.remove(stickerId);
    this.albumStore.load();
  }

  protected changePage(page: number): void {
    this.currentPage = Math.min(Math.max(1, page), this.totalPages);
    this.scrollToGallery();
  }

  protected onPageSizeChange(): void {
    this.currentPage = 1;
  }

  protected onGalleryFiltersChange(): void {
    this.currentPage = 1;
  }

  protected clearGalleryFilters(): void {
    this.imageSearch = '';
    this.imageGroupFilter = '';
    this.imageGroupSearch = '';
    this.groupFilterOpen = false;
    this.onGalleryFiltersChange();
  }

  protected selectImageGroupFilter(value: string): void {
    this.imageGroupFilter = value;
    this.groupFilterOpen = false;
    this.imageGroupSearch = '';
    this.onGalleryFiltersChange();
  }

  protected scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  protected get selectedFileName(): string {
    return this.selectedFile?.name ?? 'Ningún archivo seleccionado';
  }

  protected get selectedStickerLabel(): string {
    const sticker = this.albumStore.allStickers().find(item => item.stickerId === this.stickerId);
    return sticker
      ? `${sticker.stickerCode} · ${sticker.displayName} · ${this.getStickerContextLabel(sticker)}`
      : 'Selecciona una lámina';
  }

  protected get filteredStickers(): StickerCard[] {
    const search = this.pickerSearch.trim().toLowerCase();
    const stickers = this.albumStore.allStickers();

    if (!search) {
      return stickers;
    }

    return stickers.filter(sticker =>
      `${sticker.stickerCode} ${sticker.displayName} ${this.getStickerContextLabel(sticker)}`.toLowerCase().includes(search)
    );
  }

  protected get pagedGroups(): ImageGroup[] {
    const groups = new Map<string, ImageGroup>();

    for (const item of this.paginatedItems) {
      const group = this.getImageGroup(item);
      const existing = groups.get(group.key);
      if (existing) {
        existing.items.push(item);
      } else {
        groups.set(group.key, { ...group, items: [item] });
      }
    }

    return Array.from(groups.values());
  }

  protected get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredImageItems.length / this.pageSize));
  }

  protected get visiblePageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, start + 4);

    for (let page = start; page <= end; page += 1) {
      pages.push(page);
    }

    return pages;
  }

  protected getStickerBadge(sticker: StickerCard): string {
    return this.getStickerContextLabel(sticker);
  }

  protected get imageGroupOptions(): ImageGroupOption[] {
    const groups = new Map<string, ImageGroupOption>();

    for (const item of this.imagesStore.items()) {
      const group = this.getImageGroup(item);
      if (!groups.has(group.key)) {
        groups.set(group.key, { value: group.key, label: group.label, flagUrl: group.flagUrl });
      }
    }

    return Array.from(groups.values()).sort((a, b) => a.label.localeCompare(b.label));
  }

  protected get filteredImageGroupOptions(): ImageGroupOption[] {
    const search = this.imageGroupSearch.trim().toLowerCase();
    if (!search) {
      return this.imageGroupOptions;
    }

    return this.imageGroupOptions.filter(option => option.label.toLowerCase().includes(search));
  }

  protected get selectedImageGroupLabel(): string {
    if (!this.imageGroupFilter) {
      return 'Todos los grupos';
    }

    return this.imageGroupOptions.find(option => option.value === this.imageGroupFilter)?.label ?? 'Todos los grupos';
  }

  protected get selectedImageGroupFlagUrl(): string | null {
    if (!this.imageGroupFilter) {
      return null;
    }

    return this.imageGroupOptions.find(option => option.value === this.imageGroupFilter)?.flagUrl ?? null;
  }

  protected get filteredImagesCount(): number {
    return this.filteredImageItems.length;
  }

  private get paginatedItems(): StickerImageItem[] {
    const safePage = Math.min(this.currentPage, this.totalPages);
    if (safePage !== this.currentPage) {
      this.currentPage = safePage;
    }

    const start = (safePage - 1) * this.pageSize;
    return this.filteredImageItems.slice(start, start + this.pageSize);
  }

  private get filteredImageItems(): StickerImageItem[] {
    const search = this.imageSearch.trim().toLowerCase();

    return this.imagesStore.items().filter(item => {
      const group = this.getImageGroup(item);
      const matchesGroup = !this.imageGroupFilter || group.key === this.imageGroupFilter;
      const matchesSearch = !search || `${item.stickerCode} ${item.displayName} ${group.label} ${item.type}`.toLowerCase().includes(search);

      return matchesGroup && matchesSearch;
    });
  }

  private getImageGroup(item: StickerImageItem): Omit<ImageGroup, 'items'> {
    if (item.countryCode && item.countryName) {
      return {
        key: item.countryCode,
        label: item.countryName,
        flagUrl: getCountryFlagUrl(item.flagCode)
      };
    }

    if (item.type.toLowerCase() === 'fcw') {
      return {
        key: 'fcw',
        label: 'FCW',
        flagUrl: null
      };
    }

    return {
      key: 'otros',
      label: 'Otros',
      flagUrl: null
    };
  }

  private getStickerContextLabel(sticker: StickerCard): string {
    if (sticker.countryName) {
      return sticker.countryName;
    }

    return sticker.type === 'fcw' ? 'FCW' : 'Otros';
  }

  private scrollToGallery(): void {
    const galleryTop = document.querySelector('.gallery')?.getBoundingClientRect().top;
    if (galleryTop === undefined) {
      return;
    }

    window.scrollTo({ top: window.scrollY + galleryTop - 120, behavior: 'smooth' });
  }
}
