// Infrastructure/Persistence/Interceptors/AuditInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NashunumaApp.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ILogger<AuditInterceptor> _logger;

        public AuditInterceptor(ILogger<AuditInterceptor> logger)
        {
            _logger = logger;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                await AuditChangesAsync(eventData.Context);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task AuditChangesAsync(DbContext context)
        {
            var entries = context.ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .ToList();

            if (!entries.Any()) return;

            var auditData = new List<object>();

            foreach (var entry in entries)
            {
                var auditEntry = new
                {
                    EntityType = entry.Entity.GetType().Name,
                    State = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Properties = entry.Properties
                        .Where(p => p.IsModified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                        .Select(p => new
                        {
                            Name = p.Metadata.Name,
                            OriginalValue = entry.State == EntityState.Deleted ? p.OriginalValue : null,
                            CurrentValue = entry.State == EntityState.Added || entry.State == EntityState.Modified ? p.CurrentValue : null,
                            IsModified = p.IsModified
                        })
                };

                auditData.Add(auditEntry);
            }

            _logger.LogInformation(" Entity changes detected: {Count} changes", auditData.Count);

            // Also log with Serilog directly
            Serilog.Log.Information(" Entity changes detected: {@AuditData}", auditData);
        }
    }
}