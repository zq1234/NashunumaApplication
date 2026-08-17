using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Shared.Logging
{
    public static class LoggingConstants
    {
        // Application Information
        public const string ApplicationName = "NashunumaApp";
        public const string ApplicationVersion = "1.0.0";
        public const string DefaultEnvironment = "Production";

        // Log File Paths
        public const string LogDirectory = "Logs";
        public const string LogFileName = "log-.txt";
        public const string ErrorLogFileName = "error-.txt";
        public const string JsonLogFileName = "log-.json";

        // Log Retention Settings
        public const int RetainedFileCountLimit = 31; // 31 days
        public const int ErrorRetainedFileCountLimit = 31;
        public const int JsonRetainedFileCountLimit = 7; // 7 days
        public const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        // Output Templates
        public const string ConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
        public const string FileOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        public const string RequestLoggingTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms | User: {UserId} | IP: {RemoteIP} | Correlation: {CorrelationId}";

        // Log Levels
        public const string DefaultLogLevel = "Information";
        public const string ErrorLogLevel = "Error";
        public const string WarningLogLevel = "Warning";

        // Property Names
        public const string PropertyApplication = "Application";
        public const string PropertyEnvironment = "Environment";
        public const string PropertyUserId = "UserId";
        public const string PropertyRemoteIP = "RemoteIP";
        public const string PropertyCorrelationId = "CorrelationId";
        public const string PropertyUserAgent = "UserAgent";
        public const string PropertyRequestPath = "RequestPath";
        public const string PropertyRequestMethod = "RequestMethod";
        public const string PropertyRequestHost = "RequestHost";
        public const string PropertyIsAuthenticated = "IsAuthenticated";

        // Source Filters
        public static readonly List<string> FilteredSources = new List<string>
        {
            "Microsoft.AspNetCore.Diagnostics.HealthChecks",
            "Microsoft.AspNetCore.Hosting.Diagnostics",
            "Microsoft.EntityFrameworkCore.Database.Command"
        };

        // Static File Extensions to Filter
        public static readonly List<string> StaticFileExtensions = new List<string>
        {
            ".css", ".js", ".html", ".png", ".jpg", ".jpeg", ".gif", ".svg",
            ".woff", ".woff2", ".ttf", ".eot", ".ico", ".json", ".xml", ".map"
        };

        // Static File Paths to Filter
        public static readonly List<string> FilteredPaths = new List<string>
        {
            "/swagger", "/health", "/favicon.ico"
        };

        // Health Check
        public const string HealthCheckEndpoint = "/health";
        public const string HealthCheckStatus = "Healthy";

        // Swagger
        public const string SwaggerEndpoint = "/swagger/v1/swagger.json";
        public const string SwaggerRoutePrefix = "swagger";
        public const string SwaggerDocumentTitle = "Nashunuma App API Documentation";
        public const string SwaggerApiVersion = "v1";

        // Authentication
        public const string AuthScheme = "Bearer";
        public const string AuthSecurityId = "Bearer";

        // CORS
        public const string CorsPolicyAllowAngular = "AllowAngular";
        public const string CorsPolicyAllowAll = "AllowAll";
        public static readonly string[] DefaultAllowedOrigins = new[] { "http://localhost:4200", "https://localhost:4200" };

        // Message Templates
        public const string MessageApplicationStartup = "Starting NashunumaApp API...";
        public const string MessageApplicationVersion = "Application Version: {Version}";
        public const string MessageApplicationStarted = "Application started successfully";
        public const string MessageApplicationShutdown = "Application shutting down...";
        public const string MessageApplicationTerminated = "Application terminated unexpectedly";
        public const string MessageEnvironment = "Environment: {Environment}";
        public const string MessageSwaggerUI = "Swagger UI available at: https://localhost:7058/swagger";
        public const string MessageJwtEnabled = "JWT Authentication: Enabled";
        public const string MessageStartedAt = "Started at: {Timestamp}";
        public const string MessageCorsAllowAll = "Development Mode: CORS AllowAll enabled";
        public const string MessageCorsRestricted = "Production Mode: CORS restricted to allowed origins";

        // Error Messages
        public const string ErrorDetails = "Error Details: {ErrorType} - {ErrorMessage}";
        public const string ErrorStackTrace = "Stack Trace: {StackTrace}";

        // Claim Types
        public const string ClaimSub = "sub";
        public const string ClaimSiteId = "siteid";
        public const string ClaimName = "name";

        // Default Values
        public const string DefaultAnonymousUser = "Anonymous";
        public const string DefaultUnknown = "Unknown";
        public const string DefaultNone = "None";
        public const string DefaultDate = "dd-MM-yyyy";
        public const string DefaultDateTime = "yyyy-MM-dd HH:mm:ss";
        public const string DefaultDateFormat = "dd-MM-yyyy HH:mm:ss";

        // Response Messages
        public const string ResponseSuccess = "Success";
        public const string ResponseFailure = "Failure";
        public const string ResponseNotFound = "Not Found";
        public const string ResponseBadRequest = "Bad Request";
        public const string ResponseUnauthorized = "Unauthorized";
        public const string ResponseForbidden = "Forbidden";

        // API Routes
        public const string ApiPrefix = "api";
        public const string ApiVersion = "v1";
        public const string ApiRoutePattern = "api/[controller]";

        // Environment Constants
        public const string DevelopmentEnvironment = "Development";
        public const string ProductionEnvironment = "Production";
        public const string StagingEnvironment = "Staging";

        // Log Context Properties
        public const string ContextSource = "SourceContext";
        public const string ContextSourceContext = "SourceContext";
        public const string ContextConnectionId = "ConnectionId";
        public const string ContextRequestId = "RequestId";
        public const string ContextTraceIdentifier = "TraceIdentifier";

        // Serilog Enrichers
        public const string EnricherEnvironmentName = "WithEnvironmentName";
        public const string EnricherMachineName = "WithMachineName";
        public const string EnricherThreadId = "WithThreadId";
        public const string EnricherProcessId = "WithProcessId";
        public const string EnricherExceptionDetails = "WithExceptionDetails";
        public const string EnricherCorrelationId = "WithCorrelationId";
        public const string EnricherFromLogContext = "FromLogContext";

        // Database Logging
        public const string DbCommandLogging = "Microsoft.EntityFrameworkCore.Database.Command";
        public const string DbConnectionLogging = "Microsoft.EntityFrameworkCore.Database.Connection";
        public const string DbTransactionLogging = "Microsoft.EntityFrameworkCore.Database.Transaction";

        // Performance
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int DefaultPageNumber = 1;

        // Date Formats
        public const string DateFormatDayMonthYear = "dd-MM-yyyy";
        public const string DateFormatMonth = "dd-MMM-yyyy";

        // Validation Messages
        public const string ValidationSiteIdRequired = "Site ID is required";
        public const string ValidationOpeningStockRequired = "Opening stock boxes WAWA is required";
        public const string ValidationDateRequired = "Date is required";
        public const string ValidationInvalidDateFormat = "Invalid date format. Please use dd-MM-yyyy format.";
        public const string ValidationStockExists = "Stock information already exists for {date}! You cannot add multiple entries for the same day.";
        public const string ValidationInvalidSequence = "Cannot save stock for {date}. Please enter dates in chronological order. Last existing record is for {lastDate}.";
        public const string ValidationMissingDates = "Cannot save stock for {date}. Please first enter stock for the following missing date(s): {missingDates}";
        public const string ValidationDateTooOld = "Cannot save stock for {date}. The date is too far in the past. Please start from {startDate} or later.";
        public const string ValidationNoRecords = "No existing stock records found. Please start entering from the earliest missing date.";
        public const string ValidationUnauthorizedSite = "You don't have permission to save stock for this site";
        public const string ValidationUserNotAssigned = "User not assigned to any site";

        // Success Messages
        public const string SuccessStockSaved = "Stock Information saved successfully!";
        public const string SuccessStockDeleted = "Food stock deleted successfully";
        public const string SuccessStockRetrieved = "Food stock retrieved successfully";
        public const string SuccessStocksRetrieved = "Food stocks with site details retrieved successfully";
        public const string SuccessSummaryRetrieved = "Summary retrieved successfully";
        public const string SuccessExportGenerated = "Export generated successfully";
        public const string SuccessMissingDatesRetrieved = "Missing stock dates retrieved successfully";
        public const string SuccessAllUpToDate = "All stock entries are up to date.";

        // Error Messages
        public const string ErrorStockNotFound = "Food stock with ID {id} not found";
        public const string ErrorInvalidId = "Invalid ID provided";
        public const string ErrorInvalidIdForDeletion = "Invalid ID provided for deletion";
        public const string ErrorStockNotFoundForDeletion = "Food stock with ID {id} not found for deletion";
        public const string ErrorSavingStock = "Failed to save stock information";
        public const string ErrorRetrievingStocks = "Failed to retrieve food stocks";
        public const string ErrorRetrievingStock = "Failed to retrieve food stock";
        public const string ErrorDeletingStock = "Failed to delete food stock";
        public const string ErrorRetrievingSummary = "Failed to retrieve summary";
        public const string ErrorExportingStocks = "Failed to export food stocks";
        public const string ErrorRetrievingMissingDates = "Failed to retrieve missing stock dates";
        public const string ErrorValidatingStock = "Failed to validate stock existence";
        public const string ErrorRetrievingNextDate = "Failed to get next required date";
        public const string ErrorSavingStockGeneral = "Failed to save stock information: {message}";

        // Notification Messages
        public const string NotificationPendingEntries = "You have pending stock entries for {count} date(s). Please complete the missing stock records.";
        public const string NotificationMissingDates = "Please enter stock for {count} missing date(s). Next required date: {nextDate}";
        public const string NotificationAllUpToDate = "All stock entries are up to date.";

        // Log Event Properties
        public const string LogEventPageNumber = "PageNumber";
        public const string LogEventPageSize = "PageSize";
        public const string LogEventSearchTerm = "SearchTerm";
        public const string LogEventSiteId = "SiteId";
        public const string LogEventEnteredOn = "EnteredOn";
        public const string LogEventEnteredBy = "EnteredBy";
        public const string LogEventStockId = "StockId";
        public const string LogEventUserId = "UserId";
        public const string LogEventUsername = "Username";
        public const string LogEventDateTime = "DateTime";
        public const string LogEventDuration = "Duration";
        public const string LogEventStatusCode = "StatusCode";
        public const string LogEventError = "Error";
        public const string LogEventException = "Exception";
        public const string LogEventData = "Data";
        public const string LogEventCount = "Count";
        public const string LogEventTotalCount = "TotalCount";
        public const string LogEventFormat = "Format";
        public const string LogEventMissingCount = "MissingCount";
        public const string LogEventNextDate = "NextDate";
        public const string LogEventLastDate = "LastDate";
    }
}