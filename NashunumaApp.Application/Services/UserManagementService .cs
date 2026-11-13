// Application/Services/UserManagementService.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.User;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _userManagementRepository;
        private readonly IMapper _mapper;

        public UserManagementService(
            IUserManagementRepository userManagementRepository,
            IMapper mapper)
        {
            _userManagementRepository = userManagementRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PaginatedResponse<UserDto>>> GetPagedUsersAsync(
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
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var result = await _userManagementRepository.GetPagedUsersWithLocationAsync(
                    currentUsername, pageNumber, pageSize,
                    searchTerm, province, district, tehsil, userType, isActive);

                var mappedItems = _mapper.Map<List<UserDto>>(result.Items);

                var paginatedResponse = new PaginatedResponse<UserDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                };

                return ApiResponse<PaginatedResponse<UserDto>>.Success(
                    paginatedResponse, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<UserDto>>.Failure(
                    $"Failed to retrieve users: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ApiResponse<UserDto>.Failure("Username is required");

                var userWithLocation = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);
                if (userWithLocation == null)
                    return ApiResponse<UserDto>.Failure($"User '{username}' not found");

                return ApiResponse<UserDto>.Success(
                    _mapper.Map<UserDto>(userWithLocation), "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.Failure($"Failed to retrieve user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> TransferUserLocationAsync(
            string username, UpdateUserLocationDto locationDto, string modifiedBy)
        {
            UserWithLocationDto? user = null;
            try
            {

                if (string.IsNullOrWhiteSpace(username))
                {
                    //await RecordTransferAuditAsync(username, null, locationDto, modifiedBy, "Failed", "Username is required");
                    return ApiResponse<UserDto>.Failure("Username is required");
                }
                if (string.IsNullOrWhiteSpace(modifiedBy))
                {
                    //await RecordTransferAuditAsync(username, null, locationDto, modifiedBy, "Failed", "Modified by user is required");
                    return ApiResponse<UserDto>.Failure("Modified by user is required");
                }

                if (username == modifiedBy)
                {
                    //await RecordTransferAuditAsync(username, null, locationDto, modifiedBy, "Failed", "You cannot transfer your own location");
                    return ApiResponse<UserDto>.Failure("You cannot transfer your own location");
                }

                user = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);
                if (user == null)
                {
                    //await RecordTransferAuditAsync(username, null, locationDto, modifiedBy, "Failed", $"User '{username}' not found");
                    return ApiResponse<UserDto>.Failure($"User '{username}' not found");
                }

                if (user.Isactive != "1" && user.Isactive != "true")
                {
                    //await RecordTransferAuditAsync(username, user, locationDto, modifiedBy, "Failed", "Cannot transfer inactive user");
                    return ApiResponse<UserDto>.Failure("Cannot transfer inactive user");
                }

                if (IsSameLocation(user, locationDto))
                {
                    return ApiResponse<UserDto>.Failure("User is already assigned to the specified site");
                }

                var updated = await _userManagementRepository.UpdateUserLocationAsync(
                    username,
                    locationDto.Province, locationDto.ProvinceId,
                    locationDto.District, locationDto.DistrictId,
                    locationDto.Tehsil, locationDto.TehsilId,
                    locationDto.SiteId, locationDto.SiteName,
                    modifiedBy);

                if (!updated)
                {
                    //await RecordTransferAuditAsync(username, user, locationDto, modifiedBy, "Failed", "Failed to update user location");
                    return ApiResponse<UserDto>.Failure("Failed to update user location");
                }

                var updatedUser = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);
                await RecordTransferAuditAsync(username, user, locationDto, modifiedBy, "Success", null);
                return ApiResponse<UserDto>.Success( _mapper.Map<UserDto>(updatedUser), "User location transferred successfully");
            }
            catch (Exception ex)
            {
                //await RecordTransferAuditAsync(username, user, locationDto, modifiedBy, "Failed", ex.Message);
                return ApiResponse<UserDto>.Failure($"Failed to transfer user location: {ex.Message}");
            }
        }

        private static bool IsSameLocation(UserWithLocationDto user, UpdateUserLocationDto locationDto)
        {
            var siteId = locationDto.SiteId ?? user.SiteId;
            var siteName = locationDto.SiteId.HasValue ? locationDto.SiteName : user.SiteName;

            return user.SiteId == siteId && string.Equals(user.SiteName, siteName, StringComparison.OrdinalIgnoreCase);
        }

        private async Task RecordTransferAuditAsync( string username, UserWithLocationDto? user,  UpdateUserLocationDto? locationDto,string modifiedBy,string status, string? failureReason)
        {
            try
            {
                await _userManagementRepository.AddTransferAuditAsync(new UserLocationTransferAudit
                {
                    UserId = user?.UserId,
                    Username = username,
                    ModifiedBy = modifiedBy,
                    FromProvince = user?.Province,
                    FromProvinceId=user?.ProvinceId,
                    FromDistrict = user?.District,
                    FromDistrictId=user?.DistrictId,
                    FromTehsilId=user?.TehsilId,
                    FromTehsil = user?.Tehsil,
                    FromSiteId = user?.SiteId,
                    FromSiteName = user?.SiteName,
                    ToProvince = locationDto?.Province,
                    ToProvinceId = locationDto?.ProvinceId,
                    ToDistrict = locationDto?.District,
                    ToDistrictId = locationDto?.DistrictId,
                    ToTehsil = locationDto?.Tehsil,
                    ToTehsilId = locationDto?.TehsilId,
                    ToSiteId = locationDto?.SiteId,
                    ToSiteName = locationDto?.SiteName,
                    TransferredAt = DateTime.UtcNow,
                    Status = status,
                    FailureReason = failureReason,
                    TotalTransferCount = status == "Success" ? (user?.Istransferred ?? 0) + 1 : user?.Istransferred ?? 0
                });
            }
            catch
            {
               
            }
        }

        public async Task<ApiResponse<bool>> ToggleUserStatusAsync(string username, bool isActive, string modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ApiResponse<bool>.Failure("Username is required");
                if (string.IsNullOrWhiteSpace(modifiedBy))
                    return ApiResponse<bool>.Failure("Modified by user is required");

                if (username == modifiedBy)
                    return ApiResponse<bool>.Failure("You cannot change your own status");

                var updated = await _userManagementRepository.ToggleUserStatusAsync(username, isActive, modifiedBy);
                if (!updated)
                    return ApiResponse<bool>.Failure($"User '{username}' not found");

                var message = isActive ? "activated" : "deactivated";
                return ApiResponse<bool>.Success(true, $"User {message} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to toggle user status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> BlockUserAsync(string username, string modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ApiResponse<bool>.Failure("Username is required");
                if (string.IsNullOrWhiteSpace(modifiedBy))
                    return ApiResponse<bool>.Failure("Modified by user is required");

                if (username == modifiedBy)
                    return ApiResponse<bool>.Failure("You cannot block yourself");

                var blocked = await _userManagementRepository.BlockUserAsync(username, modifiedBy);
                if (!blocked)
                    return ApiResponse<bool>.Failure($"User '{username}' not found");

                return ApiResponse<bool>.Success(true, "User blocked successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to block user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UnblockUserAsync(string username, string modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ApiResponse<bool>.Failure("Username is required");
                if (string.IsNullOrWhiteSpace(modifiedBy))
                    return ApiResponse<bool>.Failure("Modified by user is required");

                if (username == modifiedBy)
                    return ApiResponse<bool>.Failure("You cannot unblock yourself");

                var blocked = await _userManagementRepository.UnblockUserAsync(username, modifiedBy);
                if (!blocked)
                    return ApiResponse<bool>.Failure($"User '{username}' not found");

                return ApiResponse<bool>.Success(true, "User unblocked successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to unblock user: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------
        // Role-scoped reads — forward currentUsername to the repository
        // ------------------------------------------------------------------
        public async Task<ApiResponse<PaginatedResponse<UserDto>>> GetUsersByLocationAsync(
            string currentUsername,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var result = await _userManagementRepository.GetUsersByLocationAsync(
                    currentUsername, province, district, tehsil, pageNumber, pageSize);

                var mappedItems = _mapper.Map<List<UserDto>>(result.Items);

                var paginatedResponse = new PaginatedResponse<UserDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                };

                return ApiResponse<PaginatedResponse<UserDto>>.Success(
                    paginatedResponse, "Users retrieved by location successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<UserDto>>.Failure(
                    $"Failed to retrieve users by location: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LocationStatsDto>> GetLocationStatisticsAsync(string currentUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUsername))
                    return ApiResponse<LocationStatsDto>.Failure("Current username is required");

                var stats = await _userManagementRepository.GetLocationStatisticsAsync(currentUsername);

                var total = stats.Values.Sum();

                var dto = new LocationStatsDto
                {
                    TotalUsers = total,
                    ProvinceStats = stats
                };

                return ApiResponse<LocationStatsDto>.Success(
                    dto, "Location statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LocationStatsDto>.Failure(
                    $"Failed to retrieve location statistics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserSummaryStatsDto>> GetUserSummaryStatsAsync(string currentUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUsername))
                    return ApiResponse<UserSummaryStatsDto>.Failure("Current username is required");

                var stats = await _userManagementRepository.GetUserSummaryStatsAsync(currentUsername);

                return ApiResponse<UserSummaryStatsDto>.Success(
                    stats, "User summary statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserSummaryStatsDto>.Failure(
                    $"Failed to retrieve user summary statistics: {ex.Message}");
            }
        }

    }
}