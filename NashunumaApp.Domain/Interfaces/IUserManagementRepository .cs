// Domain/Interfaces/IUserManagementRepository.cs
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IUserManagementRepository : IGenericRepository<NthUser>
    {
        Task<(List<UserWithLocationDto> Items, int TotalCount)> GetPagedUsersWithLocationAsync(
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
        Task<(List<UserWithLocationDto> Items, int TotalCount)> GetUsersByLocationAsync(
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<Dictionary<string, int>> GetLocationStatisticsAsync();
    }
}