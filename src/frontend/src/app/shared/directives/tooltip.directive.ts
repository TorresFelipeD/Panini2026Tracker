import { DOCUMENT } from '@angular/common';
import {
  Directive,
  ElementRef,
  HostListener,
  Inject,
  Input,
  OnDestroy,
  Renderer2
} from '@angular/core';

@Directive({
  selector: '[appTooltip]',
  standalone: true
})
export class TooltipDirective implements OnDestroy {
  @Input('appTooltip') tooltipText = '';
  @Input() tooltipOffset = 14;

  private tooltipElement: HTMLElement | null = null;
  private readonly tooltipId = `app-tooltip-${Math.random().toString(36).slice(2, 11)}`;
  private removeScrollListener: (() => void) | null = null;
  private removeResizeListener: (() => void) | null = null;

  constructor(
    private readonly elementRef: ElementRef<HTMLElement>,
    private readonly renderer: Renderer2,
    @Inject(DOCUMENT) private readonly document: Document
  ) {}

  @HostListener('mouseenter')
  @HostListener('focus')
  protected showTooltip(): void {
    const content = this.tooltipText.trim();
    if (!content || this.tooltipElement) {
      return;
    }

    const tooltip = this.renderer.createElement('span');
    const arrow = this.renderer.createElement('span');

    this.renderer.addClass(tooltip, 'app-tooltip');
    this.renderer.setAttribute(tooltip, 'role', 'tooltip');
    this.renderer.setAttribute(tooltip, 'id', this.tooltipId);
    this.renderer.setAttribute(arrow, 'aria-hidden', 'true');
    this.renderer.addClass(arrow, 'app-tooltip__arrow');
    this.renderer.appendChild(tooltip, this.renderer.createText(content));
    this.renderer.appendChild(tooltip, arrow);
    this.renderer.appendChild(this.document.body, tooltip);
    this.renderer.setAttribute(this.elementRef.nativeElement, 'aria-describedby', this.tooltipId);

    this.tooltipElement = tooltip;
    this.updatePosition();
    this.bindViewportListeners();

    requestAnimationFrame(() => {
      if (this.tooltipElement) {
        this.renderer.addClass(this.tooltipElement, 'app-tooltip--visible');
      }
    });
  }

  @HostListener('mouseleave')
  @HostListener('blur')
  protected hideTooltip(): void {
    this.destroyTooltip();
  }

  @HostListener('keydown.escape')
  protected onEscape(): void {
    this.destroyTooltip();
  }

  ngOnDestroy(): void {
    this.destroyTooltip();
  }

  private updatePosition(): void {
    if (!this.tooltipElement) {
      return;
    }

    const hostRect = this.elementRef.nativeElement.getBoundingClientRect();
    const tooltipRect = this.tooltipElement.getBoundingClientRect();
    const viewportWidth = this.document.documentElement.clientWidth;
    const left = hostRect.left + (hostRect.width / 2) - (tooltipRect.width / 2);
    const maxLeft = viewportWidth - tooltipRect.width - 12;
    const clampedLeft = Math.min(Math.max(12, left), Math.max(12, maxLeft));
    const top = hostRect.top - tooltipRect.height - this.tooltipOffset;
    const arrowLeft = hostRect.left + (hostRect.width / 2) - clampedLeft;

    this.renderer.setStyle(this.tooltipElement, 'left', `${clampedLeft}px`);
    this.renderer.setStyle(this.tooltipElement, 'top', `${Math.max(12, top)}px`);
    this.renderer.setStyle(this.tooltipElement, '--tooltip-arrow-left', `${arrowLeft}px`);
  }

  private bindViewportListeners(): void {
    if (this.removeScrollListener || this.removeResizeListener) {
      return;
    }

    this.removeScrollListener = this.renderer.listen('window', 'scroll', () => this.updatePosition());
    this.removeResizeListener = this.renderer.listen('window', 'resize', () => this.updatePosition());
  }

  private destroyTooltip(): void {
    if (!this.tooltipElement) {
      return;
    }

    this.removeScrollListener?.();
    this.removeResizeListener?.();
    this.removeScrollListener = null;
    this.removeResizeListener = null;
    this.renderer.removeAttribute(this.elementRef.nativeElement, 'aria-describedby');
    this.renderer.removeChild(this.document.body, this.tooltipElement);
    this.tooltipElement = null;
  }
}
