import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
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
  protected readonly store = inject(LogsStoreService);
  protected category = '';
  protected level = '';
  protected search = '';

  constructor() {
    this.store.load();
  }

  protected confirmDelete(): void {
    if (window.confirm('¿Deseas eliminar los logs que coinciden con los filtros actuales?')) {
      this.store.deleteFiltered();
    }
  }
}
