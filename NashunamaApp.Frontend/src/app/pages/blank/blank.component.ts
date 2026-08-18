
import { Component } from '@angular/core';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ComponentCardComponent } from '../../shared/components/common/component-card/component-card.component';

@Component({
  selector: 'app-blank',
  imports: [
    PageBreadcrumbComponent,
    ComponentCardComponent
],
  templateUrl: './blank.component.html',
  styles: ``
})
export class BlankComponent {

}
