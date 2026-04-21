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

  protected search = '';
  protected countryCode = '';
  protected isOwned = '';
  protected hasImage = '';
  protected hasDuplicates = '';

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
}
