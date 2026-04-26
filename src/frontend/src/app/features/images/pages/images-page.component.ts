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
  protected pickerSearch = '';
  protected readonly pageSizeOptions = [5, 10, 20];
  protected pageSize = 5;
  protected currentPage = 1;
  protected showScrollTop = false;

  constructor() {
    this.albumStore.load();
    this.imagesStore.load();
  }

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    this.showScrollTop = window.scrollY > 520;
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  protected togglePicker(): void {
    this.pickerOpen = !this.pickerOpen;
  }

  protected selectSticker(sticker: StickerCard): void {
    this.stickerId = sticker.stickerId;
    this.pickerOpen = false;
    this.pickerSearch = '';
  }

  protected closePicker(): void {
    this.pickerOpen = false;
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
    return Math.max(1, Math.ceil(this.imagesStore.items().length / this.pageSize));
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

  private get paginatedItems(): StickerImageItem[] {
    const safePage = Math.min(this.currentPage, this.totalPages);
    if (safePage !== this.currentPage) {
      this.currentPage = safePage;
    }

    const start = (safePage - 1) * this.pageSize;
    return this.imagesStore.items().slice(start, start + this.pageSize);
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
