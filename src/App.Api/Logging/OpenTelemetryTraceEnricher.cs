using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace App.Api.Logging;

/// <summary>
/// Serilog enricher that adds OpenTelemetry trace context (trace_id, span_id) to log events.
/// This enables correlation between logs and APM traces in Datadog.
/// </summary>
internal sealed class OpenTelemetryTraceEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event with OpenTelemetry trace context.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        // Add W3C trace IDs (hex format)
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString())
        );
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString())
        );

        // Add Datadog-specific trace IDs (decimal format)
        // Datadog expects the lower 64 bits of the trace ID as a decimal number
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "dd.trace_id",
                Convert.ToUInt64(activity.TraceId.ToString().Substring(16), 16).ToString()
            )
        );
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "dd.span_id",
                Convert.ToUInt64(activity.SpanId.ToString(), 16).ToString()
            )
        );
    }
}
