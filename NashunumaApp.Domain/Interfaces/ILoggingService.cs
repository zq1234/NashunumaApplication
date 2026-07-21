using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Domain.Interfaces
{
 

    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogVerbose(string message, params object[] args);
        void LogFatal(Exception exception, string message, params object[] args);

        // Structured logging
        void LogWithContext(string level, string message, object context, params object[] args);

        // Request/Response logging
        void LogRequest(string endpoint, object request, string userId = null);
        void LogResponse(string endpoint, object response, long durationMs, string userId = null);
    }
}
