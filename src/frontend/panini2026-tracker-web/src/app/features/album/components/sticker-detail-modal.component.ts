import { CommonModule, KeyValuePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StickerDetail } from '../../../core/models/app.models';

@Component({
  selector: 'app-sticker-detail-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, KeyValuePipe],
  templateUrl: './sticker-detail-modal.component.html',
  styleUrl: './sticker-detail-modal.component.scss'
})
export class StickerDetailModalComponent implements OnChanges {
  @Input() sticker: StickerDetail | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<{ stickerId: string; isOwned: boolean; duplicateCount: number; notes: string }>();

  protected isOwned = false;
  protected duplicateCount = 0;
  protected notes = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['sticker'] && this.sticker) {
      this.isOwned = this.sticker.isOwned;
      this.duplicateCount = this.sticker.duplicateCount;
      this.notes = this.sticker.notes ?? '';
    }
  }

  protected submit(): void {
    if (!this.sticker) {
      return;
    }

    this.save.emit({
      stickerId: this.sticker.stickerId,
      isOwned: this.isOwned,
      duplicateCount: this.duplicateCount,
      notes: this.notes
    });
  }

  protected incrementDuplicates(): void {
    this.duplicateCount += 1;
  }

  protected decrementDuplicates(): void {
    this.duplicateCount = Math.max(0, this.duplicateCount - 1);
  }
}
