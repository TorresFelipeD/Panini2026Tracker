import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
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
  private readonly confirmDialogService = inject(ConfirmDialogService);
  protected readonly filterOptions = {
    category: [{ value: '', label: 'Todas las categorías' }],
    level: [{ value: '', label: 'Todos los niveles' }]
  };
  protected category = this.store.category();
  protected level = this.store.level();
  protected search = this.store.search();
  protected openFilter: 'category' | 'level' | null = null;
  protected readonly categories = computed(() =>
    [...new Set([...this.defaultCategories, ...this.store.availableCategories()])].sort((a, b) => a.localeCompare(b))
  );
  protected readonly levels = computed(() =>
    [...new Set([...this.defaultLevels, ...this.store.availableLevels()])].sort((a, b) => a.localeCompare(b))
  );

  constructor() {
    this.store.load();
  }

  protected async confirmDelete(): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm({
      title: 'Eliminar logs filtrados',
      message: '¿Deseas eliminar los logs que coinciden con los filtros actuales? Esta acción no se puede deshacer.',
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      variant: 'danger'
    });

    if (confirmed) {
      this.store.deleteFiltered();
    }
  }

  protected toggleFilter(filter: 'category' | 'level'): void {
    this.openFilter = this.openFilter === filter ? null : filter;
  }

  protected selectFilter(filter: 'category' | 'level', value: string): void {
    this[filter] = value;
    this.openFilter = null;
    this.onFiltersChange();
  }

  protected closeFilters(): void {
    this.openFilter = null;
  }

  protected getFilterLabel(filter: 'category' | 'level'): string {
    if (filter === 'category') {
      return this.category || this.filterOptions.category[0].label;
    }

    return this.level || this.filterOptions.level[0].label;
  }

  protected onFiltersChange(): void {
    this.store.updateFilters(this.category, this.level, this.search);
  }

  protected getLevelBadgeClass(level: string): string {
    return `log-badge--${level.toLowerCase() || 'default'}`;
  }
}
