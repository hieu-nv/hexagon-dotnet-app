# Datadog Log Forwarding Setup

This document describes how logs are forwarded from the Hexagon .NET App to the Datadog agent.

## Architecture

The application uses **Serilog** with multiple sinks to forward logs:

1. **Console Sink**: Logs to console for development/debugging
2. **File Sink**: Writes JSON-formatted logs to `/var/log/hexagon-app/app.log`
3. **Datadog Sink**: Sends logs directly to Datadog agent via HTTP

## Configuration

### Application Configuration

The application is configured in [Program.cs](../src/App.Api/Program.cs) with:

- **Minimum log level**: Information (Warning for Microsoft.AspNetCore)
- **Enrichment**: Application name, environment, service name, context
- **File logging**: JSON format with daily rolling, 7-day retention at `logs/app.log`
- **Datadog HTTP Intake**: Direct to Datadog cloud at `https://http-intake.logs.us5.datadoghq.com`

**Note**: Logs are sent directly to Datadog's cloud intake API, not to the local agent. The local agent is only used for APM traces (port 8126) and metrics (port 8125).

### Datadog Agent Configuration

The Datadog agent is configured to:

1. **Collect logs from file**: Tails `/var/log/hexagon-app/app*.log`
2. **Listen for direct HTTP logs**: Port 8126
3. **StatsD metrics**: Port 8125

Configuration file: [datadog-logs.yaml](../datadog-logs.yaml)

## Running the Setup

### 1. Start the Datadog Agent

```bash
./run-datadog-agent.sh
```

This script:
- Starts the Datadog agent container
- Exposes ports 8125 (StatsD) and 8126 (APM/Logs)
- Mounts the log directory: `/var/log/hexagon-app`
- Loads the log collection configuration

### 2. Verify the Agent is Running

```bash
# Check if agent is running
podman ps | grep dd-agent

# View agent logs
podman logs -f dd-agent

# Check port status
netstat -naup | grep 8125
netstat -natp | grep 8126
```

### 3. Run the Application

```bash
cd src/App.Api
dotnet run
```

### 4. Verify Logs are Being Forwarded

Check the application is writing logs:

```bash
tail -f /var/log/hexagon-app/app*.log
```

Check Datadog agent is processing logs:

```bash
podman exec dd-agent agent status | grep -A 20 "Logs Agent"
```

## Log Format

Logs are written in JSON format with the following structure:

```json
{
  "@t": "2026-02-24T10:30:45.1234567Z",
  "@mt": "Starting Hexagon .NET App",
  "@l": "Information",
  "Application": "App.Api",
  "Environment": "Development",
  "service": "hexagon-dotnet-app",
  "SourceContext": "Program"
}
```

## Viewing Logs in Datadog

### Access the Logs Dashboard

1. Navigate to Datadog UI: https://us5.datadoghq.com
2. Go to **Logs** → **Logs Explorer** (or use direct link: https://us5.datadoghq.com/logs)
3. Wait 1-2 minutes for logs to appear (there's a slight ingestion delay)

### Filter Your Logs

Use these filters in the search bar:
```
service:hexagon-dotnet-app
```

Or combine multiple filters:
```
service:hexagon-dotnet-app source:csharp env:Development
```

### Available Log Attributes

- **service**: `hexagon-dotnet-app`
- **source**: `csharp`
- **env**: `Development` (or `Production`)
- **Application**: `App.Api`
- **host**: Your machine name
- **version**: `1.0.0`

### Troubleshooting Missing Logs

If logs don't appear after 2-3 minutes:

1. **Verify API Key**: Ensure the API key in Program.cs matches your Datadog account
2. **Check Network**: Logs are sent to `https://http-intake.logs.us5.datadoghq.com`
3. **Verify Application**: Run `curl http://localhost:5112/health` to ensure app is running
4. **Check Log Files**: Verify logs are being written locally:
   ```bash
   tail -f /workspaces/hexagon-dotnet-app/src/App.Api/logs/app*.log
   ```

## Troubleshooting

### Logs not appearing in Datadog

1. **Check log file permissions**:
   ```bash
   ls -la /var/log/hexagon-app/
   ```

2. **Verify Datadog agent can read logs**:
   ```bash
   podman exec dd-agent ls -la /var/log/hexagon-app/
   ```

3. **Check agent status**:
   ```bash
   podman exec dd-agent agent status
   ```

4. **Verify ports are open**:
   ```bash
   netstat -naup | grep 8125
   netstat -natp | grep 8126
   ```

### Application can't write to log directory

```bash
# Create directory with proper permissions
sudo mkdir -p /var/log/hexagon-app
sudo chmod 777 /var/log/hexagon-app
```

### Agent not collecting logs from file

Check the agent configuration is mounted correctly:

```bash
podman exec dd-agent cat /etc/datadog-agent/conf.d/hexagon-app.d/conf.yaml
```

## Configuration Files

- **Application logging**: [src/App.Api/Program.cs](../src/App.Api/Program.cs)
- **Log settings**: [src/App.Api/appsettings.json](../src/App.Api/appsettings.json)
- **Datadog log config**: [datadog-logs.yaml](../datadog-logs.yaml)
- **Agent startup**: [run-datadog-agent.sh](../run-datadog-agent.sh)

## NuGet Packages Used

- `Serilog.AspNetCore` - Serilog integration for ASP.NET Core
- `Serilog.Sinks.Datadog.Logs` - Direct Datadog HTTP logging
- `Serilog.Sinks.File` - File-based logging for agent tailing
