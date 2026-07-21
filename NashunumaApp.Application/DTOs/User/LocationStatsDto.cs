// DTOs/User/LocationStatsDto.cs
using System.Collections.Generic;

namespace NashunumaApp.Application.DTOs.User
{
    public class LocationStatsDto
    {
        public Dictionary<string, int> ProvinceStats { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class LocationCountDto
    {
        public string LocationName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}