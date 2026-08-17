using NashunumaApp.Shared.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Enrichers;

namespace NashunumaApp.API.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProcessId()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithProperty(LoggingConstants.PropertyApplication, LoggingConstants.ApplicationName)
                    .Enrich.WithProperty(LoggingConstants.PropertyEnvironment, context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("CorrelationId", Guid.NewGuid().ToString());

                // Filter out noise to reduce log clutter - CORRECT SYNTAX
                // Using separate filter calls instead of chaining
                configuration.Filter.ByExcluding(Matching.FromSource(LoggingConstants.DbCommandLogging));
                configuration.Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Diagnostics.HealthChecks"));
                configuration.Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting.Diagnostics"));

                // Filter out static file requests
                configuration.Filter.ByExcluding(logEvent =>
                {
                    if (logEvent.Properties.TryGetValue(LoggingConstants.PropertyRequestPath, out var pathValue))
                    {
                        var path = pathValue.ToString();
                        return path.Contains("/swagger") ||
                               path.Contains(LoggingConstants.HealthCheckEndpoint) ||
                               path.Contains("/favicon.ico") ||
                               path.Contains(".css") ||
                               path.Contains(".js") ||
                               path.Contains(".html") ||
                               path.Contains(".png") ||
                               path.Contains(".jpg") ||
                               path.Contains(".woff") ||
                               path.Contains(".ttf");
                    }
                    return false;
                });

                // Console sink with colored output (only in development)
                if (context.HostingEnvironment.IsDevelopment())
                {
                    configuration.WriteTo.Console(
                        outputTemplate: LoggingConstants.ConsoleOutputTemplate
                    );
                }

                // File sink - rolling by day
                configuration.WriteTo.File(
                    path: $"{LoggingConstants.LogDirectory}/{LoggingConstants.LogFileName}",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: LoggingConstants.FileOutputTemplate,
                    retainedFileCountLimit: LoggingConstants.RetainedFileCountLimit,
                    fileSizeLimitBytes: LoggingConstants.MaxFileSizeBytes,
                    rollOnFileSizeLimit: true,
                    shared: true
                );

                // Error file sink - only errors
                configuration.WriteTo.File(
                    path: $"{LoggingConstants.LogDirectory}/{LoggingConstants.ErrorLogFileName}",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: LoggingConstants.FileOutputTemplate,
                    retainedFileCountLimit: LoggingConstants.ErrorRetainedFileCountLimit,
                    fileSizeLimitBytes: LoggingConstants.MaxFileSizeBytes,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
                );

                // JSON format file sink for structured logging
                configuration.WriteTo.File(
                    path: $"{LoggingConstants.LogDirectory}/{LoggingConstants.JsonLogFileName}",
                    rollingInterval: RollingInterval.Day,
                    formatter: new Serilog.Formatting.Json.JsonFormatter(),
                    retainedFileCountLimit: LoggingConstants.JsonRetainedFileCountLimit,
                    fileSizeLimitBytes: LoggingConstants.MaxFileSizeBytes,
                    rollOnFileSizeLimit: true,
                    shared: true
                );

#if DEBUG
                Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"Serilog SelfLog: {msg}"));
#endif
            });

            return builder;
        }

        public static WebApplication UseSerilogRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = LoggingConstants.RequestLoggingTemplate;

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    var userId = httpContext.User?.FindFirst(LoggingConstants.ClaimSub)?.Value ??
                                 httpContext.User?.FindFirst(LoggingConstants.ClaimSiteId)?.Value ??
                                 httpContext.User?.Identity?.Name ??
                                 LoggingConstants.DefaultAnonymousUser;

                    diagnosticContext.Set(LoggingConstants.PropertyUserId, userId);
                    diagnosticContext.Set(LoggingConstants.PropertyRemoteIP,
                        httpContext.Connection.RemoteIpAddress?.ToString());
                    diagnosticContext.Set(LoggingConstants.PropertyCorrelationId,
                        httpContext.TraceIdentifier);
                    diagnosticContext.Set(LoggingConstants.PropertyUserAgent,
                        httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                    diagnosticContext.Set(LoggingConstants.PropertyRequestPath,
                        httpContext.Request.Path);
                    diagnosticContext.Set(LoggingConstants.PropertyRequestMethod,
                        httpContext.Request.Method);
                    diagnosticContext.Set(LoggingConstants.PropertyRequestHost,
                        httpContext.Request.Host.ToString());
                    diagnosticContext.Set(LoggingConstants.PropertyIsAuthenticated,
                        httpContext.User?.Identity?.IsAuthenticated ?? false);
                };

                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null || httpContext.Response.StatusCode >= 500)
                        return Serilog.Events.LogEventLevel.Error;
                    if (httpContext.Response.StatusCode >= 400)
                        return Serilog.Events.LogEventLevel.Warning;
                    return Serilog.Events.LogEventLevel.Information;
                };
            });

            return app;
        }
    }
}