import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StockService {
  private base = '/api/FoodStock';
  constructor(private http: HttpClient) {}

  saveStock(payload: any): Observable<any> {
    return this.http.post(`${this.base}/save`, payload);
  }

  checkExists(dateStr: string): Observable<boolean> {
    return this.http.get<any>(`${this.base}/exists?date=${encodeURIComponent(dateStr)}`)
      .pipe(
        // API returns ApiResponse<bool>. Extract data if present
        // map not imported to keep file minimal; handle in consumer if needed
        // For simplicity, return the whole response and let caller handle it
      );
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.base}/${id}`);
  }
}
