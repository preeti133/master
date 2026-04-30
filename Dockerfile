# ============================================
# RallyAPI Dockerfile - Multi-stage build
# ============================================

# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . .

RUN dotnet restore src/RallyAPI.Host/RallyAPI.Host.csproj

RUN dotnet publish src/RallyAPI.Host/RallyAPI.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RallyAPI.Host.dll"]