# Datadog Metrics Integration

This document explains how to send OpenTelemetry metrics to Datadog from the Hexagon .NET application.

## Why Prometheus Instead of OTLP?

**Datadog's OTLP endpoint does NOT support metrics** - it only accepts traces. Therefore, we export metrics using **Prometheus format** which Datadog can scrape.

## Option 1: Prometheus Exporter (Recommended)

The application exposes metrics at `/metrics` endpoint in Prometheus format. Datadog can scrape this endpoint to collect metrics.

### Configuration

This is the **default** configuration. The Prometheus exporter is automatically enabled.

```bash
# Application startup logs will show:
[OpenTelemetry] Metrics: Prometheus exporter enabled at /metrics endpoint
[OpenTelemetry] Configure Datadog to scrape http://localhost:5112/metrics
[OpenTelemetry] Prometheus metrics endpoint mapped at /metrics
```

### Verify Metrics Endpoint

```bash
# Check metrics are being exposed
curl http://localhost:5112/metrics

# Sample output:
# # TYPE http_server_request_duration_seconds histogram
# http_server_request_duration_seconds_bucket{...,le="0.005"} 10
# http_server_request_duration_seconds_bucket{...,le="0.01"} 15
# ...
```

### Available Metrics

The following OpenTelemetry instrumentation metrics are automatically collected:

#### ASP.NET Core Metrics

- `http_server_request_duration_seconds` - HTTP request duration histogram
- `http_server_active_requests` - Number of active HTTP requests
- `aspnetcore_routing_match_attempts` - Number of routing match attempts

#### HTTP Client Metrics

- `http_client_request_duration_seconds` - HTTP client request duration
- `http_client_active_requests` - Active outgoing HTTP requests

#### Runtime Metrics

- `process_runtime_dotnet_gc_collections_count` - GC collection count
- `process_runtime_dotnet_gc_heap_size_bytes` - GC heap size
- `process_runtime_dotnet_gc_pause_time_seconds` - GC pause duration
- `process_runtime_dotnet_thread_pool_threads_count` - Thread pool thread count
- `process_runtime_dotnet_assemblies_count` - Number of loaded assemblies

#### Process Metrics

- `process_cpu_time_seconds_total` - Total CPU time
- `process_memory_usage_bytes` - Physical memory usage
- `process_memory_virtual_bytes` - Virtual memory usage

### Configure Datadog to Scrape Metrics

#### Local Development (Datadog Agent)

Install and configure the Datadog Agent to scrape the Prometheus endpoint:

**Step 1: Install Datadog Agent**

```bash
# macOS
brew install datadog-agent

# Linux
DD_API_KEY=******** \
DD_SITE="us5.datadoghq.com" \
bash -c "$(curl -L https://s3.amazonaws.com/dd-agent/scripts/install_script_agent7.sh)"
```

**Step 2: Configure Prometheus Check**

Create file: `/opt/datadog-agent/etc/conf.d/prometheus.d/conf.yaml`

```yaml
instances:
  - prometheus_url: http://localhost:5112/metrics
    namespace: hexagon_dotnet_app
    metrics:
      - "*"
    tags:
      - env:development
      - service:hexagon-dotnet-app
```

**Step 3: Restart Datadog Agent**

```bash
# macOS
sudo launchctl stop com.datadoghq.agent
sudo launchctl start com.datadoghq.agent

# Linux
sudo systemctl restart datadog-agent
```

**Step 4: Verify Agent is Scraping**

```bash
# Check agent status
sudo datadog-agent status

# Look for output:
# prometheus
# ----------
#   Instance ID: prometheus:xxxx [OK]
#   Configuration Source: file:/opt/datadog-agent/etc/conf.d/prometheus.d/conf.yaml
#   Total Runs: 5
#   Metric Samples: Last Run: 42, Total: 210
#   Events: Last Run: 0, Total: 0
#   Service Checks: Last Run: 1, Total: 5
#   Average Execution Time : 15ms
```

#### Production (Kubernetes with Datadog Operator)

**Annotate Deployment:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hexagon-dotnet-app
spec:
  template:
    metadata:
      annotations:
        ad.datadoghq.com/hexagon-dotnet-app.check_names: '["openmetrics"]'
        ad.datadoghq.com/hexagon-dotnet-app.init_configs: "[{}]"
        ad.datadoghq.com/hexagon-dotnet-app.instances: |
          [
            {
              "prometheus_url": "http://%%host%%:5112/metrics",
              "namespace": "hexagon_dotnet_app",
              "metrics": ["*"]
            }
          ]
    spec:
      containers:
        - name: hexagon-dotnet-app
          image: hexagon-dotnet-app:latest
          ports:
            - containerPort: 5112
              name: metrics
```

**Or use ServiceMonitor (with Prometheus Operator + Datadog integration):**

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: hexagon-dotnet-app
  labels:
    app: hexagon-dotnet-app
spec:
  selector:
    matchLabels:
      app: hexagon-dotnet-app
  endpoints:
    - port: metrics
      path: /metrics
      interval: 30s
```

### View Metrics in Datadog

After configuring the agent to scrape metrics, view them in Datadog:

1. **Metrics Explorer**: https://us5.datadoghq.com/metric/explorer
2. **Dashboard**: Create dashboards with metrics like:
   - `hexagon_dotnet_app.http_server_request_duration_seconds`
   - `hexagon_dotnet_app.process_memory_usage_bytes`
   - `hexagon_dotnet_app.http_server_active_requests`

## Option 2: DogStatsD (Alternative)

For scenarios where you can't use Prometheus scraping, you can send metrics directly to a Datadog Agent using the StatsD protocol.

### Requirements

- Datadog Agent running locally or accessible via network
- StatsD listener enabled on the agent (default port 8125)

### Configuration

Set environment variables to enable StatsD mode:

```bash
export METRICS_EXPORTER=statsd
export DD_AGENT_HOST=localhost
export DD_DOGSTATSD_PORT=8125

dotnet run --project src/App.Api
```

### Application Startup

With StatsD mode, the application logs will show:

```
[OpenTelemetry] Metrics: DogStatsD mode enabled - expecting agent at localhost:8125
[OpenTelemetry] Note: Requires Datadog Agent running locally. See docs/DATADOG_METRICS.md
```

### Limitations

**Note**: The current implementation with `StatsdClient` package doesn't automatically bridge OpenTelemetry metrics to StatsD. For full OpenTelemetry → StatsD integration, you would need to implement a custom OpenTelemetry exporter or use custom metrics instrumentation with the `StatsdClient` library directly.

**Recommendation**: Use **Option 1 (Prometheus)** for OpenTelemetry metrics. Reserve StatsD for custom application-specific metrics.

## Option 3: Disable Metrics

To disable metrics entirely:

```bash
export METRICS_EXPORTER=disabled
```

Application logs:

```
[OpenTelemetry] Metrics: Disabled
```

## Troubleshooting

### Metrics Endpoint Returns 404

**Problem**: `curl http://localhost:5112/metrics` returns 404

**Solution**: Check that Prometheus exporter is enabled:

- Verify `METRICS_EXPORTER` is not set to `statsd` or `disabled`
- Check startup logs for: `[OpenTelemetry] Prometheus metrics endpoint mapped at /metrics`
- Ensure application started successfully

### No Metrics in Datadog

**Problem**: Metrics appear at `/metrics` endpoint but don't show up in Datadog

**Solution**:

1. Verify Datadog Agent is running: `sudo datadog-agent status`
2. Check Prometheus integration is configured: `cat /opt/datadog-agent/etc/conf.d/prometheus.d/conf.yaml`
3. Check agent logs: `tail -f /var/log/datadog/agent.log`
4. Verify agent can reach the metrics endpoint: `curl http://localhost:5112/metrics` from agent host
5. Check for firewall rules blocking port 5112

### High Cardinality Warning

**Problem**: Datadog shows warnings about high cardinality metrics

**Solution**: OpenTelemetry metrics use labels (Prometheus terminology) for dimensions. High cardinality occurs when a label has many unique values (e.g., user IDs, full URLs). The default ASP.NET Core instrumentation uses `http_route` (low cardinality) instead of full paths (high cardinality).

To reduce cardinality:

- Use route templates: `/api/v1/todos/{id}` not `/api/v1/todos/123`
- Don't add dynamic values as metric labels
- Use aggregation in Datadog queries

## Summary

| Method         | Pros                                                               | Cons                                                       | Use Case                           |
| -------------- | ------------------------------------------------------------------ | ---------------------------------------------------------- | ---------------------------------- |
| **Prometheus** | ✅ Native OTel support<br>✅ No code changes<br>✅ Standard format | ⚠️ Requires scraping setup                                 | **Recommended** for most scenarios |
| **StatsD**     | ✅ Push-based<br>✅ Direct to agent                                | ❌ Requires custom integration<br>❌ No auto OTel bridging | Custom app metrics                 |
| **Disabled**   | ✅ Reduces overhead                                                | ❌ No metrics visibility                                   | Testing, troubleshooting           |

## References

- [Datadog OpenTelemetry Support](https://docs.datadoghq.com/opentelemetry/)
- [Datadog Prometheus Integration](https://docs.datadoghq.com/integrations/prometheus/)
- [OpenTelemetry .NET Metrics](https://opentelemetry.io/docs/instrumentation/net/metrics/)
- [ASP.NET Core Metrics](https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics)
