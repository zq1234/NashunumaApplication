using Microsoft.EntityFrameworkCore;                 
using NashunumaApp.Domain.Common;                    
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Interfaces;                // <-- IUserScopeService
using NashunumaApp.Infrastructure.Data;
namespace NashunumaApp.Infrastructure.Repositories
{
    public class UserScopeService : IUserScopeService
    {
        private readonly ApplicationDbContext _context;

        public UserScopeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserScope?> ResolveAsync(string username, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            var userWithSite = await (
                from u in _context.NthUsers
                join s in _context.NthSiteLocations on u.SiteId equals s.Id
                where u.Username == username
                select new
                {
                    u.Username,
                    u.Isadmin,          // VARCHAR2(5): "true" / "false"
                    SiteId = s.Id,
                    s.Province
                }
            ).FirstOrDefaultAsync(ct);

            if (userWithSite == null)
                return null;

            var isAdmin = string.Equals(
                userWithSite.Isadmin?.Trim(), "true",
                StringComparison.OrdinalIgnoreCase);

            var activeSites = isAdmin
                ? await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1)
                    .Select(s => new SiteRef { Id = s.Id, Province = s.Province })
                    .ToListAsync(ct)
                : await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.Id == userWithSite.SiteId)
                    .Select(s => new SiteRef { Id = s.Id, Province = s.Province })
                    .ToListAsync(ct);

            return new UserScope
            {
                IsAdmin = isAdmin,
                Username = userWithSite.Username,
                UserSiteId = userWithSite.SiteId,
                ActiveSites = activeSites,
                SiteIdSet = activeSites.Select(s => s.Id.ToString())
                                         .ToHashSet(StringComparer.OrdinalIgnoreCase)
            };
        }

        public async Task<bool> IsAdminAsync(string username, CancellationToken ct = default)
            => (await ResolveAsync(username, ct))?.IsAdmin ?? false;

        public async Task<HashSet<string>> GetAllowedSiteIdsAsync(string username, CancellationToken ct = default)
            => (await ResolveAsync(username, ct))?.SiteIdSet
               ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Admin -> always true. Non-admin -> enteredBy == username (case-insensitive).
        /// Use everywhere instead of: scope.IsAdmin || f.EnteredBy == scope.Username
        /// </summary>
        public bool CanSeeRow(UserScope scope, string? enteredBy)
        {
            if (scope == null) return false;
            if (scope.IsAdmin) return true;
            return string.Equals(enteredBy, scope.Username, StringComparison.OrdinalIgnoreCase);
        }
    }
}