using NashunumaApp.Application.DTOs.Stock;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Application.DTOs.FoodStock
{
    public class NextRequiredDateDto
    {
        public string SiteId { get; set; }
        public string TodayDate { get; set; }
        public bool HasMissingDates { get; set; }
        public List<MissingStockDateDto> MissingDates { get; set; }
        public string NextRequiredDate { get; set; }
        public string NextRequiredDateDisplay { get; set; }
        public int TotalMissingCount { get; set; }
    }
}
