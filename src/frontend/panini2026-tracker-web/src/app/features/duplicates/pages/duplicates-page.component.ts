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
}
