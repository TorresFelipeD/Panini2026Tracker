import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface HelpTopic {
  id: string;
  label: string;
  route: string;
  description: string;
  steps: string[];
  tips: string[];
}

@Component({
  selector: 'app-help-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './help-page.component.html',
  styleUrl: './help-page.component.scss'
})
export class HelpPageComponent {
  protected readonly searchTerm = signal('');
  protected readonly expandedTopicIds = signal<string[]>(['album']);

  protected readonly topics: HelpTopic[] = [
    {
      id: 'album',
      label: 'Álbum',
      route: '/',
      description: 'Consulta el avance general del álbum, revisa figuritas y controla el progreso de cada sección.',
      steps: [
        'Entra al menú Álbum para ver el tablero principal con tu progreso.',
        'Usa los filtros y estados de las figuritas para marcar cuáles tienes, cuáles faltan o cuáles están repetidas.',
        'Abre el detalle de una figurita cuando necesites revisar información más puntual.'
      ],
      tips: [
        'Ideal para el seguimiento diario del avance general.',
        'Si quieres ubicar una figurita puntual, combina la vista con filtros del álbum.'
      ]
    },
    {
      id: 'duplicates',
      label: 'Repetidas',
      route: '/repetidas',
      description: 'Administra el inventario de figuritas repetidas para saber cuáles puedes intercambiar.',
      steps: [
        'Abre Repetidas desde el menú superior.',
        'Revisa las figuritas marcadas como repetidas y valida la cantidad disponible.',
        'Usa esta vista antes de un intercambio para decidir qué puedes ofrecer.'
      ],
      tips: [
        'Mantener esta sección al día ayuda a no perder oportunidades de cambio.',
        'Puedes cruzarla con el faltante del álbum para priorizar intercambios.'
      ]
    },
    {
      id: 'images',
      label: 'Imágenes',
      route: '/imagenes',
      description: 'Explora las imágenes cargadas en la aplicación para identificar figuritas o revisar material visual.',
      steps: [
        'Ingresa a Imágenes para ver el catálogo visual disponible.',
        'Abre una imagen cuando necesites inspeccionar mejor una figurita o referencia.',
        'Utiliza esta sección como apoyo visual para validar contenido del álbum.'
      ],
      tips: [
        'Es útil cuando necesitas confirmar rápidamente una referencia visual.',
        'Combina esta vista con Álbum para contrastar progreso y material gráfico.'
      ]
    },
    {
      id: 'settings',
      label: 'Configuración',
      route: '/configuraciones',
      description: 'Ajusta idioma, catálogo, respaldo de datos y acciones administrativas del seguimiento.',
      steps: [
        'Abre Configuración para cambiar opciones del álbum activo.',
        'Selecciona idioma, administra respaldos o abre el editor del catálogo según la necesidad.',
        'Usa las acciones con cuidado cuando impliquen importar datos o restablecer progreso.'
      ],
      tips: [
        'Haz una copia de seguridad antes de una importación importante.',
        'Desde aquí también puedes revisar logs del sistema.'
      ]
    },
    {
      id: 'help',
      label: 'Ayuda',
      route: '/ayuda',
      description: 'Encuentra una guía rápida de cada menú, con tarjetas desplegables y un buscador para ubicar conceptos.',
      steps: [
        'Escribe una palabra en el buscador para filtrar tarjetas relacionadas.',
        'Abre o cierra cada card para leer cómo se usa cada módulo.',
        'Cuando busques algo específico, las coincidencias quedan resaltadas dentro del contenido.'
      ],
      tips: [
        'Prueba búsquedas como progreso, repetidas, respaldo o imágenes.',
        'Si el buscador no encuentra resultados, limpia el texto para volver a ver todas las tarjetas.'
      ]
    }
  ];

  protected readonly filteredTopics = computed(() => {
    const query = this.normalizedQuery();
    if (!query.length) {
      return this.topics;
    }

    return this.topics.filter(topic => this.topicMatches(topic, query));
  });

  protected readonly hasActiveSearch = computed(() => this.normalizedQuery().length > 0);

  protected toggleTopic(topicId: string): void {
    this.expandedTopicIds.update(current =>
      current.includes(topicId) ? current.filter(id => id !== topicId) : [...current, topicId]
    );
  }

  protected isExpanded(topicId: string): boolean {
    return this.hasActiveSearch() || this.expandedTopicIds().includes(topicId);
  }

  protected clearSearch(): void {
    this.searchTerm.set('');
  }

  protected trackByTopic(_: number, topic: HelpTopic): string {
    return topic.id;
  }

  protected highlightText(text: string): string {
    const terms = this.searchTokens();
    const escapedText = this.escapeHtml(text);

    if (!terms.length) {
      return escapedText;
    }

    const pattern = new RegExp(`(${terms.map(term => this.escapeRegExp(term)).join('|')})`, 'gi');
    return escapedText.replace(pattern, '<mark>$1</mark>');
  }

  protected topicHasDirectMatch(topic: HelpTopic): boolean {
    const query = this.normalizedQuery();
    return query.length > 0 && this.topicMatches(topic, query);
  }

  private normalizedQuery(): string[] {
    return this.searchTerm()
      .toLocaleLowerCase()
      .trim()
      .split(/\s+/)
      .filter(Boolean);
  }

  private searchTokens(): string[] {
    return [...new Set(this.normalizedQuery())];
  }

  private topicMatches(topic: HelpTopic, query: string[]): boolean {
    const haystack = [
      topic.label,
      topic.route,
      topic.description,
      ...topic.steps,
      ...topic.tips
    ]
      .join(' ')
      .toLocaleLowerCase();

    return query.every(term => haystack.includes(term));
  }

  private escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  private escapeRegExp(value: string): string {
    return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }
}
