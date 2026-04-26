import { Injectable, signal } from '@angular/core';

export type ConfirmDialogVariant = 'default' | 'danger';

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: ConfirmDialogVariant;
}

interface ConfirmDialogState extends Required<ConfirmDialogOptions> {
  id: number;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  readonly current = signal<ConfirmDialogState | null>(null);

  private nextId = 1;
  private resolver: ((value: boolean) => void) | null = null;

  confirm(options: ConfirmDialogOptions): Promise<boolean> {
    this.resolve(false);

    this.current.set({
      id: this.nextId++,
      title: options.title,
      message: options.message,
      confirmLabel: options.confirmLabel ?? 'Confirmar',
      cancelLabel: options.cancelLabel ?? 'Cancelar',
      variant: options.variant ?? 'default'
    });

    return new Promise<boolean>(resolve => {
      this.resolver = resolve;
    });
  }

  accept(): void {
    this.resolve(true);
  }

  dismiss(): void {
    this.resolve(false);
  }

  private resolve(value: boolean): void {
    const resolver = this.resolver;
    this.resolver = null;
    this.current.set(null);
    resolver?.(value);
  }
}
