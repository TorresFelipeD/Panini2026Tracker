import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { StickerCard } from '../../../core/models/app.models';

@Component({
  selector: 'app-sticker-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sticker-card.component.html',
  styleUrl: './sticker-card.component.scss'
})
export class StickerCardComponent {
  @Input({ required: true }) sticker!: StickerCard;
  @Output() select = new EventEmitter<string>();
}
