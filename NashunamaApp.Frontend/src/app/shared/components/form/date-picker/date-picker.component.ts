
import { Component, Input, Output, EventEmitter, ElementRef, ViewChild, SimpleChanges } from '@angular/core';
import flatpickr from 'flatpickr';
import { LabelComponent } from '../label/label.component';
import "flatpickr/dist/flatpickr.css";

@Component({
  selector: 'app-date-picker',
  imports: [LabelComponent],
  templateUrl: './date-picker.component.html',
  styles: ``
})
export class DatePickerComponent {

  @Input() id!: string;
  @Input() mode: 'single' | 'multiple' | 'range' | 'time' = 'single';
  @Input() defaultDate?: string | Date | string[] | Date[];
  @Input() value?: string | null;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Output() dateChange = new EventEmitter<any>();

  @ViewChild('dateInput', { static: false }) dateInput!: ElementRef<HTMLInputElement>;

  private flatpickrInstance: flatpickr.Instance | undefined;

  ngAfterViewInit() {
    this.flatpickrInstance = flatpickr(this.dateInput.nativeElement, {
      mode: this.mode,
      static: true,
      monthSelectorType: 'static',
      dateFormat: 'Y-m-d',
      defaultDate: this.value ?? this.defaultDate,
      onChange: (selectedDates, dateStr, instance) => {
        this.dateChange.emit({ selectedDates, dateStr, instance });
      }
    });

    if (this.value) {
      this.dateInput.nativeElement.value = this.value;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['value'] && this.dateInput?.nativeElement) {
      this.dateInput.nativeElement.value = this.value ?? '';
      if (this.flatpickrInstance) {
        this.flatpickrInstance.setDate(this.value ?? '', false);
      }
    }
  }

  ngOnDestroy() {
    if (this.flatpickrInstance) {
      this.flatpickrInstance.destroy();
    }
  }
}
