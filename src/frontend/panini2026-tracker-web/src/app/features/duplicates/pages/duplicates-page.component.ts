import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DuplicatesStoreService } from '../../../core/services/duplicates.store';

@Component({
  selector: 'app-duplicates-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './duplicates-page.component.html',
  styleUrl: './duplicates-page.component.scss'
})
export class DuplicatesPageComponent {
  protected readonly store = inject(DuplicatesStoreService);
  protected search = '';
  protected countryCode = '';

  constructor() {
    this.store.load();
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
