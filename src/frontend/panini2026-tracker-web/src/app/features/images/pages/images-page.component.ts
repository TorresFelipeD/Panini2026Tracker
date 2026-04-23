import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StickerCard } from '../../../core/models/app.models';
import { AlbumStoreService } from '../../../core/services/album.store';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
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
  private readonly confirmDialogService = inject(ConfirmDialogService);

  protected stickerId = '';
  protected selectedFile: File | null = null;
  protected pickerOpen = false;
  protected pickerSearch = '';

  constructor() {
    this.albumStore.load();
    this.imagesStore.load();
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

  protected get selectedFileName(): string {
    return this.selectedFile?.name ?? 'Ningún archivo seleccionado';
  }

  protected get selectedStickerLabel(): string {
    const sticker = this.albumStore.allStickers().find(item => item.stickerId === this.stickerId);
    return sticker
      ? `${sticker.stickerCode} · ${sticker.displayName} · ${sticker.countryName}`
      : 'Selecciona una lámina';
  }

  protected get filteredStickers(): StickerCard[] {
    const search = this.pickerSearch.trim().toLowerCase();
    const stickers = this.albumStore.allStickers();

    if (!search) {
      return stickers;
    }

    return stickers.filter(sticker =>
      `${sticker.stickerCode} ${sticker.displayName} ${sticker.countryName}`.toLowerCase().includes(search)
    );
  }
}
