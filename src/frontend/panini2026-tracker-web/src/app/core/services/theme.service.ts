import { DOCUMENT } from '@angular/common';
import { computed, effect, inject, Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'panini-2026-theme';
  private readonly currentTheme = signal<'light' | 'dark'>((localStorage.getItem(this.storageKey) as 'light' | 'dark') ?? 'dark');

  readonly isDark = computed(() => this.currentTheme() === 'dark');

  constructor() {
    effect(() => {
      const theme = this.currentTheme();
      this.document.documentElement.dataset['theme'] = theme;
      localStorage.setItem(this.storageKey, theme);
    });
  }

  toggle(): void {
    this.currentTheme.update(theme => theme === 'dark' ? 'light' : 'dark');
  }
}
