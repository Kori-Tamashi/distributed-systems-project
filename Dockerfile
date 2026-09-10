# syntax=docker/dockerfile:1.5

# =============================================================================
# Stage 1: BASE - Dependencies Caching
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base

# Set working directory
WORKDIR /src

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# =============================================================================
# Stage 2: BUILD - Restore Dependencies
# =============================================================================
FROM base AS build-deps

# Copy only solution/project files first (leverage Docker layer caching)
COPY ["src/core/core.csproj", "src/core/"]
COPY ["src/businesslogic/businesslogic.csproj", "src/businesslogic/"]
COPY ["src/dataaccess/dataaccess.csproj", "src/dataaccess/"]
COPY ["src/presentation/presentation.csproj", "src/presentation/"]
COPY ["src/tests/tests.csproj", "src/tests/"]

# Restore dependencies (cached if dependencies haven't changed)
RUN dotnet restore "src/presentation/presentation.csproj"

# =============================================================================
# Stage 3: BUILD - Compile Application
# =============================================================================
FROM build-deps AS build

# Copy all source code
COPY src/ ./src/

# Build the application (Release mode)
RUN dotnet build "src/presentation/presentation.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# =============================================================================
# Stage 4: PUBLISH - Create Release Artifacts
# =============================================================================
FROM build AS publish

# Publish the application (Release mode)
RUN dotnet publish "src/presentation/presentation.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================================
# Stage 5: RUNTIME - Final Production Image
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Build args for configuration
ARG VERSION=1.0.0
ARG BUILD_DATE

# Labels for image metadata
LABEL maintainer="kori-tamashi" \
      version="${VERSION}" \
      description="Person API - Lab 01 Distributed Systems" \
      org.opencontainers.image.title="person-api" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.created="${BUILD_DATE}"

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_GCServer=1

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Graceful shutdown timeout
ENV DOTNET_SYSTEM_Threading_Thread_Abort_CrossAppDomain=false

# Run the application
ENTRYPOINT ["dotnet", "presentation.dll"]
