# Parkland Disc Golf Association (PLDGA)

A full-featured web application for managing the Parkland Disc Golf Association, built with ASP.NET Core MVC and Clean Architecture.

## Features

- **Member Management** — Track members with paid/unpaid status, quick payment processing, and bulk payment operations
- **Event Tracking** — Create and manage disc golf events with registration, results recording, and points calculation
- **Season Leaderboards** — Real-time leaderboard with drill-down to individual player stats and event breakdowns
- **Community Polls** — Create polls, vote, and view results with auto-close on expiration
- **News Articles** — Submit articles with image uploads and admin approval workflow
- **Admin Dashboard** — Comprehensive dashboard with statistics, site settings, and management tools
- **Annual Dues** — Configurable dues management (default $30/year) with payment tracking
- **Site Settings** — Fully configurable settings for membership, events, leaderboard, polls, news, and appearance

## Technology Stack

- **.NET 8.0** — ASP.NET Core MVC
- **Clean Architecture** — Domain, Application, Infrastructure, and Web layers
- **JSON File Persistence** — Thread-safe with atomic writes (no database required)
- **NUnit + Moq** — Unit testing with 80%+ coverage target
- **Bootstrap 5** — Responsive modern UI with custom disc golf theme
- **Docker** — Multi-stage build, containerized deployment on port 10420

## Project Structure

```
PLDGA.sln
├── src/
│   ├── PLDGA.Domain/          # Entities, repository interfaces
│   ├── PLDGA.Application/     # Services, DTOs, service interfaces
│   ├── PLDGA.Infrastructure/  # JSON file repositories, DI registration
│   └── PLDGA.Web/             # ASP.NET Core MVC (controllers, views, static assets)
├── tests/
│   └── PLDGA.Tests/           # NUnit tests (Domain, Application, Infrastructure)
├── Dockerfile
└── README.md
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Docker and Docker Compose (optional, for containerized deployment)
- A modern web browser

## Building the Project

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build PLDGA.sln

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Running the Application

### Development

```bash
cd src/PLDGA.Web
dotnet run
```

The application will start at `http://localhost:10420`.

### Default Admin Account

- **Username:** `admin`
- **Password:** `Admin123!`

> Change the default admin password after first login.

## Docker Deployment

### Build the Docker Image

```bash
docker build -t pldga-app .
```

### Run the Container

```bash
docker run -d -p 10420:10420 --name pldga pldga-app
```

### With Data Persistence (Volume Mount)

```bash
docker run -d -p 10420:10420 -v pldga-data:/app/App_Data --name pldga pldga-app
```

### Docker Compose

```yaml
version: '3.8'
services:
  pldga:
    build: .
    ports:
      - "10420:10420"
    volumes:
      - pldga-data:/app/App_Data
    restart: unless-stopped

volumes:
  pldga-data:
```

```bash
docker compose up -d
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test category
dotnet test --filter "FullyQualifiedName~Application"
dotnet test --filter "FullyQualifiedName~Domain"
dotnet test --filter "FullyQualifiedName~Infrastructure"
```

## Data Files

The application stores data as JSON files in the `App_Data/` directory:

| File | Description |
|------|-------------|
| `members.json` | Member records |
| `events.json` | Event data and results |
| `polls.json` | Polls, answers, and votes |
| `news_articles.json` | News articles |
| `seasons.json` | Season definitions |
| `users.json` | User account credentials |
| `site_settings.json` | Site configuration |

**Backup:** Regularly back up the `App_Data/` directory to prevent data loss.

## Admin Features

1. Navigate to `/Admin` after logging in with an admin account
2. **Member Management** — Add/edit members, toggle payment status, bulk mark as paid
3. **Event Management** — Create events, record results, complete events
4. **News Approval** — Review and approve/reject submitted articles
5. **Site Settings** — Configure all aspects of the application at `/Admin/Settings`
6. **Recalculate Points** — Rebuild season leaderboard from event results

## Configuration

Application settings are in `src/PLDGA.Web/appsettings.json`. Key settings:

- **Kestrel endpoint** — Port 10420 (configurable)
- **Annual dues** — Default $30/year (configurable via Admin > Settings)
- **Placement points** — 1st=100, 2nd=90, ..., 20th=31 (configurable)

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 10420 in use | Change port in `appsettings.json` or use `-p` flag in Docker |
| JSON file corruption | Restore from backup; atomic writes prevent most corruption |
| Login fails | Use default admin credentials or check `users.json` |
| Missing data after restart | Ensure Docker volume is mounted for `App_Data/` |

Logs are written to the console by default. In Docker, view with `docker logs pldga`.

## License

This project is licensed under the **GNU General Public License v3.0** (GPL-3.0). See [LICENSE](LICENSE) for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request
