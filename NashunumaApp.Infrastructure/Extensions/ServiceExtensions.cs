// Infrastructure/Extensions/ServiceExtensions.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using NashunumaApp.Infrastructure.Identity;
using NashunumaApp.Infrastructure.Repositories;
using NashunumaApp.Infrastructure.Persistence.Interceptors;
using Serilog;
using System;
using System.Text;

namespace NashunumaApp.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Log.Information(" Registering Infrastructure Services...");

            // ============================================================
            // 1. REGISTER INTERCEPTORS FIRST
            // ============================================================
            services.AddScoped<DbLoggingInterceptor>();
            services.AddScoped<AuditInterceptor>();

            Log.Information(" Interceptors registered");

            // ============================================================
            // 2. ADD DATABASE CONTEXT WITH ORACLE
            // ============================================================
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string not configured");

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseOracle(connectionString, oracleOptions =>
                {
                    oracleOptions.CommandTimeout(60);
                });

                // Add interceptors from service provider
                var dbLoggingInterceptor = serviceProvider.GetService<DbLoggingInterceptor>();
                var auditInterceptor = serviceProvider.GetService<AuditInterceptor>();

                if (dbLoggingInterceptor != null)
                {
                    options.AddInterceptors(dbLoggingInterceptor);
                }

                if (auditInterceptor != null)
                {
                    options.AddInterceptors(auditInterceptor);
                }
            });

            Log.Information(" Database Context registered with Oracle");

            // ============================================================
            // 3. ADD IDENTITY WITH ROLES
            // ============================================================
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            Log.Information(" Identity Services registered");

            // ============================================================
            // 4. ADD JWT AUTHENTICATION
            // ============================================================
            var jwtSecret = configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured in appsettings.json");

            var validIssuer = configuration["JWT:ValidIssuer"]
                ?? throw new InvalidOperationException("JWT ValidIssuer not configured in appsettings.json");

            var validAudience = configuration["JWT:ValidAudience"]
                ?? throw new InvalidOperationException("JWT ValidAudience not configured in appsettings.json");

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = validIssuer,
                    ValidateAudience = true,
                    ValidAudience = validAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.Name
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                            Log.Warning(" Token expired for request: {Path}",
                                context.HttpContext.Request.Path);
                        }
                        else
                        {
                            Log.Error(context.Exception,
                                " Authentication failed for request: {Path}",
                                context.HttpContext.Request.Path);
                        }
                        return System.Threading.Tasks.Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            isSuccess = false,
                            message = "You are not authorized. Please provide a valid token.",
                            statusCode = 401
                        });
                        Log.Warning(" Unauthorized access attempt to: {Path}",
                            context.HttpContext.Request.Path);
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            isSuccess = false,
                            message = "You don't have permission to access this resource.",
                            statusCode = 403
                        });
                        Log.Warning(" Forbidden access attempt to: {Path} by User: {User}",
                            context.HttpContext.Request.Path,
                            context.HttpContext.User?.Identity?.Name ?? "Unknown");
                        return context.Response.WriteAsync(result);
                    },
                    OnTokenValidated = context =>
                    {
                        var userId = context.Principal?.FindFirst("sub")?.Value
                            ?? context.Principal?.Identity?.Name;
                        Log.Debug(" Token validated for User: {UserId}", userId ?? "Unknown");
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                };
            });

            Log.Information(" JWT Authentication registered");

            // ============================================================
            // 5. REGISTER REPOSITORIES
            // ============================================================
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IFoodStockRepository, FoodStockRepository>();
            services.AddScoped<IUserManagementRepository, UserManagementRepository>();

            Log.Information(" Repositories registered");

            // ============================================================
            // 6. ADD AUTHORIZATION POLICIES
            // ============================================================
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("UserOrAdmin", policy =>
                    policy.RequireRole("User", "Admin"));
            });

            Log.Information(" Authorization policies registered");
            Log.Information(" All Infrastructure Services registered successfully");

            return services;
        }
    }
}