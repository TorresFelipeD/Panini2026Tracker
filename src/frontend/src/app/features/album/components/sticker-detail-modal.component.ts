import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StickerDetail } from '../../../core/models/app.models';
import { TooltipDirective } from '../../../shared/directives/tooltip.directive';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';

@Component({
  selector: 'app-sticker-detail-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TooltipDirective],
  templateUrl: './sticker-detail-modal.component.html',
  styleUrl: './sticker-detail-modal.component.scss'
})
export class StickerDetailModalComponent implements OnChanges {
  @Input() sticker: StickerDetail | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<{
    stickerId: string;
    isOwned: boolean;
    duplicateCount: number;
    notes: string;
    birthday: string;
    height: string;
    weight: string;
    team: string;
  }>();

  protected isOwned = false;
  protected duplicateCount = 0;
  protected notes = '';
  protected birthday = '';
  protected height = '';
  protected weight = '';
  protected team = '';
  protected isImageExpanded = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['sticker'] && this.sticker) {
      this.isOwned = this.sticker.isOwned;
      this.duplicateCount = this.sticker.duplicateCount;
      this.notes = this.sticker.notes ?? '';
      this.birthday = this.sticker.birthday ?? '';
      this.height = this.sticker.height ?? '';
      this.weight = this.sticker.weight ?? '';
      this.team = this.sticker.team ?? '';
      this.isImageExpanded = false;
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
      notes: this.notes,
      birthday: this.birthday,
      height: this.height,
      weight: this.weight,
      team: this.team
    });
  }

  protected get isPlayer(): boolean {
    return this.sticker?.type === 'jugador';
  }

  protected incrementDuplicates(): void {
    this.duplicateCount += 1;
  }

  protected decrementDuplicates(): void {
    this.duplicateCount = Math.max(0, this.duplicateCount - 1);
  }

  protected getCountryFlagUrl(flagCode: string | null): string {
    return getCountryFlagUrl(flagCode);
  }

  protected toggleImageExpanded(): void {
    if (!this.sticker?.imageUrl) {
      return;
    }

    this.isImageExpanded = !this.isImageExpanded;
  }
}
