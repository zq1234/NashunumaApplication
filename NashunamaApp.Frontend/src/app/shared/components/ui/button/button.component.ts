import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { SafeHtmlPipe } from '../../../pipe/safe-html.pipe';

@Component({
  selector: 'app-button',
  imports: [
    CommonModule,
    SafeHtmlPipe,
  ],
  templateUrl: './button.component.html',
  styles: ``,
  host: {

  },
})
export class ButtonComponent {

  @Input() size: 'sm' | 'md' = 'md';
  @Input() variant: 'primary' | 'outline' = 'primary';
  @Input() disabled = false;
  @Input() className = '';
  @Input() startIcon?: string; // SVG or icon class, or use ng-content for more flexibility
  @Input() endIcon?: string;

  @Output() btnClick = new EventEmitter<Event>();

  get sizeClasses(): string {
    return this.size === 'sm'
      ? 'px-3.5 py-2.5 text-xs font-semibold'
      : 'px-4 py-3 text-sm font-semibold';
  }

  get variantClasses(): string {
    return this.variant === 'primary'
      ? 'bg-gradient-to-r from-brand-500 to-brand-600 text-white shadow-sm hover:from-brand-600 hover:to-brand-700 disabled:from-brand-300 disabled:to-brand-300 dark:from-brand-400 dark:to-brand-500'
      : 'bg-white text-gray-700 ring-1 ring-inset ring-gray-200 shadow-sm hover:bg-gray-50 hover:text-gray-900 dark:bg-gray-900 dark:text-gray-200 dark:ring-gray-700 dark:hover:bg-gray-800 dark:hover:text-white';
  }

  get disabledClasses(): string {
    return this.disabled ? 'cursor-not-allowed opacity-50' : '';
  }

  onClick(event: Event) {
    if (!this.disabled) {
      this.btnClick.emit(event);
    }
  }
}
