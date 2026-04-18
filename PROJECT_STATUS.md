# Project Status (CV-Ready Snapshot)

## Summary

The project is now in a strong portfolio state:

- Upgraded to .NET 10 across all projects.
- Security/stability baseline implemented (JWT, CORS, exception hardening).
- Dependency vulnerabilities remediated.
- CI pipeline added for build/test automation.
- Controller and security-focused tests added.

## Completed Areas

### Platform and Architecture

- .NET 10 solution-wide upgrade completed.
- Layered architecture remains clean and modular.
- Dockerfile updated to .NET 10 runtime/sdk images.

### Security and API Hardening

- JWT secret supports environment override (`JWT_SECRET_KEY`).
- Config-backed demo users replace hardcoded credentials.
- Restricted CORS policy with explicit allowed origins.
- Internal exception details are not exposed in API responses.

### Engineering Quality

- CI workflow available at `.github/workflows/dotnet-ci.yml`.
- Root `.editorconfig` added for style consistency.
- Unit and controller tests are passing.

## Validation Commands

```bash
dotnet restore Articles_API_Task_01.sln
dotnet build Articles_API_Task_01.sln
dotnet test Articles_API_Task_01.sln
dotnet list Articles_API_Task_01.sln package --vulnerable
```

## Known Tradeoffs

- Authentication remains demo-oriented, not full production identity.
- Integration tests with real SQL Server are not yet included.

## Optional Next Milestone

1. Add ASP.NET Identity and persistent users.
2. Add integration tests with a test SQL Server.
3. Add coverage reporting badge in CI.
