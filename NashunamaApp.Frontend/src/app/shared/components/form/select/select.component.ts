
import { Component, Input, Output, EventEmitter, OnInit, AfterViewInit, ElementRef, ViewChild, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';

export interface Option {
  value: any;
  label: string | number;
  labelHtml?: string;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './select.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: SelectComponent,
      multi: true
    }
  ]
})
export class SelectComponent implements OnInit, AfterViewInit, OnChanges, OnDestroy, ControlValueAccessor {
  @Input() options: Option[] = [];
  @Input() placeholder: string = 'Select an option';
  @Input() className: string = '';
  @Input() defaultValue: string = '';
  @Input() value: string = '';
  @Input() disabled: boolean = false;
  /** fields for option objects */
  @Input() valueField: string = 'value';
  @Input() labelField: string = 'label';
  @Input() multiple: boolean = false;
  @Input() allowHtml: boolean = false; // if true, option.labelHtml will be rendered as HTML in select2

  @Output() valueChange = new EventEmitter<any>();
  @ViewChild('selectEl', { static: true }) selectEl!: ElementRef<HTMLSelectElement>;

  private onTouched: () => void = () => {};
  private onChangeFn: (v: any) => void = () => {};

  ngOnInit() {
    if (!this.value && this.defaultValue) {
      this.value = this.defaultValue;
    }
  }

  ngAfterViewInit(): void {
    // Init select2 on native select - using jquery with safety checks
    try {
      const $ = (window as any).$ || (window as any).jQuery;
        if ($ && $(this.selectEl.nativeElement).select2) {
        $(this.selectEl.nativeElement).select2({
          width: '100%',
          multiple: this.multiple,
          templateResult: (data: any) => {
            if (!data.id) return data.text;
            if (this.allowHtml && data.element) {
              const html = data.element.getAttribute('data-html');
              return html ? html : data.text;
            }
            return data.text;
          },
          templateSelection: (data: any) => {
            if (!data.id) return data.text;
            if (this.allowHtml && data.element) {
              const html = data.element.getAttribute('data-html');
              return html ? html : data.text;
            }
            return data.text;
          },
          escapeMarkup: (m: any) => m
        });
        // ensure initial value is set
        if (this.value !== undefined && this.value !== null && this.value !== '') {
          $(this.selectEl.nativeElement).val(this.value).trigger('change.select2');
        }
        // set disabled state
        if (this.disabled) {
          $(this.selectEl.nativeElement).prop('disabled', true).trigger('change.select2');
        }
        // listen to select2 changes
        $(this.selectEl.nativeElement).on('select2:select select2:unselect change', (e: any) => {
          let val: any = (this.selectEl.nativeElement as HTMLSelectElement).value;
          if (this.multiple) {
            // when multiple, value may be comma-separated or jQuery returns array
            const selected = $(this.selectEl.nativeElement).val();
            val = Array.isArray(selected) ? selected : (selected ? String(selected).split(',') : []);
          }
          this.value = val;
          this.valueChange.emit(val);
          this.onChangeFn(val);
        });
      }
    } catch (e) {
      // ignore if select2 or jQuery not available
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // when options change, reinitialize select2 to pick up new options
    try {
      const $ = (window as any).$ || (window as any).jQuery;
      if ($ && $(this.selectEl?.nativeElement).select2) {
        // destroy then reinit
        $(this.selectEl.nativeElement).select2('destroy');
        $(this.selectEl.nativeElement).select2({ width: '100%' });
        if (this.value !== undefined && this.value !== null && this.value !== '') $(this.selectEl.nativeElement).val(this.value).trigger('change.select2');
        if (this.disabled) $(this.selectEl.nativeElement).prop('disabled', true).trigger('change.select2');
      }
    } catch {}
  }

  ngOnDestroy(): void {
    try {
      const $ = (window as any).$ || (window as any).jQuery;
      if ($ && $(this.selectEl?.nativeElement).select2) {
        $(this.selectEl.nativeElement).off('select2:select select2:unselect change');
        $(this.selectEl.nativeElement).select2('destroy');
      }
    } catch {}
  }

  onChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.value = value;
    this.valueChange.emit(value);
    this.onChangeFn(value);
  }

  // -------------------------
  // ControlValueAccessor
  // -------------------------
  writeValue(obj: any): void {
    this.value = obj ?? '';
    try {
      const $ = (window as any).$ || (window as any).jQuery;
      if ($ && $(this.selectEl?.nativeElement).select2) {
        $(this.selectEl.nativeElement).val(this.value).trigger('change.select2');
      }
    } catch {}
  }

  registerOnChange(fn: any): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    try {
      const $ = (window as any).$ || (window as any).jQuery;
      if ($ && $(this.selectEl?.nativeElement).select2) {
        $(this.selectEl.nativeElement).prop('disabled', isDisabled);
        $(this.selectEl.nativeElement).select2();
      } else {
        (this.selectEl.nativeElement as HTMLSelectElement).disabled = isDisabled;
      }
    } catch {}
  }
}
