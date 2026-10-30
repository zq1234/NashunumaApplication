using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
        var swaggerConfig = builder.Configuration.GetSection(LoggingConstants.SwaggerConfigSectionName);
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

        // XML Documentation (API layer)
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // XML Documentation (Application layer)
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
    var allowedOrigins = builder.Configuration
        .GetSection(LoggingConstants.CorsAllowedOriginsKey)
        .Get<string[]>()
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

    // Request logging
    app.UseSerilogRequestLogging();

    // 7.1 CORS must run before redirect / exception / auth middleware,
    //     otherwise preflight OPTIONS requests fail with
    //     "No 'Access-Control-Allow-Origin' header is present".
    if (app.Environment.IsDevelopment())
    {
        app.UseCors(LoggingConstants.CorsPolicyAllowAll);
        Log.Information(LoggingConstants.MessageCorsAllowAll);
    }
    else
    {
        app.UseCors(LoggingConstants.CorsPolicyAllowAngular);
        Log.Information(LoggingConstants.MessageCorsRestricted);
        Log.Information(LoggingConstants.MessageCorsOrigins, string.Join(", ", allowedOrigins));
    }

    // Exception handling middleware
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseHttpsRedirection();

    // Static files (for Swagger customizations)
    app.UseStaticFiles();

    // 7.2 Swagger (enabled in Development or when explicitly configured)
    var swaggerConfigSection = builder.Configuration.GetSection(LoggingConstants.SwaggerConfigSectionName);
    var swaggerEnabled =
        app.Environment.IsDevelopment() ||
        swaggerConfigSection.GetValue<bool>("EnableInProduction");

    if (swaggerEnabled)
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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapGet(LoggingConstants.HealthCheckEndpoint, () => Results.Ok(new
    {
        Status = LoggingConstants.HealthCheckStatus,
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName,
        Application = LoggingConstants.ApplicationName
    }));

    // ============================================================
    // 8. LOG BOUND ADDRESSES ONCE KESTREL STARTS
    //    (Kestrel only knows the actual ports after it binds,
    //     which happens inside app.Run().)
    // ============================================================
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

    lifetime.ApplicationStarted.Register(() =>
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses
                        ?? new List<string>();

        if (addresses.Count == 0)
        {
            Log.Warning(LoggingConstants.MessageNoBoundAddresses);
        }
        else
        {
            foreach (var address in addresses)
            {
                Log.Information(LoggingConstants.MessageListeningOn, address);
            }
        }

        Log.Information(LoggingConstants.MessageEnvironmentLabel, app.Environment.EnvironmentName);
        Log.Information(LoggingConstants.MessageJwtStatus, "Enabled");

        if (swaggerEnabled)
        {
            var swaggerBase = addresses.FirstOrDefault(a => a.StartsWith("http://"))
                              ?? addresses.FirstOrDefault();

            if (swaggerBase != null)
            {
                Log.Information(LoggingConstants.MessageSwaggerUiUrl,
                    swaggerBase.TrimEnd('/'),
                    LoggingConstants.SwaggerRoutePrefix);
            }
        }
        else
        {
            Log.Information(LoggingConstants.MessageSwaggerDisabled);
        }

        Log.Information(LoggingConstants.MessageApplicationStarted);
        Log.Information(LoggingConstants.MessageStartedAt,
            DateTime.Now.ToString(LoggingConstants.DefaultDateTime));
    });

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