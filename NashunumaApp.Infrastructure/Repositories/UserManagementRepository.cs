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

        public UserManagementRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetPagedUsersWithLocationAsync(
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
                // Build query with join first
                var baseQuery = from user in _context.NthUsers
                                join site in _context.NthSiteLocations
                                    on user.SiteId equals site.Id into userSite
                                from site in userSite.DefaultIfEmpty()
                                where site.IsActive == 1
                                select new { user, site };

                // Apply simple search on user fields
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    baseQuery = baseQuery.Where(x =>
                        (x.user.Username != null && x.user.Username.ToLower().Contains(searchTerm)) ||
                        (x.user.Personname != null && x.user.Personname.ToLower().Contains(searchTerm)) ||
                        (x.user.Email != null && x.user.Email.ToLower().Contains(searchTerm)) ||
                        (x.user.Mobilenumber != null && x.user.Mobilenumber.ToLower().Contains(searchTerm)) ||
                        (x.user.SiteName != null && x.user.SiteName.ToLower().Contains(searchTerm)) ||
                        (x.user.Designation != null && x.user.Designation.ToLower().Contains(searchTerm))
                    );
                }

                // Apply location filters
                if (!string.IsNullOrWhiteSpace(province))
                {
                    baseQuery = baseQuery.Where(x => x.user.Province == province);
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    baseQuery = baseQuery.Where(x => x.user.District == district);
                }

                if (!string.IsNullOrWhiteSpace(tehsil))
                {
                    baseQuery = baseQuery.Where(x => x.user.Tehsil == tehsil);
                }

                if (!string.IsNullOrWhiteSpace(userType))
                {
                    baseQuery = baseQuery.Where(x => x.user.Usertype == userType);
                }

                if (isActive.HasValue)
                {
                    var activeValue = isActive.Value ? "1" : "0";
                    baseQuery = baseQuery.Where(x => x.user.Isactive == activeValue);
                }

                var totalCount = await baseQuery.CountAsync();

                // Project to DTO
                var items = await baseQuery
                    .OrderBy(x => x.user.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new UserWithLocationDto
                    {
                        // User fields
                        UserId = x.user.Userid,
                        Username = x.user.Username,
                        PersonName = x.user.Personname,
                        Email = x.user.Email,
                        Mobilenumber = x.user.Mobilenumber,
                        Designation = x.user.Designation,
                        Usertype = x.user.Usertype,
                        Province = x.user.Province,
                        District = x.user.District,
                        Tehsil = x.user.Tehsil,
                        SiteId = x.user.SiteId,
                        SiteName = x.user.SiteName,
                        Isactive = x.user.Isactive,
                        Isadmin = x.user.Isadmin,
                        Lastlogindatetime = x.user.Lastlogindatetime,
                        Imeino = x.user.Imeino,
                        Macaddress = x.user.Macaddress,
                        Requestdatetime = x.user.Requestdatetime,
                        Activedatetime = x.user.Activedatetime,
                        Activedby = x.user.Activedby,
                        ChangeType = x.user.ChangeType,
                        Istransferred = x.user.Istransferred,
                        // Site Location fields
                        SiteAddress = x.site != null ? x.site.Address : null,
                        SiteContact = x.site != null ? x.site.Contact : null,
                        SiteGeoLocation = x.site != null ? x.site.GeoLocation : null,
                        SiteProvince = x.site != null ? x.site.Province : null,
                        SiteDistrict = x.site != null ? x.site.District : null,
                        SiteTehsil = x.site != null ? x.site.Tehsil : null,
                        SiteHeadName = x.site != null ? x.site.HeadName : null,
                        SiteIsClosed = x.site != null ? x.site.IsClosed : null,
                        SiteIsMobileSite = x.site != null ? x.site.IsMobileSite : null,
                        SiteProvinceNew = x.site != null ? x.site.ProvinceNew : null,
                        SiteDistrictNew = x.site != null ? x.site.DistrictNew : null,
                        SiteTehsilNew = x.site != null ? x.site.TehsilNew : null,
                        SiteLatitude = x.site != null ? x.site.Latitude : null,
                        SiteLongitude = x.site != null ? x.site.Longitude : null
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users with location details: {ex.Message}", ex);
            }
        }

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
                if (user == null)
                    return false;

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

                user.Istransferred = 1;
                user.ChangeType = "Location Transfer";

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
                if (user == null)
                    return false;

                user.Isactive = isActive ? "1" : "0";
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
                if (user == null)
                    return false;

                user.Isactive = "0";
                user.Isadmin = "0";
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

        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByLocationAsync(
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var query = from user in _context.NthUsers
                            join site in _context.NthSiteLocations
                                on user.SiteId equals site.Id into userSite
                            from site in userSite.DefaultIfEmpty()
                            where user.Isactive == "1" || user.Isactive == "true"
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

                if (!string.IsNullOrWhiteSpace(province))
                {
                    query = query.Where(x => x.Province == province);
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    query = query.Where(x => x.District == district);
                }

                if (!string.IsNullOrWhiteSpace(tehsil))
                {
                    query = query.Where(x => x.Tehsil == tehsil);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users by location: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetLocationStatisticsAsync()
        {
            try
            {
                var stats = await _context.NthUsers
                    .Where(u => u.Isactive == "1" || u.Isactive == "true")
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

        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> SearchUsersAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
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
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(x =>
                        (x.Username != null && x.Username.ToLower().Contains(searchTerm)) ||
                        (x.PersonName != null && x.PersonName.ToLower().Contains(searchTerm)) ||
                        (x.Email != null && x.Email.ToLower().Contains(searchTerm)) ||
                        (x.Mobilenumber != null && x.Mobilenumber.ToLower().Contains(searchTerm)) ||
                        (x.SiteName != null && x.SiteName.ToLower().Contains(searchTerm)) ||
                        (x.Designation != null && x.Designation.ToLower().Contains(searchTerm))
                    );
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users: {ex.Message}", ex);
            }
        }

        public async Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByRoleAsync(
            string role,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users by role: {ex.Message}", ex);
            }
        }
    }
}