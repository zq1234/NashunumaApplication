// Application/Interfaces/IUserManagementService.cs
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.User;
using NashunumaApp.Domain.Common.DTOs;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<ApiResponse<PaginatedResponse<UserDto>>> GetPagedUsersAsync(
            string currentUsername,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? userType = null,
            bool? isActive = null);

        Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
        Task<ApiResponse<UserDto>> TransferUserLocationAsync(string username, UpdateUserLocationDto locationDto, string modifiedBy);
        Task<ApiResponse<bool>> ToggleUserStatusAsync(string username, bool isActive, string modifiedBy);
        Task<ApiResponse<bool>> BlockUserAsync(string username, string modifiedBy);
        Task<ApiResponse<bool>> UnblockUserAsync(string username, string modifiedBy);

        Task<ApiResponse<PaginatedResponse<UserDto>>> GetUsersByLocationAsync(
            string currentUsername,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ApiResponse<LocationStatsDto>> GetLocationStatisticsAsync(string currentUsername);
        Task<ApiResponse<UserSummaryStatsDto>> GetUserSummaryStatsAsync(string currentUsername);
    }
}