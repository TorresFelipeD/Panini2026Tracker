import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { MetaService } from '../core/services/meta.service';
import { ThemeService } from '../core/services/theme.service';
import { ConfirmDialogComponent } from './confirm-dialog.component';
import { ToastStackComponent } from './toast-stack.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToastStackComponent, ConfirmDialogComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  protected readonly metaService = inject(MetaService);
  protected readonly themeService = inject(ThemeService);
  protected readonly isNavOpen = signal(false);
  protected readonly navItems = [
    { label: 'Álbum', path: '/', exact: true },
    { label: 'Repetidas', path: '/repetidas', exact: false },
    { label: 'Imágenes', path: '/imagenes', exact: false },
    { label: 'Logs', path: '/logs', exact: false },
    { label: 'Configuracion', path: '/configuraciones', exact: false }
  ] as const;

  private readonly router = inject(Router);

  constructor() {
    this.metaService.load();

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe(() => this.isNavOpen.set(false));
  }

  protected toggleNav(): void {
    this.isNavOpen.update(isOpen => !isOpen);
  }
}
