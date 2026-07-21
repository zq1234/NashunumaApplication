using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NashunumaApp.Application.DTOs.Common;
using Serilog;
using System.Net;
using System.Text.Json;

namespace NashunumaApp.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log with Serilog directly
                Log.Error(ex,
                    "💥 Unhandled exception: {Message} | Request: {Method} {Path} | User: {User} | IP: {IP}",
                    ex.Message,
                    context.Request.Method,
                    context.Request.Path,
                    context.User?.Identity?.Name ?? "Anonymous",
                    context.Connection.RemoteIpAddress?.ToString());

                // Also log with ILogger for compatibility
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An error occurred while processing your request",
                Errors = new List<string>()
            };

            // Log the specific exception type for better tracking
            Log.ForContext("ExceptionType", exception.GetType().Name)
               .ForContext("RequestPath", context.Request.Path)
               .ForContext("RequestMethod", context.Request.Method)
               .Warning("Handling exception of type {ExceptionType}: {Message}",
                    exception.GetType().Name,
                    exception.Message);

            switch (exception)
            {
                case DbUpdateException dbEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = "Database operation failed";
                    response.Errors.Add(dbEx.InnerException?.Message ?? dbEx.Message);
                    Log.Error(dbEx, " Database update failed");
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "You are not authorized to access this resource";
                    response.Errors.Add("Authentication required");
                    Log.Warning("Unauthorized access attempt to: {Path}", context.Request.Path);
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid request parameters";
                    response.Errors.Add(argEx.Message);
                    Log.Warning(" Invalid argument: {Message}", argEx.Message);
                    break;

                case InvalidOperationException invOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid operation";
                    response.Errors.Add(invOpEx.Message);
                    Log.Warning(" Invalid operation: {Message}", invOpEx.Message);
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "The requested resource was not found";
                    response.Errors.Add("Resource not found");
                    Log.Warning(" Resource not found: {Path}", context.Request.Path);
                    break;

                //case ArgumentNullException nullEx:
                //    response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    response.Message = "Required data is missing";
                //    response.Errors.Add(nullEx.Message);
                //    Log.Warning(" Required data missing: {Message}", nullEx.Message);
                //    break;

                

                default:
                    response.Message = "An unexpected error occurred. Please try again later.";
                    response.Errors.Add(exception.Message);
                    Log.Error(exception, " Unhandled exception: {Message}", exception.Message);
                    break;
            }

            // Add correlation ID for tracking
            var correlationId = context.TraceIdentifier;
            response.Errors?.Add($"Correlation ID: {correlationId}");

            context.Response.StatusCode = response.StatusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            // Log response details
            Log.Debug("Exception response: StatusCode={StatusCode}, Message={Message}, CorrelationId={CorrelationId}",
                response.StatusCode,
                response.Message,
                correlationId);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}