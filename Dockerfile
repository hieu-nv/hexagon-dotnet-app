# Multi-stage Dockerfile for Hexagon .NET App

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage layer caching
COPY src/App.slnx ./
COPY src/App.Api/*.csproj ./App.Api/
COPY src/App.AppHost/*.csproj ./App.AppHost/
COPY src/App.Core/*.csproj ./App.Core/
COPY src/App.Data/*.csproj ./App.Data/
COPY src/App.Gateway/*.csproj ./App.Gateway/
COPY src/App.ServiceDefaults/*.csproj ./App.ServiceDefaults/

# Restore dependencies
RUN dotnet restore App.slnx

# Copy everything else and build
COPY src/ ./
RUN dotnet build App.slnx -c Release --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /app/publish --no-build

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Run as non-root user
USER app

# Copy published files
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "App.Api.dll"]
