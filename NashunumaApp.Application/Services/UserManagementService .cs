// Application/Services/UserManagementService.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.User;
using NashunumaApp.Application.Interfaces;
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

        public UserManagementService(IUserManagementRepository userManagementRepository, IMapper mapper)
        {
            _userManagementRepository = userManagementRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PaginatedResponse<UserDto>>> GetPagedUsersAsync(
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
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                // Get paged data with location information
                var result = await _userManagementRepository.GetPagedUsersWithLocationAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    province,
                    district,
                    tehsil,
                    userType,
                    isActive
                );

                // Extract items and total count
                var items = result.Items;
                var totalCount = result.TotalCount;

                // Map Domain DTO to Application DTO using AutoMapper
                var mappedItems = _mapper.Map<List<UserDto>>(items);

                // Create paginated response
                var paginatedResponse = new PaginatedResponse<UserDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<UserDto>>.Success(
                    paginatedResponse,
                    "Users retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<UserDto>>.Failure(
                    $"Failed to retrieve users: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return ApiResponse<UserDto>.Failure("Username is required");
                }

                var userWithLocation = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);

                if (userWithLocation == null)
                {
                    return ApiResponse<UserDto>.Failure($"User '{username}' not found");
                }

                var mappedUser = _mapper.Map<UserDto>(userWithLocation);

                return ApiResponse<UserDto>.Success(mappedUser, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.Failure($"Failed to retrieve user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDto>> TransferUserLocationAsync(string username,UpdateUserLocationDto locationDto,string modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return ApiResponse<UserDto>.Failure("Username is required");
                }

                if (string.IsNullOrWhiteSpace(modifiedBy))
                {
                    return ApiResponse<UserDto>.Failure("Modified by user is required");
                }

                // Check if user exists and is active
                var user = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);
                if (user == null)
                {
                    return ApiResponse<UserDto>.Failure($"User '{username}' not found");
                }

                if (user.Isactive != "1" && user.Isactive != "true")
                {
                    return ApiResponse<UserDto>.Failure("Cannot transfer inactive user");
                }

                // Update location
                var updated = await _userManagementRepository.UpdateUserLocationAsync(
                    username,
                    locationDto.Province,
                    locationDto.ProvinceId,
                    locationDto.District,
                    locationDto.DistrictId,
                    locationDto.Tehsil,
                    locationDto.TehsilId,
                    locationDto.SiteId,
                    locationDto.SiteName,
                    modifiedBy
                );

                if (!updated)
                {
                    return ApiResponse<UserDto>.Failure("Failed to update user location");
                }

                // Get updated user
                var updatedUser = await _userManagementRepository.GetUserWithLocationByUsernameAsync(username);
                var mappedUser = _mapper.Map<UserDto>(updatedUser);

                return ApiResponse<UserDto>.Success(mappedUser, "User location transferred successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.Failure($"Failed to transfer user location: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ToggleUserStatusAsync(string username, bool isActive, string modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return ApiResponse<bool>.Failure("Username is required");
                }

                if (string.IsNullOrWhiteSpace(modifiedBy))
                {
                    return ApiResponse<bool>.Failure("Modified by user is required");
                }

                // Prevent deactivating self
                if (username == modifiedBy)
                {
                    return ApiResponse<bool>.Failure("You cannot change your own status");
                }

                var updated = await _userManagementRepository.ToggleUserStatusAsync(username, isActive, modifiedBy);

                if (!updated)
                {
                    return ApiResponse<bool>.Failure($"User '{username}' not found");
                }

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
                {
                    return ApiResponse<bool>.Failure("Username is required");
                }

                if (string.IsNullOrWhiteSpace(modifiedBy))
                {
                    return ApiResponse<bool>.Failure("Modified by user is required");
                }

                // Prevent blocking self
                if (username == modifiedBy)
                {
                    return ApiResponse<bool>.Failure("You cannot block yourself");
                }

                var blocked = await _userManagementRepository.BlockUserAsync(username, modifiedBy);

                if (!blocked)
                {
                    return ApiResponse<bool>.Failure($"User '{username}' not found");
                }

                return ApiResponse<bool>.Success(true, "User blocked successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to block user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<UserDto>>> GetUsersByLocationAsync(
            string? province = null,
            string? district = null,
            string? tehsil = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var result = await _userManagementRepository.GetUsersByLocationAsync(
                    province,
                    district,
                    tehsil,
                    pageNumber,
                    pageSize
                );

                var items = result.Items;
                var totalCount = result.TotalCount;

                var mappedItems = _mapper.Map<List<UserDto>>(items);

                var paginatedResponse = new PaginatedResponse<UserDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<UserDto>>.Success(
                    paginatedResponse,
                    "Users by location retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<UserDto>>.Failure(
                    $"Failed to retrieve users by location: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<LocationStatsDto>> GetLocationStatisticsAsync()
        {
            try
            {
                var provinceStats = await _userManagementRepository.GetLocationStatisticsAsync();

                var stats = new LocationStatsDto
                {
                    ProvinceStats = provinceStats,
                    TotalUsers = provinceStats.Values.Sum(),
                    ActiveUsers = provinceStats.Values.Sum(),
                    InactiveUsers = 0
                };

                return ApiResponse<LocationStatsDto>.Success(stats, "Location statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LocationStatsDto>.Failure($"Failed to retrieve statistics: {ex.Message}");
            }
        }
    }
}