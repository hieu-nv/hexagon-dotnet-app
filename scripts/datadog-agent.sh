#!/bin/bash
# 
# Start Datadog Agent for local APM development
# 
# This script runs the Datadog agent in a Docker/Podman container
# to collect APM traces, metrics, and logs from the application.
#

set -e

# Detect container runtime (Docker or Podman)
if command -v docker &> /dev/null; then
    CONTAINER_RUNTIME="docker"
elif command -v podman &> /dev/null; then
    CONTAINER_RUNTIME="podman"
else
    echo "❌ Error: Neither Docker nor Podman is installed"
    exit 1
fi

echo "🐕 Starting Datadog Agent using ${CONTAINER_RUNTIME}..."

# Stop and remove existing agent container if it exists
if ${CONTAINER_RUNTIME} ps -a --format '{{.Names}}' | grep -q '^dd-agent$'; then
    echo "🛑 Stopping existing dd-agent container..."
    ${CONTAINER_RUNTIME} stop dd-agent || true
    ${CONTAINER_RUNTIME} rm dd-agent || true
fi

# Get the directory of this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check if DD_API_KEY is set
if [ -z "${DD_API_KEY}" ]; then
    echo "⚠️  Warning: DD_API_KEY environment variable is not set"
    echo "   The agent will still work for local development,"
    echo "   but metrics/traces won't be sent to Datadog cloud."
    echo ""
    echo "   To set it: export DD_API_KEY=your-api-key"
    echo ""
fi

# Run Datadog agent
${CONTAINER_RUNTIME} run -d \
    --name dd-agent \
    --hostname hexagon-dotnet-local \
    -e DD_API_KEY="${DD_API_KEY:-dummy-key-for-local-dev}" \
    -e DD_SITE="us5.datadoghq.com" \
    -e DD_ENV="development" \
    -e DD_SERVICE="hexagon-dotnet-app" \
    -e DD_VERSION="1.0.0" \
    -e DD_APM_ENABLED=true \
    -e DD_APM_NON_LOCAL_TRAFFIC=false \
    -e DD_DOGSTATSD_NON_LOCAL_TRAFFIC=true \
    -e DD_LOGS_ENABLED=true \
    -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT="0.0.0.0:4317" \
    -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT="0.0.0.0:4318" \
    -p 8125:8125/udp \
    -p 8126:8126 \
    -p 4317:4317 \
    -p 4318:4318 \
    -v /var/run/docker.sock:/var/run/docker.sock:ro \
    -v "${SCRIPT_DIR}/datadog-agent/datadog.yaml:/etc/datadog-agent/datadog.yaml:ro" \
    -v "${SCRIPT_DIR}/datadog-agent/conf.d:/etc/datadog-agent/conf.d:ro" \
    -v "${SCRIPT_DIR}/../src/App.Api/logs:/app/logs:ro" \
    gcr.io/datadoghq/agent:latest

echo ""
echo "✅ Datadog agent started successfully!"
echo ""
echo "📊 Agent Status:"
echo "   - StatsD (metrics):     localhost:8125"
echo "   - APM (traces):         localhost:8126"
echo "   - OTLP gRPC:           localhost:4317"
echo "   - OTLP HTTP:           localhost:4318"
echo ""
echo "🔍 Useful commands:"
echo "   Check status:  ${CONTAINER_RUNTIME} exec dd-agent agent status"
echo "   View logs:     ${CONTAINER_RUNTIME} logs -f dd-agent"
echo "   Stop agent:    ${CONTAINER_RUNTIME} stop dd-agent"
echo "   Remove agent:  ${CONTAINER_RUNTIME} rm dd-agent"
echo ""
echo "💡 Now start your application with: dotnet run --project src/App.Api"
echo ""
