// Copyright © Korshun Ivan. All rights reserved.

using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Template.Library.DataSource.Dapper;

public sealed partial class SqlCommandLoggingObserver(ILogger<SqlCommandLoggingObserver> logger) : IObserver<DiagnosticListener>
{
    private readonly ILogger _logger = logger;

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == "SqlClientDiagnosticListener")
        {
            value.Subscribe(new CommandTextExtractor(_logger));
        }
    }

    public void OnCompleted() { }

    public void OnError(Exception error) { }

    private sealed partial class CommandTextExtractor(ILogger logger) : IObserver<KeyValuePair<string, object?>>
    {
        private readonly ILogger _logger = logger;

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            if (value.Key == "Microsoft.Data.SqlClient.WriteCommandBefore"
                && value.Value is SqlClientCommandBefore beforeData
                && beforeData.Command != null)
            {
                SqlCommand cmd = beforeData.Command;

                var sb = new StringBuilder();
                sb.AppendLine("--- [SQL EXECUTING] ---");
                sb.AppendLine(cmd.CommandText);

                if (cmd.Parameters.Count > 0)
                {
                    sb.AppendLine("Parameters:");
                    foreach (DbParameter param in cmd.Parameters)
                    {
                        sb.AppendLine(
                            CultureInfo.InvariantCulture,
                            $"  -> {param.ParameterName} = {param.Value ?? "NULL"} ({param.DbType})");
                    }
                }

                LogStart(sb.ToString().TrimEnd());
                return;
            }

            if (value.Key == "Microsoft.Data.SqlClient.WriteCommandAfter"
                && value.Value is SqlClientCommandAfter afterData)
            {
                IDictionary? stats = afterData.Statistics;
                if (stats?.Count > 0)
                {
                    long executionTimeMs = stats.Contains("ExecutionTime") ? (long)stats["ExecutionTime"]! : 0;
                    long networkServerTime = stats.Contains("NetworkServerTime") ? (long)stats["NetworkServerTime"]! : 0;
                    long networkPackets = stats.Contains("BuffersReceived") ? (long)stats["BuffersReceived"]! : 0;

                    LogEnd(executionTimeMs, networkServerTime, networkPackets);
                }
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        /// <summary>Workaround for CS1570 error in auto-generated comment.</summary>
        [LoggerMessage(LogLevel.Debug, "\e[96m{Message}\e[0m")]
        private partial void LogStart(string message);

        /// <summary>Workaround for CS1570 error in auto-generated comment.</summary>
        [LoggerMessage(
            LogLevel.Debug,
            "\e[96m[SQL EXECUTED] ExecutionTime: {ExecutionTimeMs} ms | NetworkServerTime: {NetworkServerTime} ms | Network Packets: {NetworkPackets}\e[0m")]
        private partial void LogEnd(long executionTimeMs, long networkServerTime, long networkPackets);
    }
}
