// src/app/pages/food-stock/food-stock-detail/food-stock-detail.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FoodStockService } from '@shared/services/food-stock.service';
import { FoodStock } from '@core/models/food-stock.model';
import { PageBreadcrumbComponent } from '@shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { ComponentCardComponent } from '@shared/components/common/component-card/component-card.component';
import { ButtonComponent } from '@shared/components/ui/button/button.component';

@Component({
  selector: 'app-food-stock-detail',
  standalone: true,
  imports: [
    CommonModule,
    PageBreadcrumbComponent,
    ComponentCardComponent,
    ButtonComponent
  ],
  templateUrl: './food-stock-detail.component.html',
  styleUrls: ['./food-stock-detail.component.scss']
})
export class FoodStockDetailComponent implements OnInit {
  stock: FoodStock | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private foodStockService: FoodStockService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.loadStock(id);
    } else {
      this.error = 'No stock ID provided';
    }
  }

  loadStock(id: number): void {
    this.loading = true;
    this.error = null;
    
    this.foodStockService.getFoodStockById(id).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.isSuccess && response.data) {
          this.stock = response.data;
        } else {
          this.error = response.message || 'Failed to load stock details';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred while loading stock details';
        console.error('Error loading stock:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/foodstock']);
  }

  onEdit(): void {
    if (this.stock?.id) {
      this.router.navigate(['/foodstock/edit', this.stock.id]);
    }
  }

  onPrint(): void {
    window.print();
  }

  // Helper methods for statistics
  getTotalStock(): number {
    if (!this.stock) return 0;
    const stocks = [
      this.stock.openingStockBoxesMamta,
      this.stock.openingStockBoxesWawa,
      this.stock.openingStockSachetsMamta,
      this.stock.openingStockSachetsWawa
    ];
    return stocks.reduce((sum, val) => sum + (parseInt(val || '0') || 0), 0);
  }

  getTotalReceived(): number {
    if (!this.stock) return 0;
    const stocks = [
      this.stock.receivedStockBoxesMamta,
      this.stock.receivedStockBoxesWawa,
      this.stock.rutfReceived,
      this.stock.ifaReceived,
      this.stock.mmsReceived
    ];
    return stocks.reduce((sum, val) => sum + (parseInt(val || '0') || 0), 0);
  }

  getTotalDistributed(): number {
    if (!this.stock) return 0;
    const stocks = [
      this.stock.distributedBoxesMamta,
      this.stock.distributedBoxesWawa,
      this.stock.distributedSachetsMamta,
      this.stock.distributedSachetsWawa,
      this.stock.rutfDistributed,
      this.stock.ifaDistributed,
      this.stock.mmsDistributed
    ];
    return stocks.reduce((sum, val) => sum + (parseInt(val || '0') || 0), 0);
  }

  getStockTypes(): number {
    if (!this.stock) return 0;
    let count = 0;
    if (parseInt(this.stock.openingStockBoxesMamta || '0') > 0 || 
        parseInt(this.stock.openingStockBoxesWawa || '0') > 0) {
      count++;
    }
    if (parseInt(this.stock.rutfOpening || '0') > 0) count++;
    if (parseInt(this.stock.ifaOpening || '0') > 0) count++;
    if (parseInt(this.stock.mmsOpening || '0') > 0) count++;
    return count || 1;
  }
}
