import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/album/pages/album-page.component').then(m => m.AlbumPageComponent)
  },
  {
    path: 'repetidas',
    loadComponent: () => import('./features/duplicates/pages/duplicates-page.component').then(m => m.DuplicatesPageComponent)
  },
  {
    path: 'logs',
    loadComponent: () => import('./features/logs/pages/logs-page.component').then(m => m.LogsPageComponent)
  },
  {
    path: 'imagenes',
    loadComponent: () => import('./features/images/pages/images-page.component').then(m => m.ImagesPageComponent)
  },
  {
    path: 'configuraciones',
    loadComponent: () => import('./features/settings/pages/settings-page.component').then(m => m.SettingsPageComponent)
  },
  {
    path: 'ayuda',
    loadComponent: () => import('./features/help/pages/help-page.component').then(m => m.HelpPageComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
