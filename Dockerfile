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
RUN dotnet publish src/PLDGA.Web/PLDGA.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 10420

# Create data directory for JSON persistence
RUN mkdir -p /app/App_Data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10420
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:10420/ || exit 1

ENTRYPOINT ["dotnet", "PLDGA.Web.dll"]
