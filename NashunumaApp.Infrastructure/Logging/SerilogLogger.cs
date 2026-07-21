using System;
using System.Collections.Generic;
using System.Text;
using NashunumaApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;

using SerilogILogger = Serilog.ILogger;

namespace NashunumaApp.Infrastructure.Logging
{
    public class SerilogLogger : ILoggingService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<SerilogLogger> _logger;
        private readonly SerilogILogger _serilogLogger;

        public SerilogLogger(Microsoft.Extensions.Logging.ILogger<SerilogLogger> logger)
        {
            _logger = logger;
            _serilogLogger = Log.Logger;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
            _serilogLogger.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
            _serilogLogger.Warning(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
            _serilogLogger.Error(exception, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
            _serilogLogger.Error(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
            _serilogLogger.Debug(message, args);
        }

        public void LogVerbose(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
            _serilogLogger.Verbose(message, args);
        }

        public void LogFatal(Exception exception, string message, params object[] args)
        {
            _logger.LogCritical(exception, message, args);
            _serilogLogger.Fatal(exception, message, args);
        }

        public void LogWithContext(string level, string message, object context, params object[] args)
        {
            var contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            switch (level.ToLower())
            {
                case "information":
                    _serilogLogger.ForContext("Context", contextJson)
                        .Information(message, args);
                    break;
                case "warning":
                    _serilogLogger.ForContext("Context", contextJson)
                        .Warning(message, args);
                    break;
                case "error":
                    _serilogLogger.ForContext("Context", contextJson)
                        .Error(message, args);
                    break;
                case "debug":
                    _serilogLogger.ForContext("Context", contextJson)
                        .Debug(message, args);
                    break;
                default:
                    _serilogLogger.ForContext("Context", contextJson)
                        .Information(message, args);
                    break;
            }
        }

        public void LogRequest(string endpoint, object request, string userId = null)
        {
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _serilogLogger
                .ForContext("Endpoint", endpoint)
                .ForContext("UserId", userId ?? "Anonymous")
                .ForContext("RequestData", requestJson)
                .Information("API Request: {Endpoint} from User {UserId}", endpoint, userId ?? "Anonymous");
        }

        public void LogResponse(string endpoint, object response, long durationMs, string userId = null)
        {
            var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _serilogLogger
                .ForContext("Endpoint", endpoint)
                .ForContext("UserId", userId ?? "Anonymous")
                .ForContext("ResponseData", responseJson)
                .ForContext("DurationMs", durationMs)
                .Information("API Response: {Endpoint} completed in {DurationMs}ms", endpoint, durationMs);
        }
    }
}