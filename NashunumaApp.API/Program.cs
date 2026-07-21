using Microsoft.OpenApi.Models;
using NashunumaApp.API.Middleware;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Application.Mappings;
using NashunumaApp.Application.Services;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Extensions;
using NashunumaApp.Infrastructure.Logging;
using NashunumaApp.Infrastructure.Persistence.Interceptors;
using NashunumaApp.Infrastructure.Repositories;
using NashunumaApp.Infrastructure.Services;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURE SERILOG (MUST BE FIRST)
// ============================================================
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
        .Enrich.WithProperty("Application", "NashunumaApp")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    // Console sink with colored output
    configuration.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    );

    // File sink - rolling by day
    configuration.WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 31,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        rollOnFileSizeLimit: true,
        shared: true
    );

    // Error file sink - only errors
    configuration.WriteTo.File(
        path: "Logs/error-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 31,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        shared: true,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
    );
});

try
{
    Log.Information(" Starting NashunumaApp API...");

    // ============================================================
    // 2. CONFIGURE CONTROLLERS
    // ============================================================
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // ============================================================
    // 3. CONFIGURE SWAGGER
    // ============================================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        var swaggerConfig = builder.Configuration.GetSection("Swagger");
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = swaggerConfig.GetValue<string>("Title") ?? "Nashunuma App API",
            Version = swaggerConfig.GetValue<string>("Version") ?? "v1",
            Description = swaggerConfig.GetValue<string>("Description") ?? "Nashunuma App - Food Stock Management System API",
            Contact = new OpenApiContact
            {
                Name = "Nashunuma Support",
                Email = "support@nashunuma.com",
                Url = new Uri("https://www.nashunuma.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // JWT Authentication for Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your Bearer token. Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // XML Documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Include XML from Application layer
        var appXmlFile = "NashunumaApp.Application.xml";
        var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
        if (File.Exists(appXmlPath))
        {
            c.IncludeXmlComments(appXmlPath);
        }
    });

    // ============================================================
    // 4. REGISTER SERVICES
    // ============================================================

    // Infrastructure Services (Database, Identity, Repositories)
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddScoped<ILoggingService, SerilogLogger>();
    builder.Services.AddScoped<DbLoggingInterceptor>();
    builder.Services.AddScoped<AuditInterceptor>();
    // Application Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IFoodStockService, FoodStockService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<ILocationRepository, LocationRepository>();
    builder.Services.AddScoped<ILocationService, LocationService>();
    // AutoMapper
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // ============================================================
    // 5. CONFIGURE CORS
    // ============================================================
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:4200", "https://localhost:4200" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Pagination", "X-Total-Count", "Content-Disposition");
        });

        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // ============================================================
    // 6. BUILD APPLICATION
    // ============================================================
    var app = builder.Build();

    // ============================================================
    // 7. CONFIGURE PIPELINE
    // ============================================================

    // Use Serilog Request Logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms | " +
            "User: {UserId} | IP: {RemoteIP} | Correlation: {CorrelationId}";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User?.FindFirst("sub")?.Value ??
                         httpContext.User?.Identity?.Name ??
                         "Anonymous";

            diagnosticContext.Set("UserId", userId);
            diagnosticContext.Set("RemoteIP",
                httpContext.Connection.RemoteIpAddress?.ToString());
            diagnosticContext.Set("CorrelationId",
                httpContext.TraceIdentifier);
            diagnosticContext.Set("UserAgent",
                httpContext.Request.Headers["User-Agent"].FirstOrDefault());
            diagnosticContext.Set("RequestPath",
                httpContext.Request.Path);
            diagnosticContext.Set("RequestMethod",
                httpContext.Request.Method);
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

    // Swagger Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nashunuma App API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "Nashunuma App API Documentation";
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
            c.DefaultModelsExpandDepth(2);
            c.DisplayOperationId();
            c.EnableDeepLinking();
            c.ShowExtensions();
        });
    }

    app.UseHttpsRedirection();

    // Static Files (for Swagger customizations)
    app.UseStaticFiles();

    // Use CORS based on environment
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("AllowAll");
        Log.Information(" Development Mode: CORS AllowAll enabled");
    }
    else
    {
        app.UseCors("AllowAngular");
        Log.Information("Production Mode: CORS restricted to allowed origins");
    }

    // Exception Handling Middleware (with logging)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health Check Endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    }));

    // ============================================================
    // 8. DATABASE INITIALIZATION AND SEEDING
    // ============================================================
    Log.Information($" Environment: {app.Environment.EnvironmentName}");
    Log.Information($" Swagger UI: https://localhost:7058/swagger");

    // ============================================================
    // 9. RUN APPLICATION
    // ============================================================
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, " Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}