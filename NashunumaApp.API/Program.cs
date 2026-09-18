using Microsoft.OpenApi.Models;
using NashunumaApp.API.Extensions;
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
using NashunumaApp.Shared.Logging;
using Serilog;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURE SERILOG 
// ============================================================
builder.AddSerilog();

try
{
    Log.Information(LoggingConstants.MessageApplicationStartup);
    Log.Information(LoggingConstants.MessageApplicationVersion, Assembly.GetExecutingAssembly().GetName().Version);

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
        c.AddSecurityDefinition(LoggingConstants.AuthScheme, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = LoggingConstants.AuthScheme,
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
                        Id = LoggingConstants.AuthSecurityId
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
    builder.Services.AddScoped<IMotherTrimisterService, MotherTrimisterService>();

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // ============================================================
    // 5. CONFIGURE CORS
    // ============================================================
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? LoggingConstants.DefaultAllowedOrigins;

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LoggingConstants.CorsPolicyAllowAngular, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Pagination", "X-Total-Count", "Content-Disposition");
        });

        options.AddPolicy(LoggingConstants.CorsPolicyAllowAll, policy =>
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
    app.UseSerilogRequestLogging();

    // Swagger Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(LoggingConstants.SwaggerEndpoint, "Nashunuma App API v1");
            c.RoutePrefix = LoggingConstants.SwaggerRoutePrefix;
            c.DocumentTitle = LoggingConstants.SwaggerDocumentTitle;
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
        app.UseCors(LoggingConstants.CorsPolicyAllowAll);
        Log.Information(LoggingConstants.MessageCorsAllowAll);
    }
    else
    {
        app.UseCors(LoggingConstants.CorsPolicyAllowAngular);
        Log.Information(LoggingConstants.MessageCorsRestricted);
    }

    // Exception Handling Middleware (with logging)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health Check Endpoint
    app.MapGet(LoggingConstants.HealthCheckEndpoint, () => Results.Ok(new
    {
        Status = LoggingConstants.HealthCheckStatus,
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName,
        Application = LoggingConstants.ApplicationName
    }));

    // ============================================================
    // 8. DATABASE INITIALIZATION AND SEEDING
    // ============================================================
    Log.Information(LoggingConstants.MessageEnvironment, app.Environment.EnvironmentName);
    Log.Information(LoggingConstants.MessageJwtEnabled);

    if (app.Environment.IsDevelopment())
    {
        Log.Information(LoggingConstants.MessageSwaggerUI);
    }

    Log.Information(LoggingConstants.MessageApplicationStarted);
    Log.Information(LoggingConstants.MessageStartedAt, DateTime.Now.ToString(LoggingConstants.DefaultDateTime));

    // ============================================================
    // 9. RUN APPLICATION
    // ============================================================
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, LoggingConstants.MessageApplicationTerminated);
    Log.Fatal(LoggingConstants.ErrorDetails, ex.GetType().Name, ex.Message);
    Log.Fatal(LoggingConstants.ErrorStackTrace, ex.StackTrace);
    throw;
}
finally
{
    Log.Information(LoggingConstants.MessageApplicationShutdown);
    Log.CloseAndFlush();
}