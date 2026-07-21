// DTOs/Auth/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace NashunumaApp.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string PersonName { get; set; } = string.Empty;

        [Required]
        public string MobileNumber { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;
        public string UserType { get; set; } = "User";
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Tehsil { get; set; } = string.Empty;
        public decimal? SiteId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string IMEINo { get; set; } = string.Empty;
        public string MACAddress { get; set; } = string.Empty;
    }
}