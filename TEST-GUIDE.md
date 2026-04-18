# Articles API Testing Guide (Get Article by Title)

This guide explains how to test the protected endpoint:

- GET /api/articles/title/{title}

The endpoint requires a valid JWT token.

## Auth Defaults

- Username: admin
- Password: password123
- Token expiration: 60 minutes (configurable)

## Testing Options

### 1. Browser Test Page

File: frontend-test.html

Steps:

1. Run the API.
2. Open frontend-test.html.
3. Click connection test.
4. Login and obtain token.
5. Search by article title.

Key capabilities:

- User-friendly interface
- Automatic token handling
- Clear error feedback

### 2. PowerShell Script

File: test-article-by-title.ps1

Basic usage:

```powershell
.\test-article-by-title.ps1
```

Custom title:

```powershell
.\test-article-by-title.ps1 -ArticleTitle "My Article"
```

Custom base URL:

```powershell
.\test-article-by-title.ps1 -BaseUrl "http://localhost:5212" -ArticleTitle "Test"
```

### 3. HTTP Requests File

File: test-article-by-title.http

Workflow:

1. Run login request and copy token.
2. Replace token placeholder.
3. Run protected endpoint request.

## Run the API

```bash
dotnet run --project Articles.Api
```

## Expected Responses

### Successful Login

```json
{
  "token": "...",
  "username": "admin",
  "expiresAt": "2026-01-01T12:00:00Z"
}
```

### Successful Article Response

```json
{
  "id": 1,
  "title": "Sample Article",
  "description": "Sample description",
  "content": "Sample content",
  "author": "John Doe",
  "isPublished": true,
  "viewCount": 5
}
```

### Common Errors

Unauthorized:

```json
{
  "error": "Unauthorized"
}
```

Not found:

```json
{
  "error": "Article not found"
}
```

## Troubleshooting

- Ensure API is running on [http://localhost:5212](http://localhost:5212).
- Re-login if token expired.
- Use exact title when searching.

## Endpoint Summary

| Method | Endpoint | Auth Required | Purpose |
| ------ | -------- | ------------- | ------- |
| POST | /api/auth/login | No | Get JWT token |
| GET | /api/articles/title/{title} | Yes | Get article by title |
| GET | /api/articles | No | List articles |
| GET | /health | No | Health check |

## Security Notes

- Replace demo credentials in production.
- Use HTTPS in production.
- Keep JWT secret out of source control.
