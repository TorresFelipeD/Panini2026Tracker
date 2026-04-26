import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'success' | 'error' | 'info';

export interface ToastItem {
  id: number;
  message: string;
  variant: ToastVariant;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly items = signal<ToastItem[]>([]);
  private nextId = 1;

  show(message: string, variant: ToastVariant = 'info', durationMs = 3200): void {
    const toast: ToastItem = {
      id: this.nextId++,
      message,
      variant
    };

    this.items.update(current => [...current, toast]);
    window.setTimeout(() => this.dismiss(toast.id), durationMs);
  }

  success(message: string, durationMs?: number): void {
    this.show(message, 'success', durationMs);
  }

  error(message: string, durationMs = 4200): void {
    this.show(message, 'error', durationMs);
  }

  info(message: string, durationMs?: number): void {
    this.show(message, 'info', durationMs);
  }

  dismiss(id: number): void {
    this.items.update(current => current.filter(item => item.id !== id));
  }
}
