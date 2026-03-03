using System.Diagnostics;
using App.Api.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using Moq;
using Xunit;

namespace App.Api.Tests.Logging;

public class OpenTelemetryTraceEnricherTests
{
    private readonly OpenTelemetryTraceEnricher _enricher;
    private readonly Mock<ILogEventPropertyFactory> _propertyFactoryMock;
    private readonly LogEvent _logEvent;

    public OpenTelemetryTraceEnricherTests()
    {
        _enricher = new OpenTelemetryTraceEnricher();
        _propertyFactoryMock = new Mock<ILogEventPropertyFactory>();
        _logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse(string.Empty),
            Enumerable.Empty<LogEventProperty>()
        );
    }

    [Fact]
    public void Enrich_WithNoActivity_ShouldNotAddProperties()
    {
        // Arrange
        Activity.Current = null;

        // Act
        _enricher.Enrich(_logEvent, _propertyFactoryMock.Object);

        // Assert
        Assert.Empty(_logEvent.Properties);
    }

    [Fact]
    public void Enrich_WithActivity_ShouldAddTraceAndSpanIds()
    {
        // Arrange
        using var activity = new Activity("TestAction");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        Activity.Current = activity;

        _propertyFactoryMock.Setup(f => f.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns((string name, object value, bool _) => new LogEventProperty(name, new ScalarValue(value)));

        // Act
        _enricher.Enrich(_logEvent, _propertyFactoryMock.Object);

        // Assert
        Assert.True(_logEvent.Properties.ContainsKey("TraceId"));
        Assert.True(_logEvent.Properties.ContainsKey("SpanId"));
        Assert.True(_logEvent.Properties.ContainsKey("dd.trace_id"));
        Assert.True(_logEvent.Properties.ContainsKey("dd.span_id"));
        
        Activity.Current = null;
    }

    [Fact]
    public void Enrich_WithActivity_DdTraceIdShouldBeDecimalString()
    {
        // Arrange — dd.trace_id must be the lower 64-bits of the W3C trace ID as a decimal number
        using var activity = new Activity("DecimalCheck");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        Activity.Current = activity;

        _propertyFactoryMock
            .Setup(f => f.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns((string name, object value, bool _) => new LogEventProperty(name, new ScalarValue(value)));

        // Act
        _enricher.Enrich(_logEvent, _propertyFactoryMock.Object);

        // Assert dd.trace_id is a valid non-empty decimal string
        Assert.True(_logEvent.Properties.TryGetValue("dd.trace_id", out var ddTraceId));
        var ddTraceIdStr = ((ScalarValue)ddTraceId).Value?.ToString();
        Assert.NotNull(ddTraceIdStr);
        Assert.True(ulong.TryParse(ddTraceIdStr, out _),
            $"dd.trace_id should be a decimal ulong string but was: {ddTraceIdStr}");

        Assert.True(_logEvent.Properties.TryGetValue("dd.span_id", out var ddSpanId));
        var ddSpanIdStr = ((ScalarValue)ddSpanId).Value?.ToString();
        Assert.NotNull(ddSpanIdStr);
        Assert.True(ulong.TryParse(ddSpanIdStr, out _),
            $"dd.span_id should be a decimal ulong string but was: {ddSpanIdStr}");

        Activity.Current = null;
    }
}
