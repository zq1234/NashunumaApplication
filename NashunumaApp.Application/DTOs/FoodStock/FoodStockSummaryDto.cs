using System;
using System.Collections.Generic;
using System.Text;

// Application/DTOs/FoodStock/FoodStockSummaryDto.cs
namespace NashunumaApp.Application.DTOs.FoodStock
{
    public class FoodStockSummaryDto
    {
        public int TotalSites { get; set; }
        public int NotUpdated { get; set; }
        public int LowStockSites { get; set; }
        public Dictionary<string, int>? StockByProvince { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
