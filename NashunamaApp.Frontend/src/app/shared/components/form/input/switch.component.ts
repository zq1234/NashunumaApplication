import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-switch',
  imports: [
    CommonModule
  ],
  template: `
   <label
      class="flex cursor-pointer select-none items-center gap-3 text-sm font-medium"
      [ngClass]="disabled ? 'text-gray-400' : 'text-gray-700 dark:text-gray-400'"
      (click)="handleToggle()"
    >
      <div class="relative">
        <div
          class="block transition duration-150 ease-linear h-6 w-11 rounded-full"
          [ngClass]="
            (disabled
              ? 'bg-gray-100 pointer-events-none dark:bg-gray-800'
              : switchColors.background)
          "
        ></div>
        <div
          class="absolute left-0.5 top-0.5 h-5 w-5 rounded-full shadow-theme-sm duration-150 ease-linear transform"
          [ngClass]="switchColors.knob"
        ></div>
      </div>
      {{ label }}
    </label>
  `
})
export class SwitchComponent implements OnInit, OnChanges {

  @Input() label!: string;
  @Input() defaultChecked: boolean = false;
  /** Controlled checked input. If provided (not null), the component acts as a controlled input and
   *  will not update its internal state on click — it will emit valueChange and rely on the parent
   *  to update the checked value. */
  @Input() checked: boolean | null = null;
  @Input() disabled: boolean = false;
  @Input() color: 'blue' | 'gray' = 'blue';

  @Output() valueChange = new EventEmitter<boolean>();

  isChecked: boolean = false;

  ngOnInit() {
    this.isChecked = this.checked !== null ? this.checked : this.defaultChecked;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['checked'] && this.checked !== null) {
      this.isChecked = this.checked;
    }
  }

  handleToggle() {
    if (this.disabled) return;
    const next = !this.isChecked;
    // If controlled (checked input provided), do not update internal state here — parent will update
    if (this.checked === null) {
      this.isChecked = next;
    }
    this.valueChange.emit(next);
  }

  get switchColors() {
    if (this.color === 'blue') {
      return {
        background: this.isChecked
          ? 'bg-brand-500'
          : 'bg-gray-200 dark:bg-white/10',
        knob: this.isChecked
          ? 'translate-x-full bg-white'
          : 'translate-x-0 bg-white',
      };
    } else {
      return {
        background: this.isChecked
          ? 'bg-gray-800 dark:bg-white/10'
          : 'bg-gray-200 dark:bg-white/10',
        knob: this.isChecked
          ? 'translate-x-full bg-white'
          : 'translate-x-0 bg-white',
      };
    }
  }
}
