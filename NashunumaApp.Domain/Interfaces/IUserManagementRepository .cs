// Domain/Interfaces/IUserManagementRepository.cs
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IUserManagementRepository : IGenericRepository<NthUser>
    {
        // Role-scoped + self-excluded:
        //   Admin     -> all users except the caller
        //   Non-admin -> only users on the caller's own site, except the caller
        Task<(List<UserWithLocationDto> Items, int TotalCount)> GetPagedUsersWithLocationAsync(
            string currentUsername,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? userType = null,
            bool? isActive = null);

        Task<UserWithLocationDto> GetUserWithLocationByUsernameAsync(string username);

        Task<bool> UpdateUserLocationAsync(string username, string province, decimal? provinceId,
            string district, decimal? districtId, string tehsil, decimal? tehsilId,
            decimal? siteId, string siteName, string modifiedBy);

        Task<bool> ToggleUserStatusAsync(string username, bool isActive, string modifiedBy);
        Task<bool> BlockUserAsync(string username, string modifiedBy);
        Task<bool> UnblockUserAsync(string username, string modifiedBy);

        // Role-scoped + self-excluded:
        Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByLocationAsync(
            string currentUsername,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10);

        // Role-scoped:
        //   Admin     -> all active users grouped by province (caller excluded)
        //   Non-admin -> only active users on the caller's own site (caller excluded)
        Task<Dictionary<string, int>> GetLocationStatisticsAsync(string currentUsername);
        Task<UserSummaryStatsDto> GetUserSummaryStatsAsync(string currentUsername);
    }
}