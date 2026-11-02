// Infrastructure/Repositories/UserManagementRepository.cs
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class UserManagementRepository : GenericRepository<NthUser>, IUserManagementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserScopeService _scopeService;

        public UserManagementRepository(ApplicationDbContext context, IUserScopeService scopeService) : base(context)
        {
            _scopeService = scopeService;
            _context = context;
        }

        // ------------------------------------------------------------------
        // Paged list
        // ------------------------------------------------------------------
        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetPagedUsersWithLocationAsync(
            string currentUsername,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? userType = null,
            bool? isActive = null)
        {
            try
            {
                var caller = await _scopeService.ResolveAsync(currentUsername);
                if (caller == null)
                    return (new List<UserWithLocationDto>(), 0);
                if (!caller.IsAdmin && caller.ActiveSites.Count == 0)
                    return (new List<UserWithLocationDto>(), 0);

                var isAdmin = caller.IsAdmin;
                var activeSiteIds = caller.ActiveSites.Select(s => s.Id).ToList();   // List<decimal>

                var baseQuery = _context.NthUsers
                    .Where(user => isAdmin ? true : (user.Username == currentUsername
                    /*|| (user.SiteId.HasValue && activeSiteIds.Contains(user.SiteId.Value))*/));
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var s = searchTerm.ToLower();
                    baseQuery = baseQuery.Where(user =>
                        (user.Username != null && user.Username.ToLower().Contains(s)) ||
                        (user.Personname != null && user.Personname.ToLower().Contains(s)) ||
                        (user.Email != null && user.Email.ToLower().Contains(s)) ||
                        (user.Mobilenumber != null && user.Mobilenumber.ToLower().Contains(s)) ||
                        (user.SiteName != null && user.SiteName.ToLower().Contains(s)) ||
                        (user.Designation != null && user.Designation.ToLower().Contains(s)) ||
                        (user.Isactive != null && user.Isactive.Contains(s)));
                }

                if (!string.IsNullOrWhiteSpace(province))
                    baseQuery = baseQuery.Where(user => user.Province == province);

                if (!string.IsNullOrWhiteSpace(district))
                    baseQuery = baseQuery.Where(user => user.District == district);

                if (!string.IsNullOrWhiteSpace(tehsil))
                    baseQuery = baseQuery.Where(user => user.Tehsil == tehsil);

                if (!string.IsNullOrWhiteSpace(userType))
                    baseQuery = baseQuery.Where(user => user.Usertype == userType);

                if (isActive.HasValue)
                {
                    var activeValue = isActive.Value ? "1" : "0";
                    baseQuery = baseQuery.Where(user => user.Isactive == activeValue);
                }

                var totalCount = await baseQuery.CountAsync();

                var items = await baseQuery
                    .OrderBy(user => user.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(user => new UserWithLocationDto
                    {
                        UserId = user.Userid,
                        Username = user.Username,
                        PersonName = user.Personname,
                        Email = user.Email,
                        Mobilenumber = user.Mobilenumber,
                        Designation = user.Designation,
                        Usertype = user.Usertype,
                        Province = user.Province,
                        District = user.District,
                        Tehsil = user.Tehsil,
                        SiteId = user.SiteId,
                        SiteName = user.SiteName,
                        Isactive = user.Isactive,
                        Isadmin = user.Isadmin,
                        Lastlogindatetime = user.Lastlogindatetime,
                        Imeino = user.Imeino,
                        Macaddress = user.Macaddress,
                        Requestdatetime = user.Requestdatetime,
                        Activedatetime = user.Activedatetime,
                        Activedby = user.Activedby,
                        ChangeType = user.ChangeType,
                        Istransferred = user.Istransferred,
                        SiteAddress = null,
                        SiteContact = null,
                        SiteGeoLocation = null,
                        SiteProvince = null,
                        SiteDistrict = null,
                        SiteTehsil = null,
                        SiteHeadName = null,
                        SiteIsClosed = null,
                        SiteIsMobileSite = null,
                        SiteProvinceNew = null,
                        SiteDistrictNew = null,
                        SiteTehsilNew = null,
                        SiteLatitude = null,
                        SiteLongitude = null
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users with location details: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Single user — keeps its join, not role-scoped
        // ------------------------------------------------------------------
        public async Task<UserWithLocationDto> GetUserWithLocationByUsernameAsync(string username)
        {
            try
            {
                var query = from user in _context.NthUsers
                            join site in _context.NthSiteLocations
                                on user.SiteId equals site.Id into userSite
                            from site in userSite.DefaultIfEmpty()
                            where user.Username == username
                            select new UserWithLocationDto
                            {
                                UserId = user.Userid,
                                Username = user.Username,
                                PersonName = user.Personname,
                                Email = user.Email,
                                Mobilenumber = user.Mobilenumber,
                                Designation = user.Designation,
                                Usertype = user.Usertype,
                                Province = user.Province,
                                District = user.District,
                                Tehsil = user.Tehsil,
                                SiteId = user.SiteId,
                                SiteName = user.SiteName,
                                Isactive = user.Isactive,
                                Isadmin = user.Isadmin,
                                Lastlogindatetime = user.Lastlogindatetime,
                                Imeino = user.Imeino,
                                Macaddress = user.Macaddress,
                                Requestdatetime = user.Requestdatetime,
                                Activedatetime = user.Activedatetime,
                                Activedby = user.Activedby,
                                ChangeType = user.ChangeType,
                                Istransferred = user.Istransferred,
                                SiteAddress = site != null ? site.Address : null,
                                SiteContact = site != null ? site.Contact : null,
                                SiteGeoLocation = site != null ? site.GeoLocation : null,
                                SiteProvince = site != null ? site.Province : null,
                                SiteDistrict = site != null ? site.District : null,
                                SiteTehsil = site != null ? site.Tehsil : null,
                                SiteHeadName = site != null ? site.HeadName : null,
                                SiteIsClosed = site != null ? site.IsClosed : null,
                                SiteIsMobileSite = site != null ? site.IsMobileSite : null,
                                SiteProvinceNew = site != null ? site.ProvinceNew : null,
                                SiteDistrictNew = site != null ? site.DistrictNew : null,
                                SiteTehsilNew = site != null ? site.TehsilNew : null,
                                SiteLatitude = site != null ? site.Latitude : null,
                                SiteLongitude = site != null ? site.Longitude : null
                            };

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with location details: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Mutations — unchanged
        // ------------------------------------------------------------------
        public async Task<bool> UpdateUserLocationAsync(
            string username,
            string province,
            decimal? provinceId,
            string district,
            decimal? districtId,
            string tehsil,
            decimal? tehsilId,
            decimal? siteId,
            string siteName,
            string modifiedBy)
        {
            try
            {
                var user = await _context.NthUsers.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                if (!string.IsNullOrEmpty(province))
                {
                    user.Province = province;
                    user.ProvinceId = provinceId;
                }

                if (!string.IsNullOrEmpty(district))
                {
                    user.District = district;
                    user.DistrictId = districtId;
                }

                if (!string.IsNullOrEmpty(tehsil))
                {
                    user.Tehsil = tehsil;
                    user.TehsilId = tehsilId;
                }

                if (siteId.HasValue)
                {
                    user.SiteId = siteId;
                    user.SiteName = siteName;
                }

                var currentTransferCount = user.Istransferred ?? 0;
                user.Istransferred = currentTransferCount + 1;

                var siteTransferCount = user.TransferCountForSite ?? 0;
                user.TransferCountForSite = siteTransferCount + 1;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user location: {ex.Message}", ex);
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string username, bool isActive, string modifiedBy)
        {
            try
            {
                var user = await _context.NthUsers.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                user.Isactive = isActive ? "true" : "false";
                user.Activedby = modifiedBy;
                user.Activedatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling user status: {ex.Message}", ex);
            }
        }

        public async Task<bool> BlockUserAsync(string username, string modifiedBy)
        {
            try
            {
                var user = await _context.NthUsers.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                user.Isactive = "false";
                user.Isadmin = "false";
                user.Activedby = modifiedBy;
                user.Activedatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error blocking user: {ex.Message}", ex);
            }
        }

        public async Task<bool> UnblockUserAsync(string username, string modifiedBy)
        {
            try
            {
                var user = await _context.NthUsers.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                user.Isactive = "true";
                user.Isadmin = "0";
                user.Activedby = modifiedBy;
                user.Activedatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error unblocking user: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Users by location — scope via _scopeService, no join
        // ------------------------------------------------------------------
        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByLocationAsync(
            string currentUsername,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var caller = await _scopeService.ResolveAsync(currentUsername);
                if (caller == null)
                    return (new List<UserWithLocationDto>(), 0);
                if (!caller.IsAdmin && caller.ActiveSites.Count == 0)
                    return (new List<UserWithLocationDto>(), 0);

                var isAdmin = caller.IsAdmin;
                var activeSiteIds = caller.ActiveSites.Select(s => s.Id).ToList();   // List<decimal>

                var query = _context.NthUsers
                    .Where(user => (user.Isactive == "1" || user.Isactive == "true")
                                   && user.Username != currentUsername
                                   && (isAdmin
                                       || (user.SiteId.HasValue && activeSiteIds.Contains(user.SiteId.Value))));

                if (!string.IsNullOrWhiteSpace(province))
                    query = query.Where(user => user.Province == province);

                if (!string.IsNullOrWhiteSpace(district))
                    query = query.Where(user => user.District == district);

                if (!string.IsNullOrWhiteSpace(tehsil))
                    query = query.Where(user => user.Tehsil == tehsil);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(user => user.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(user => new UserWithLocationDto
                    {
                        UserId = user.Userid,
                        Username = user.Username,
                        PersonName = user.Personname,
                        Email = user.Email,
                        Mobilenumber = user.Mobilenumber,
                        Designation = user.Designation,
                        Usertype = user.Usertype,
                        Province = user.Province,
                        District = user.District,
                        Tehsil = user.Tehsil,
                        SiteId = user.SiteId,
                        SiteName = user.SiteName,
                        Isactive = user.Isactive,
                        Isadmin = user.Isadmin,
                        Lastlogindatetime = user.Lastlogindatetime,
                        Imeino = user.Imeino,
                        Macaddress = user.Macaddress,
                        Requestdatetime = user.Requestdatetime,
                        Activedatetime = user.Activedatetime,
                        Activedby = user.Activedby,
                        ChangeType = user.ChangeType,
                        Istransferred = user.Istransferred,
                        SiteAddress = null,
                        SiteContact = null,
                        SiteGeoLocation = null,
                        SiteProvince = null,
                        SiteDistrict = null,
                        SiteTehsil = null,
                        SiteHeadName = null,
                        SiteIsClosed = null,
                        SiteIsMobileSite = null,
                        SiteProvinceNew = null,
                        SiteDistrictNew = null,
                        SiteTehsilNew = null,
                        SiteLatitude = null,
                        SiteLongitude = null
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users by location: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Location statistics — scope via _scopeService
        // ------------------------------------------------------------------
        public async Task<Dictionary<string, int>> GetLocationStatisticsAsync(string currentUsername)
        {
            try
            {
                var caller = await _scopeService.ResolveAsync(currentUsername);
                if (caller == null)
                    return new Dictionary<string, int>();
                if (!caller.IsAdmin && caller.ActiveSites.Count == 0)
                    return new Dictionary<string, int>();

                var isAdmin = caller.IsAdmin;
                var activeSiteIds = caller.ActiveSites.Select(s => s.Id).ToList();   // List<decimal>

                var query = _context.NthUsers
                    .Where(u => (u.Isactive == "1" || u.Isactive == "true")
                                && u.Username != currentUsername);

                if (!isAdmin)
                {
                    query = query.Where(u => u.SiteId.HasValue && activeSiteIds.Contains(u.SiteId.Value));
                }

                var stats = await query
                    .GroupBy(u => u.Province ?? "Unknown")
                    .Select(g => new { Province = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Province, x => x.Count);

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving location statistics: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Search / role (kept for compat — not role-scoped, not exposed)
        // ------------------------------------------------------------------
        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> SearchUsersAsync(
            string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var query = from user in _context.NthUsers
                        join site in _context.NthSiteLocations
                            on user.SiteId equals site.Id into userSite
                        from site in userSite.DefaultIfEmpty()
                        select new UserWithLocationDto
                        {
                            UserId = user.Userid,
                            Username = user.Username,
                            PersonName = user.Personname,
                            Email = user.Email,
                            Mobilenumber = user.Mobilenumber,
                            Designation = user.Designation,
                            Usertype = user.Usertype,
                            Province = user.Province,
                            District = user.District,
                            Tehsil = user.Tehsil,
                            SiteId = user.SiteId,
                            SiteName = user.SiteName,
                            Isactive = user.Isactive,
                            Isadmin = user.Isadmin,
                            Lastlogindatetime = user.Lastlogindatetime,
                            Imeino = user.Imeino,
                            Macaddress = user.Macaddress,
                            Requestdatetime = user.Requestdatetime,
                            Activedatetime = user.Activedatetime,
                            Activedby = user.Activedby,
                            ChangeType = user.ChangeType,
                            Istransferred = user.Istransferred,
                            SiteAddress = site != null ? site.Address : null,
                            SiteContact = site != null ? site.Contact : null,
                            SiteGeoLocation = site != null ? site.GeoLocation : null,
                            SiteProvince = site != null ? site.Province : null,
                            SiteDistrict = site != null ? site.District : null,
                            SiteTehsil = site != null ? site.Tehsil : null,
                            SiteHeadName = site != null ? site.HeadName : null,
                            SiteIsClosed = site != null ? site.IsClosed : null,
                            SiteIsMobileSite = site != null ? site.IsMobileSite : null,
                            SiteProvinceNew = site != null ? site.ProvinceNew : null,
                            SiteDistrictNew = site != null ? site.DistrictNew : null,
                            SiteTehsilNew = site != null ? site.TehsilNew : null,
                            SiteLatitude = site != null ? site.Latitude : null,
                            SiteLongitude = site != null ? site.Longitude : null
                        };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.Username != null && x.Username.ToLower().Contains(s)) ||
                    (x.PersonName != null && x.PersonName.ToLower().Contains(s)) ||
                    (x.Email != null && x.Email.ToLower().Contains(s)) ||
                    (x.Mobilenumber != null && x.Mobilenumber.ToLower().Contains(s)) ||
                    (x.SiteName != null && x.SiteName.ToLower().Contains(s)) ||
                    (x.Designation != null && x.Designation.ToLower().Contains(s)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByRoleAsync(
            string role, int pageNumber = 1, int pageSize = 10)
        {
            var query = from user in _context.NthUsers
                        join site in _context.NthSiteLocations
                            on user.SiteId equals site.Id into userSite
                        from site in userSite.DefaultIfEmpty()
                        where user.Usertype == role || user.Usertype.ToLower() == role.ToLower()
                        select new UserWithLocationDto
                        {
                            UserId = user.Userid,
                            Username = user.Username,
                            PersonName = user.Personname,
                            Email = user.Email,
                            Mobilenumber = user.Mobilenumber,
                            Designation = user.Designation,
                            Usertype = user.Usertype,
                            Province = user.Province,
                            District = user.District,
                            Tehsil = user.Tehsil,
                            SiteId = user.SiteId,
                            SiteName = user.SiteName,
                            Isactive = user.Isactive,
                            Isadmin = user.Isadmin,
                            Lastlogindatetime = user.Lastlogindatetime,
                            Imeino = user.Imeino,
                            Macaddress = user.Macaddress,
                            Requestdatetime = user.Requestdatetime,
                            Activedatetime = user.Activedatetime,
                            Activedby = user.Activedby,
                            ChangeType = user.ChangeType,
                            Istransferred = user.Istransferred,
                            SiteAddress = site != null ? site.Address : null,
                            SiteContact = site != null ? site.Contact : null,
                            SiteGeoLocation = site != null ? site.GeoLocation : null,
                            SiteProvince = site != null ? site.Province : null,
                            SiteDistrict = site != null ? site.District : null,
                            SiteTehsil = site != null ? site.Tehsil : null,
                            SiteHeadName = site != null ? site.HeadName : null,
                            SiteIsClosed = site != null ? site.IsClosed : null,
                            SiteIsMobileSite = site != null ? site.IsMobileSite : null,
                            SiteProvinceNew = site != null ? site.ProvinceNew : null,
                            SiteDistrictNew = site != null ? site.DistrictNew : null,
                            SiteTehsilNew = site != null ? site.TehsilNew : null,
                            SiteLatitude = site != null ? site.Latitude : null,
                            SiteLongitude = site != null ? site.Longitude : null
                        };

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // ------------------------------------------------------------------
        // Summary statistics — active/inactive, admin/non-admin, transferred
        // ------------------------------------------------------------------
        public async Task<UserSummaryStatsDto> GetUserSummaryStatsAsync(string currentUsername)
        {
            try
            {
                var caller = await _scopeService.ResolveAsync(currentUsername);
                if (caller == null)
                    return new UserSummaryStatsDto();

                if (!caller.IsAdmin && caller.ActiveSites.Count == 0)
                    return new UserSummaryStatsDto();

                var isAdmin = caller.IsAdmin;
                var activeSiteIds = caller.ActiveSites.Select(s => s.Id).ToList();   // List<decimal>

                // Scoped base query — self-excluded, same rules as the paged read.
                var baseQuery = _context.NthUsers
                    .Where(u => u.Username != currentUsername
                                && (isAdmin
                                    /*|| (u.SiteId.HasValue && activeSiteIds.Contains(u.SiteId.Value))*/));

                // Single round-trip: aggregate everything at once.
                var stats = await baseQuery
                    .GroupBy(_ => 1)                        // collapse to one row
                    .Select(g => new UserSummaryStatsDto
                    {
                        TotalUsers = g.Count(),

                        ActiveUsers = g.Count(u => u.Isactive == "True" || u.Isactive == "true"),
                        InactiveUsers = g.Count(u => !(u.Isactive == "True" || u.Isactive == "true")),

                        AdminUsers = g.Count(u => u.Isadmin == "True" || u.Isadmin == "true"),
                        NonAdminUsers = g.Count(u => !(u.Isadmin == "True" || u.Isadmin == "true")),

                        // Total number of transfers across all users (sum of the per-user counter).
                        TransferredUsers = g.Sum(u => u.TransferCountForSite ?? 0),

                        // How many users have been transferred at least once.
                        //TransferredUsers = g.Count(u => u.TransferCountForSite.HasValue && u.TransferCountForSite > 0),

                        // How many users have never been transferred.
                        NotTransferredUsers = g.Count(u => u.TransferCountForSite == null || u.TransferCountForSite == 0)
                    })
                    .FirstOrDefaultAsync();

                // Empty scope → no rows → FirstOrDefaultAsync returns null.
                return stats ?? new UserSummaryStatsDto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user summary statistics: {ex.Message}", ex);
            }
        }
    }
    }