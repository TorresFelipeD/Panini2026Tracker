import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LogsStoreService } from '../../../core/services/logs.store';

@Component({
  selector: 'app-logs-page',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './logs-page.component.html',
  styleUrl: './logs-page.component.scss'
})
export class LogsPageComponent {
  private readonly defaultCategories = ['duplicates', 'images', 'logs', 'seed', 'stickers', 'system'];
  private readonly defaultLevels = ['info', 'warning', 'error'];

  protected readonly store = inject(LogsStoreService);
  protected category = this.store.category();
  protected level = this.store.level();
  protected search = this.store.search();
  protected readonly categories = computed(() =>
    [...new Set([...this.defaultCategories, ...this.store.availableCategories()])].sort((a, b) => a.localeCompare(b))
  );
  protected readonly levels = computed(() =>
    [...new Set([...this.defaultLevels, ...this.store.availableLevels()])].sort((a, b) => a.localeCompare(b))
  );

  constructor() {
    this.store.load();
  }

  protected confirmDelete(): void {
    if (window.confirm('¿Deseas eliminar los logs que coinciden con los filtros actuales?')) {
      this.store.deleteFiltered();
    }
  }

  protected onFiltersChange(): void {
    this.store.updateFilters(this.category, this.level, this.search);
  }
}
