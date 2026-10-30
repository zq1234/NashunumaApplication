// Application/DTOs/FoodStock/FoodStockDto.cs
namespace NashunumaApp.Application.DTOs.FoodStock
{
    public sealed class UserScope
    {
        public bool IsAdmin { get; init; }
        public string Username { get; init; } = string.Empty;
        public decimal UserSiteId { get; init; }
        public IReadOnlyList<SiteRef> ActiveSites { get; init; } = Array.Empty<SiteRef>();
        public HashSet<string> SiteIdSet { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public bool CanSeeAllRows => IsAdmin;
    }

    public sealed class SiteRef
    {
        public decimal Id { get; init; }
        public string? Province { get; init; }
    }
}