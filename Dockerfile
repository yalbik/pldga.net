# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY PLDGA.sln .
COPY src/PLDGA.Domain/PLDGA.Domain.csproj src/PLDGA.Domain/
COPY src/PLDGA.Application/PLDGA.Application.csproj src/PLDGA.Application/
COPY src/PLDGA.Infrastructure/PLDGA.Infrastructure.csproj src/PLDGA.Infrastructure/
COPY src/PLDGA.Web/PLDGA.Web.csproj src/PLDGA.Web/
COPY tests/PLDGA.Tests/PLDGA.Tests.csproj tests/PLDGA.Tests/

RUN dotnet restore

# Copy all source and build
COPY . .
RUN dotnet test tests/PLDGA.Tests/PLDGA.Tests.csproj -c Release --no-restore
RUN dotnet publish src/PLDGA.Web/PLDGA.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Create data directory for JSON persistence
# Azure App Service mounts persistent storage at /home
RUN mkdir -p /home/App_Data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV APP_DATA_PATH=/home/App_Data

HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "PLDGA.Web.dll"]
