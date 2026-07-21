// DTOs/Auth/LoginResponseDto.cs
namespace NashunumaApp.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public decimal? SiteId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Tehsil { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}