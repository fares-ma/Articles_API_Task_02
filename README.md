# Articles API

![dotnet](https://img.shields.io/badge/.NET-10-512BD4)
![tests](https://img.shields.io/badge/tests-40%20passing-brightgreen)
![architecture](https://img.shields.io/badge/architecture-clean%20onion-blue)

A .NET 10 Web API built with Clean Architecture (Onion Architecture) for managing articles and newspapers with pagination, validation, and JWT-based security.

## CV Highlights

- Migrated a multi-project solution from .NET 8 to .NET 10.
- Hardened API security with JWT, restricted CORS, and centralized exception handling.
- Added CI quality gate for restore/build/test via GitHub Actions.
- Expanded coverage with service and controller tests.
- Cleaned dependency baseline and improved maintainability.

## Architecture

The solution follows Onion Architecture with clear separation of concerns:

- Core: domain models, business services, abstractions.
- Infrastructure: persistence and repository implementations.
- Presentation: API layer with controllers and middleware.
- Shared: DTOs and shared models.

## Technology Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10 (SQL Server)
- JWT Authentication
- AutoMapper
- Serilog
- xUnit, Moq, FluentAssertions

## Run Locally

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or SQL Server Express)

### Setup

1. Configure connection string in Articles.Api/appsettings.json.
2. Apply migrations.
3. Run the API.

### Commands

```bash
dotnet restore Articles_API_Task_01.sln
dotnet build Articles_API_Task_01.sln
dotnet ef database update --project Infrastructure/Persistence --startup-project Articles.Api
dotnet run --project Articles.Api
```

### Default Local URLs

- HTTP: [http://localhost:5000](http://localhost:5000)
- HTTPS: [https://localhost:7232](https://localhost:7232)
- Swagger UI: [https://localhost:7232/swagger](https://localhost:7232/swagger) or [http://localhost:5000/swagger](http://localhost:5000/swagger)

## API Coverage

Authentication endpoints:

- POST /api/auth/login
- POST /api/auth/validate (requires auth)
- POST /api/auth/validate-header

Article endpoints:

- GET /api/articles
- GET /api/articles/{id} (requires auth)
- GET /api/articles/title/{title} (requires auth)
- GET /api/articles/tag/{tag} (requires auth)
- GET /api/articles/published (requires auth)
- GET /api/articles/newspaper/{newspaperId} (requires auth)
- POST /api/articles (requires auth)
- PUT /api/articles/{id} (requires auth)
- DELETE /api/articles/{id} (requires auth)

Newspaper endpoints:

- GET /api/newspapers
- GET /api/newspapers/all
- GET /api/newspapers/active
- GET /api/newspapers/{id}
- GET /api/newspapers/name/{name}
- POST /api/newspapers (requires auth)
- PUT /api/newspapers/{id} (requires auth)
- DELETE /api/newspapers/{id} (requires auth)

Health endpoint:

- GET /health

Pagination query parameters:

- pageNumber (default: 1)
- pageSize (default: 10, max: 50)

## Authentication Flow

1. Call POST /api/auth/login with username and password.
2. Copy the returned JWT token.
3. Send token in Authorization header for protected routes.

```http
Authorization: Bearer YOUR_TOKEN
```

## Storage Provider Behavior

The application supports selecting article storage via configuration:

- StorageSettings:UseS3 = false: SQL Server article service.
- StorageSettings:UseS3 = true: S3-backed article service.

## Quality Gates

- CI pipeline: .github/workflows/dotnet-ci.yml
- Local validation:

```bash
dotnet restore Articles_API_Task_01.sln
dotnet build Articles_API_Task_01.sln
dotnet test Articles_API_Task_01.sln
```

## Production Notes

- Replace demo credentials before production usage.
- Store JWT secret in environment variable (JWT_SECRET_KEY).
- Tighten CORS origins to trusted clients only.
- Use HTTPS and secure secret management.

## Deploy to ASPMonster (Visual Studio)

1. Right click the API project and select Publish.
2. Create or select a Web Deploy profile from your hosting panel.
3. Set Configuration to Release and Target Runtime to Portable.
4. Publish.

After publishing, verify these URLs:

- Root: [http://faresarticles.runasp.net/](http://faresarticles.runasp.net/)
- Health: [http://faresarticles.runasp.net/health](http://faresarticles.runasp.net/health)
- Swagger: [http://faresarticles.runasp.net/swagger](http://faresarticles.runasp.net/swagger)

If Swagger does not open, republish latest code and recycle the app pool from hosting panel.

## Project Structure

```text
Articles_API_Task_01/
+-- Articles.Api/
+-- Core/
+-- Infrastructure/
+-- Shared/
+-- Articles.Tests/
```

## License

This project is for educational and portfolio purposes.
