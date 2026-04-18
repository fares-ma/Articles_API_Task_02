# Troubleshooting Guide - Articles API

## Baseline Health Check

```bash
dotnet restore Articles_API_Task_01.sln
dotnet build Articles_API_Task_01.sln
dotnet test Articles_API_Task_01.sln
```

If these pass, the environment is typically healthy.

## Common Problems

### Missing JWT Secret at Startup

Symptom:

- App fails on startup with JWT configuration error.

Fix:

1. Set `JWT_SECRET_KEY` environment variable.
2. Or provide a valid development secret in appsettings.

### CORS Errors in Browser

Symptom:

- Browser blocks API requests due to CORS policy.

Fix:

1. Add your frontend origin to `Cors:AllowedOrigins`.
2. Restart the API.

### 401 on Login or Protected Endpoints

Symptom:

- Authentication requests return Unauthorized.

Fix:

1. Verify `Auth:DemoUsers` credentials in configuration.
2. Use `Authorization: Bearer <token>` format for protected routes.

### SQL Server Connection/Migration Issues

Symptom:

- DB connection or migration fails at startup.

Fix:

1. Verify `ConnectionStrings:DefaultConnection`.
2. Ensure SQL Server instance is running and reachable.
3. Run migration manually:

```bash
dotnet ef database update --project Infrastructure/Persistence --startup-project Articles.Api
```

### Swagger Not Available

Symptom:

- `/swagger` does not load.

Fix:

1. Confirm app is running in Development.
2. Check launch URL from startup output.
