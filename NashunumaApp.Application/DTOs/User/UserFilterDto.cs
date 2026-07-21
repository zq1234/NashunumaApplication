// DTOs/User/UserFilterDto.cs
namespace NashunumaApp.Application.DTOs.User
{
    public class UserFilterDto
    {
        public string SearchTerm { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Tehsil { get; set; }
        public string UserType { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}