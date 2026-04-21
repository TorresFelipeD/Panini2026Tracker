import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MetaService } from '../core/services/meta.service';
import { ThemeService } from '../core/services/theme.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  protected readonly metaService = inject(MetaService);
  protected readonly themeService = inject(ThemeService);

  constructor() {
    this.metaService.load();
  }
}
