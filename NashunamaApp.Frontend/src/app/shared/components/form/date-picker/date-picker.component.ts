
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
  @Input() latestEnteredDate: string | null = null;
  @Input() missingDates: string[] = [];
  @Input() onlyDate: string | null = null;
  @Input() maxDate?: string;
  @Output() dateChange = new EventEmitter<any>();

  @ViewChild('dateInput', { static: false }) dateInput!: ElementRef<HTMLInputElement>;

  private flatpickrInstance: flatpickr.Instance | undefined;

  ngAfterViewInit() {
    this.flatpickrInstance = flatpickr(this.dateInput.nativeElement, {
      mode: this.mode,
      static: true,
      monthSelectorType: 'static',
      dateFormat: 'Y-m-d',
      maxDate: this.maxDate,
      disable: [(date: Date) => this.isDisabledDate(date)],
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
    if (this.flatpickrInstance && (changes['latestEnteredDate'] || changes['missingDates'] || changes['onlyDate'] || changes['maxDate'])) {
      this.flatpickrInstance.set('maxDate', this.maxDate ?? '');
      this.flatpickrInstance.set('disable', [(date: Date) => this.isDisabledDate(date)]);
    }

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

  private isDisabledDate(date: Date): boolean {
    const dateKey = this.toDateKey(date);
    const maxDateKey = this.maxDate === 'today'
      ? this.toDateKey(new Date())
      : this.maxDate;

    if (maxDateKey && dateKey > maxDateKey) {
      return true;
    }

    if (this.onlyDate) {
      return dateKey !== this.onlyDate;
    }

    return !!this.latestEnteredDate &&
      dateKey < this.latestEnteredDate &&
      !this.missingDates.includes(dateKey);
  }

  private toDateKey(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
