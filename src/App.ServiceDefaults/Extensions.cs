using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for configuring Aspire service defaults.
/// </summary>
public static class AspireServiceDefaultsExtensions
{
    /// <summary>
    /// Adds Aspire service defaults including OpenTelemetry, service discovery, and resilience.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry logging, tracing, and metrics.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder
    )
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                var serviceName =
                    builder.Configuration["OTEL_SERVICE_NAME"]
                    ?? builder.Configuration["DD_SERVICE"]
                    ?? "hexagon-dotnet-app";
                var serviceVersion =
                    builder.Configuration["DD_VERSION"]
                    ?? builder.Configuration["OTEL_SERVICE_VERSION"]
                    ?? "1.0.0";
                var environment =
                    builder.Configuration["DD_ENV"]
                    ?? builder.Configuration["DOTNET_ENVIRONMENT"]
                    ?? "development";

                resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: serviceVersion,
                        serviceInstanceId: Environment.MachineName
                    )
                    .AddAttributes(
                        new Dictionary<string, object>
                        {
                            ["deployment.environment"] = environment,
                            ["service.namespace"] = "hexagon",
                            ["host.name"] = Environment.MachineName,
                            ["process.runtime.name"] = ".NET",
                            ["process.runtime.version"] = Environment.Version.ToString(),
                            ["telemetry.sdk.language"] = "dotnet",
                        }
                    );
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Record exception details in traces
                        options.RecordException = true;
                        // Enrich spans with additional HTTP request/response details
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.method", httpRequest.Method);
                            activity.SetTag("http.request.path", httpRequest.Path.Value);
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Record exception details in HTTP client traces
                        options.RecordException = true;
                        // Enrich spans with HTTP request/response details
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag(
                                "http.request.uri",
                                httpRequestMessage.RequestUri?.ToString()
                            );
                        };
                        options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                        {
                            activity.SetTag(
                                "http.response.status_code",
                                (int)httpResponseMessage.StatusCode
                            );
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        // Capture detailed EF Core query information
                        options.SetDbStatementForText = true;
                        // Include EF Core command text in traces
                        options.SetDbStatementForStoredProcedure = true;
                        // Record EF Core exceptions
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag(
                                "db.execution_time_ms",
                                activity.Duration.TotalMilliseconds
                            );
                        };
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        // Capture SQL command text for better observability
                        options.SetDbStatementForText = true;
                        // Record SQL command execution time
                        options.RecordException = true;
                    });
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(
        this IHostApplicationBuilder builder
    )
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        );

        if (useOtlpExporter)
        {
            var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            Console.WriteLine($"[OpenTelemetry] Configuring OTLP Exporter to: {endpoint}");
            Console.WriteLine(
                $"[OpenTelemetry] Service: {builder.Configuration["OTEL_SERVICE_NAME"] ?? "hexagon-dotnet-app"}"
            );

            // Datadog's OTLP endpoint only supports TRACES, not metrics or logs
            // - Traces: Supported via OTLP ✅
            // - Metrics: Use Prometheus + Datadog scraping OR StatsD ✅
            // - Logs: Use Serilog HTTP sink (NOT OTLP) ✅

            // Configure OTLP exporter ONLY for traces
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
            {
                tracing.AddOtlpExporter();
            });

            Console.WriteLine("[OpenTelemetry] OTLP exporter configured for TRACES only");
            Console.WriteLine("[OpenTelemetry] Logs: Using Serilog HTTP sink directly");
        }
        else
        {
            Console.WriteLine(
                "[OpenTelemetry] OTLP Exporter disabled - no OTEL_EXPORTER_OTLP_ENDPOINT configured"
            );
        }

        // Configure metrics export
        builder.AddMetricsExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddMetricsExporters(this IHostApplicationBuilder builder)
    {
        // Option 1: Prometheus exporter (recommended for Datadog integration)
        // Exposes metrics at /metrics endpoint for Datadog to scrape
        var usePrometheus =
            builder.Configuration["METRICS_EXPORTER"] != "statsd"
            && builder.Configuration["METRICS_EXPORTER"] != "disabled";

        if (usePrometheus)
        {
            builder
                .Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddPrometheusExporter();
                });

            Console.WriteLine(
                "[OpenTelemetry] Metrics: Prometheus exporter enabled at /metrics endpoint"
            );
            Console.WriteLine(
                "[OpenTelemetry] Configure Datadog to scrape http://localhost:5112/metrics"
            );
        }

        // Option 2: DogStatsD (direct push to Datadog)
        // Set METRICS_EXPORTER=statsd and provide DD_AGENT_HOST and DD_DOGSTATSD_PORT
        var useStatsd = builder.Configuration["METRICS_EXPORTER"] == "statsd";

        if (useStatsd)
        {
            var ddAgentHost = builder.Configuration["DD_AGENT_HOST"] ?? "localhost";
            var ddStatsdPort = int.Parse(
                builder.Configuration["DD_DOGSTATSD_PORT"] ?? "8125",
                System.Globalization.CultureInfo.InvariantCulture
            );

            Console.WriteLine(
                $"[OpenTelemetry] Metrics: DogStatsD mode enabled - expecting agent at {ddAgentHost}:{ddStatsdPort}"
            );
            Console.WriteLine(
                "[OpenTelemetry] Note: Requires Datadog Agent running locally. See docs/DATADOG_METRICS.md"
            );
        }

        if (builder.Configuration["METRICS_EXPORTER"] == "disabled")
        {
            Console.WriteLine("[OpenTelemetry] Metrics: Disabled");
        }

        return builder;
    }

    /// <summary>
    /// Adds default health checks for liveness and readiness.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
    public static IHostApplicationBuilder AddDefaultHealthChecks(
        this IHostApplicationBuilder builder
    )
    {
        builder
            .Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps default health check endpoints.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    /// <returns>The <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered live
        app.MapHealthChecks(
            "/alive",
            new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }
        );

        // Map Prometheus metrics endpoint if enabled
        var usePrometheus =
            app.Configuration["METRICS_EXPORTER"] != "statsd"
            && app.Configuration["METRICS_EXPORTER"] != "disabled";

        if (usePrometheus)
        {
            app.MapPrometheusScrapingEndpoint();
            Console.WriteLine("[OpenTelemetry] Prometheus metrics endpoint mapped at /metrics");
        }

        return app;
    }
}
