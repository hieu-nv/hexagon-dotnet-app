#!/bin/bash
# Test Datadog Integration

echo "🔍 Testing Datadog APM & Logging Integration"
echo "=============================================="
echo ""

# Check environment variables
echo "📋 Environment Configuration:"
echo "  DD_API_KEY: ${DD_API_KEY:0:8}..."
echo "  DD_SITE: $DD_SITE"
echo "  OTEL_EXPORTER_OTLP_ENDPOINT: $OTEL_EXPORTER_OTLP_ENDPOINT"
echo ""

# Start the application in background
echo "🚀 Starting application..."
cd "$(dirname "$0")/.."
dotnet run --project src/App.Api --no-launch-profile \
  -e DD_API_KEY="${DD_API_KEY}" \
  -e DD_SITE="${DD_SITE}" \
  -e DD_ENV="development" \
  -e DD_SERVICE="hexagon-dotnet-app" \
  -e DD_VERSION="1.0.0" \
  -e OTEL_EXPORTER_OTLP_ENDPOINT="${OTEL_EXPORTER_OTLP_ENDPOINT}" \
  -e OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf" \
  -e OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=${DD_API_KEY}" \
  -e OTEL_SERVICE_NAME="hexagon-dotnet-app" \
  -e OTEL_RESOURCE_ATTRIBUTES="deployment.environment=development,service.version=1.0.0" \
  > /tmp/hexagon-app.log 2>&1 &

APP_PID=$!
echo "  App PID: $APP_PID"

# Wait for app to start
echo "⏳ Waiting for app to start..."
sleep 5

# Test health endpoint
echo ""
echo "🏥 Testing health endpoint..."
curl -s http://localhost:5112/health | jq '.' || echo "Health check failed"

# Generate test traffic
echo ""
echo "📊 Generating test traffic..."

echo "  1. Creating todos..."
curl -s -X POST http://localhost:5112/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Datadog APM Integration","isCompleted":false}' | jq '.' || echo "Failed"

echo "  2. Listing todos..."
curl -s http://localhost:5112/todos | jq '.' || echo "Failed"

echo "  3. Getting Pokemon (external HTTP trace)..."
curl -s http://localhost:5112/pokemon/pikachu | jq '.' || echo "Failed"

echo ""
echo "🔍 Checking what was sent to Datadog..."
echo "Logs from app startup:"
tail -20 /tmp/hexagon-app.log

# Check local log file for trace IDs
echo ""
echo "📝 Checking log file for trace correlation..."
if [ -f "logs/app.log" ]; then
  echo "Recent log entries with TraceId:"
  tail -5 logs/app.log | grep -o '"TraceId":"[^"]*"' || echo "No TraceId found in logs"
  
  echo ""
  echo "Recent log entries with dd.trace_id:"
  tail -5 logs/app.log | grep -o '"dd.trace_id":"[^"]*"' || echo "No dd.trace_id found in logs"
else
  echo "Log file not found at logs/app.log"
fi

echo ""
echo "🛑 Stopping application..."
kill $APP_PID 2>/dev/null

echo ""
echo "✅ Test complete!"
echo ""
echo "📍 Check your Datadog dashboards:"
echo "  - Traces: https://us5.datadoghq.com/apm/traces?query=service%3Ahexagon-dotnet-app"
echo "  - Logs: https://us5.datadoghq.com/logs?query=service%3Ahexagon-dotnet-app"
echo ""
echo "⏰ Note: It may take 1-2 minutes for data to appear in Datadog"
