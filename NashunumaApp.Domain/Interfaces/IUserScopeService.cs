using NashunumaApp.Domain.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IUserScopeService
    {
        Task<UserScope?> ResolveAsync(string username, CancellationToken ct = default);
        Task<bool> IsAdminAsync(string username, CancellationToken ct = default);
        Task<HashSet<string>> GetAllowedSiteIdsAsync(string username, CancellationToken ct = default);
        bool CanSeeRow(UserScope scope, string? enteredBy);
    }

    

  
}