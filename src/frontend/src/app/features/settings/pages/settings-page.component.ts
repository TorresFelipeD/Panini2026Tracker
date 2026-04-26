import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CURRENT_ALBUM_CATALOG_JSON } from '../../../core/constants/album-catalog-seed';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog.service';
import { ToastService } from '../../../core/services/toast.service';
import { getCountryFlagUrl } from '../../../core/utils/country-flag';
import { LogsPageComponent } from '../../logs/pages/logs-page.component';

type PickerType = 'album' | 'language' | null;

interface AlbumOption {
  id: string;
  label: string;
  note: string;
}

interface LanguageOption {
  value: string;
  label: string;
  nativeLabel: string;
  flagCode: string;
}

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule, LogsPageComponent],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss'
})
export class SettingsPageComponent {
  @ViewChild('catalogImportInput') private catalogImportInput?: ElementRef<HTMLInputElement>;
  @ViewChild('databaseImportInput') private databaseImportInput?: ElementRef<HTMLInputElement>;

  private readonly toastService = inject(ToastService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  protected readonly albumOptions: AlbumOption[] = [
    { id: 'panini-2026', label: 'Álbum Panini 2026', note: 'Progreso principal activo' },
    { id: 'panini-2026-alt', label: 'Álbum Panini 2026 · Segundo progreso', note: 'Cascarón para perfiles adicionales' }
  ];

  protected readonly languageOptions: LanguageOption[] = [
    { value: 'es-CO', label: 'Español', nativeLabel: 'Colombia', flagCode: 'co' },
    { value: 'en-US', label: 'English', nativeLabel: 'USA', flagCode: 'us' },
    { value: 'pt-BR', label: 'Portuguese', nativeLabel: 'Brasil', flagCode: 'br' }
  ];

  protected selectedAlbumId = this.albumOptions[0].id;
  protected selectedLanguage = this.languageOptions[0];
  protected pendingAlbumName = '';
  protected catalogModalOpen = false;
  protected logsModalOpen = false;
  protected openPicker: PickerType = null;
  protected catalogDraft = this.formatJson(CURRENT_ALBUM_CATALOG_JSON);
  protected importedDatabaseName = '';

  protected togglePicker(type: Exclude<PickerType, null>): void {
    this.openPicker = this.openPicker === type ? null : type;
  }

  protected closePickers(): void {
    this.openPicker = null;
  }

  protected selectAlbum(option: AlbumOption): void {
    this.selectedAlbumId = option.id;
    this.closePickers();
  }

  protected selectLanguage(option: LanguageOption): void {
    this.selectedLanguage = option;
    this.closePickers();
  }

  protected openCatalogModal(): void {
    this.catalogDraft = this.formatJson(this.catalogDraft);
    this.catalogModalOpen = true;
  }

  protected closeCatalogModal(): void {
    this.catalogModalOpen = false;
  }

  protected openLogsModal(): void {
    this.logsModalOpen = true;
  }

  protected closeLogsModal(): void {
    this.logsModalOpen = false;
  }

  protected formatCatalogDraft(): void {
    this.catalogDraft = this.formatJson(this.catalogDraft);
    this.toastService.success('JSON formateado correctamente.');
  }

  protected downloadCatalogDraft(): void {
    this.downloadFile('album-catalog.json', this.catalogDraft, 'application/json');
    this.toastService.info('Descarga de album-catalog.json iniciada.');
  }

  protected triggerCatalogImport(): void {
    this.catalogImportInput?.nativeElement.click();
  }

  protected triggerDatabaseImport(): void {
    this.databaseImportInput?.nativeElement.click();
  }

  protected async resetProgress(): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm({
      title: 'Restablecer progreso',
      message: 'Este cascarón limpiará el progreso del álbum activo cuando conectemos la funcionalidad. ¿Quieres continuar?',
      confirmLabel: 'Restablecer',
      cancelLabel: 'Cancelar',
      variant: 'danger'
    });

    if (!confirmed) {
      return;
    }

    this.toastService.info('Cascarón listo: luego conectamos el restablecimiento real del progreso.');
  }

  protected createAlbum(): void {
    const name = this.pendingAlbumName.trim();
    if (!name) {
      this.toastService.error('Escribe un nombre antes de crear un nuevo álbum.');
      return;
    }

    this.toastService.success(`Cascarón listo para crear "${name}".`);
    this.pendingAlbumName = '';
  }

  protected applyAlbumSelection(): void {
    this.toastService.info(`Álbum seleccionado: ${this.selectedAlbum.label}.`);
  }

  protected applyLanguageSelection(): void {
    this.toastService.info(`Idioma preparado: ${this.selectedLanguage.label} (${this.selectedLanguage.nativeLabel}).`);
  }

  protected downloadDatabase(): void {
    const dump = JSON.stringify({
      exportedAt: new Date().toISOString(),
      albumId: this.selectedAlbumId,
      note: 'Cascarón de exportación de base de datos'
    }, null, 2);

    this.downloadFile('panini2026-backup.json', dump, 'application/json');
    this.toastService.info('Descarga de la base de datos preparada.');
  }

  protected async onCatalogImported(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) {
      return;
    }

    try {
      this.catalogDraft = this.formatJson(await file.text());
      this.toastService.success(`JSON importado: ${file.name}`);
    } catch {
      this.toastService.error('El archivo JSON no es válido.');
    } finally {
      (event.target as HTMLInputElement).value = '';
    }
  }

  protected async onDatabaseImported(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) {
      return;
    }

    this.importedDatabaseName = file.name;
    this.toastService.success(`Base de datos cargada para revisión: ${file.name}`);
    (event.target as HTMLInputElement).value = '';
  }

  protected get selectedAlbum(): AlbumOption {
    return this.albumOptions.find(option => option.id === this.selectedAlbumId) ?? this.albumOptions[0];
  }

  protected getCountryFlagUrl(flagCode: string): string {
    return getCountryFlagUrl(flagCode);
  }

  private formatJson(source: string): string {
    return JSON.stringify(JSON.parse(source), null, 2);
  }

  private downloadFile(name: string, content: string, type: string): void {
    const blob = new Blob([content], { type });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = name;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
