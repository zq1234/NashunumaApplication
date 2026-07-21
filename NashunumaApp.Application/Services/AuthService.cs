// Application/Services/AuthService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NashunumaApp.Application.DTOs.Auth;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<NthUser> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<NthSiteLocation> _siteLocationRepository;

        public AuthService(
            IGenericRepository<NthUser> userRepository,
            IConfiguration configuration,
            IGenericRepository<NthSiteLocation> siteLocationRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _siteLocationRepository = siteLocationRepository;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by username or email
                var users = await _userRepository.FindAsync(u =>
                    u.Username == loginDto.Email || u.Email == loginDto.Email);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    return ApiResponse<LoginResponseDto>.Failure("Invalid username or password");
                }

                // Check if user is active
                if (user.Isactive != "1" && user.Isactive != "true")
                {
                    return ApiResponse<LoginResponseDto>.Failure("User account is inactive. Please contact administrator.");
                }
                if(loginDto.Password!= user.Password)
                {
                    // Verify password
                    if (!VerifyPassword(loginDto.Password, user.Password))
                    {
                        return ApiResponse<LoginResponseDto>.Failure("Invalid username or password");
                    }
                }
                

                // Update last login datetime
                user.Lastlogindatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //await _userRepository.UpdateAsync(user);

                // Get site location information if available
                NthSiteLocation siteLocation = null;
                if (user.SiteId.HasValue)
                {
                    var siteLocations = await _siteLocationRepository.FindAsync(s => s.Id == user.SiteId.Value);
                    siteLocation = siteLocations.FirstOrDefault();
                }

                var token = GenerateJwtToken(user, siteLocation);

                var response = new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    UserId = user.Userid?.ToString() ?? string.Empty,
                    Email = user.Email,
                    PersonName = user.Personname,
                    Designation = user.Designation,
                    UserType = user.Usertype,
                    SiteId = user.SiteId,
                    SiteName = user.SiteName,
                    Province = user.Province,
                    District = user.District,
                    Tehsil = user.Tehsil,
                    MobileNumber = user.Mobilenumber,
                    IsAdmin = user.Isadmin == "1" || user.Isadmin == "true"
                };

                return ApiResponse<LoginResponseDto>.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.Failure($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                var existingUsers = await _userRepository.FindAsync(u => u.Username == registerDto.Email);
                if (existingUsers.Any())
                {
                    return ApiResponse<string>.Failure("Username already exists");
                }

                // Check if email already exists
                var existingEmails = await _userRepository.FindAsync(u => u.Email == registerDto.Email);
                if (existingEmails.Any())
                {
                    return ApiResponse<string>.Failure("Email already exists");
                }

                // Create new user
                var user = new NthUser
                {
                    Username = registerDto.Email,
                    Password = HashPassword(registerDto.Password),
                    Email = registerDto.Email,
                    Personname = registerDto.PersonName,
                    Mobilenumber = registerDto.MobileNumber,
                    Designation = registerDto.Designation,
                    Usertype = registerDto.UserType ?? "User",
                    Province = registerDto.Province,
                    District = registerDto.District,
                    Tehsil = registerDto.Tehsil,
                    SiteId = registerDto.SiteId,
                    SiteName = registerDto.SiteName,
                    Imeino = registerDto.IMEINo,
                    Macaddress = registerDto.MACAddress,
                    Isactive = "1", // Active by default
                    Isadmin = "0", // Not admin by default
                    Requestdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Activedatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Activedby = "System"
                };

                await _userRepository.AddAsync(user);

                return ApiResponse<string>.Success(user.Username, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto, string username)
        {
            try
            {
                var users = await _userRepository.FindAsync(u => u.Username == username);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    return ApiResponse<string>.Failure("User not found");
                }

                // Verify current password
                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
                {
                    return ApiResponse<string>.Failure("Current password is incorrect");
                }

                // Update password
                user.Password = HashPassword(changePasswordDto.NewPassword);
               // user.Updatedby = username;
                //user.Updatedon = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _userRepository.UpdateAsync(user);

                return ApiResponse<string>.Success("Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure($"Password change failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var users = await _userRepository.FindAsync(u =>
                    u.Username == resetPasswordDto.Email && u.Email == resetPasswordDto.Email);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    return ApiResponse<string>.Failure("User not found with the provided credentials");
                }

                // Reset password
                user.Password = HashPassword(resetPasswordDto.NewPassword);
                //user.Updatedby = "System";
                //user.Updatedon = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _userRepository.UpdateAsync(user);

                return ApiResponse<string>.Success("Password reset successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure($"Password reset failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> LogoutAsync(string userId)
        {
            try
            {
                // For JWT, logout is handled client-side by removing the token
                // We can optionally log the logout activity
                return ApiResponse<string>.Success("Logged out successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure($"Logout failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string username)
        {
            try
            {
                var users = await _userRepository.FindAsync(u => u.Username == username);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    return ApiResponse<UserProfileDto>.Failure("User not found");
                }

                var profile = new UserProfileDto
                {
                    Username = user.Username,
                    PersonName = user.Personname,
                    Email = user.Email,
                    MobileNumber = user.Mobilenumber,
                    Designation = user.Designation,
                    UserType = user.Usertype,
                    Province = user.Province,
                    District = user.District,
                    Tehsil = user.Tehsil,
                    SiteId = user.SiteId,
                    SiteName = user.SiteName,
                    IsActive = user.Isactive == "1" || user.Isactive == "true",
                    IsAdmin = user.Isadmin == "1" || user.Isadmin == "true"
                };

                return ApiResponse<UserProfileDto>.Success(profile, "Profile retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserProfileDto>.Failure($"Failed to get profile: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return ApiResponse<bool>.Success(true, "Token is valid");
            }
            catch
            {
                return ApiResponse<bool>.Failure("Invalid token");
            }
        }

        private string GenerateJwtToken(NthUser user, NthSiteLocation siteLocation)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid?.ToString() ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("username", user.Username),
                new Claim("email", user.Email ?? string.Empty),
                new Claim("personname", user.Personname ?? string.Empty),
                new Claim("designation", user.Designation ?? string.Empty),
                new Claim("usertype", user.Usertype ?? string.Empty),
                new Claim("province", user.Province ?? string.Empty),
                new Claim("district", user.District ?? string.Empty),
                new Claim("tehsil", user.Tehsil ?? string.Empty),
                new Claim("mobilenumber", user.Mobilenumber ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.SiteId.HasValue)
            {
                claims.Add(new Claim("siteid", user.SiteId.Value.ToString()));
                claims.Add(new Claim("sitename", user.SiteName ?? string.Empty));
            }

            if (siteLocation != null)
            {
                claims.Add(new Claim("siteaddress", siteLocation.Address ?? string.Empty));
                claims.Add(new Claim("sitecontact", siteLocation.Contact ?? string.Empty));
            }

            // Add admin claim
            if (user.Isadmin == "1" || user.Isadmin == "true")
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("isadmin", "true"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                claims.Add(new Claim("isadmin", "false"));
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            
            return HashPassword(inputPassword) == storedPassword;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}