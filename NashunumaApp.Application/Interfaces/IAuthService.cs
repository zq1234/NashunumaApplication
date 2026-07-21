using NashunumaApp.Application.DTOs.Auth;
using NashunumaApp.Application.DTOs.Common;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface IAuthService
    {

        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto, string username);
        Task<ApiResponse<bool>> ValidateTokenAsync(string token);
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string username);
        Task<ApiResponse<string>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ApiResponse<string>> LogoutAsync(string userId);
    }
}