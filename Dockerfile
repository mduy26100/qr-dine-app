# Multi-stage build for .NET 8 API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/QRDine.API/QRDine.API.csproj", "QRDine.API/"]
COPY ["src/QRDine.Application/QRDine.Application.csproj", "QRDine.Application/"]
COPY ["src/QRDine.Application.Common/QRDine.Application.Common.csproj", "QRDine.Application.Common/"]
COPY ["src/QRDine.Domain/QRDine.Domain.csproj", "QRDine.Domain/"]
COPY ["src/QRDine.Infrastructure/QRDine.Infrastructure.csproj", "QRDine.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "QRDine.API/QRDine.API.csproj"

# Copy source code
COPY ["src/", "."]

# Build release
RUN dotnet build "QRDine.API/QRDine.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "QRDine.API/QRDine.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Install curl for health check AND libfontconfig1 for SkiaSharp
RUN apt-get update && apt-get install -y curl libfontconfig1 && rm -rf /var/lib/apt/lists/*

# Grant execute permissions for curl so non-root user can run healthcheck
RUN chmod +x /usr/bin/curl

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "QRDine.API.dll"]