// Infrastructure/Persistence/Interceptors/DbLoggingInterceptor.cs
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace NashunumaApp.Infrastructure.Persistence.Interceptors
{
    public class DbLoggingInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<DbLoggingInterceptor> _logger;
        private readonly Stopwatch _stopwatch = new();

        public DbLoggingInterceptor(ILogger<DbLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            _stopwatch.Restart();
            LogDebugCommand(command, "Executing");
            return base.ReaderExecuting(command, eventData, result);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            _stopwatch.Stop();
            LogCommandResult(command, eventData, _stopwatch.ElapsedMilliseconds);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            _stopwatch.Restart();
            LogDebugCommand(command, "Executing NonQuery");
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            _stopwatch.Stop();
            LogCommandResult(command, eventData, _stopwatch.ElapsedMilliseconds, result);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            _stopwatch.Restart();
            LogDebugCommand(command, "Executing Scalar");
            return base.ScalarExecuting(command, eventData, result);
        }

        public override object ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result)
        {
            _stopwatch.Stop();
            LogCommandResult(command, eventData, _stopwatch.ElapsedMilliseconds, result);
            return base.ScalarExecuted(command, eventData, result);
        }

        private void LogDebugCommand(DbCommand command, string action)
        {
            if (command == null) return;

            var parameters = new StringBuilder();
            foreach (DbParameter param in command.Parameters)
            {
                parameters.AppendLine($"    {param.ParameterName} = {param.Value} ({param.DbType})");
            }

            _logger.LogDebug("SQL {Action}: {CommandText}{NewLine}Parameters: {Parameters}",
                action,
                command.CommandText,
                Environment.NewLine,
                parameters.ToString());
        }

        private void LogCommandResult(DbCommand command, CommandExecutedEventData eventData, long durationMs, object result = null)
        {
            if (durationMs > 1000)
            {
                _logger.LogWarning(" SLOW SQL Query ({DurationMs}ms): {CommandText}",
                    durationMs,
                    command.CommandText);

                // Also log with Serilog
                Serilog.Log.Warning(" SLOW SQL Query ({DurationMs}ms): {CommandText}",
                    durationMs,
                    command.CommandText);
            }
            else
            {
                _logger.LogDebug(" SQL Command completed in {DurationMs}ms", durationMs);
            }

            // Log error if any
            //if (eventData.Exception != null)
            //{
            //    _logger.LogError(eventData.Exception, " SQL Command failed: {CommandText}", command.CommandText);
            //    Serilog.Log.Error(eventData.Exception, " SQL Command failed: {CommandText}", command.CommandText);
            }
        }
    
}