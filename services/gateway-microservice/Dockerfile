# ===========================================
# Gateway Microservice Dockerfile
# Multi-stage build for .NET 10
# ===========================================

# Stage 1: Base build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /src
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Stage 2: Restore dependencies
FROM base AS build-deps
COPY src/core/core.csproj src/core/
COPY src/businesslogic/businesslogic.csproj src/businesslogic/
COPY src/dataaccess/dataaccess.csproj src/dataaccess/
COPY src/presentation/presentation.csproj src/presentation/
COPY src/tests/tests.csproj src/tests/
RUN dotnet restore "src/presentation/presentation.csproj"

# Stage 3: Build
FROM build-deps AS build
COPY src/ ./src/
RUN dotnet build "src/presentation/presentation.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# Stage 4: Publish
FROM build-deps AS publish
COPY src/ ./src/
RUN dotnet publish "src/presentation/presentation.csproj" \
    -c Release \
    -o /app/publish

# Stage 5: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published app
COPY --from=publish /app/publish .

# Set permissions
RUN chown -R appuser:appgroup /app

# Set environment variable for port
ENV APP_PORT=8080

# Switch to non-root user
USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "presentation.dll"]
