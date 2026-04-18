# Frontend Test Guide for Articles API

## Overview

The repository includes a lightweight frontend/testing harness for quick demos:

- `frontend-test.html`: Interactive UI for auth + article operations.
- `api-test.js`: Scripted testing helper (browser console / JS runtime).

## Demo Flow

1. Start API.
2. Open `frontend-test.html`.
3. Login with configured demo credentials.
4. Query article endpoints and show protected access behavior.

## Run API

```bash
dotnet run --project Articles.Api
```

## Default Demo Credentials

- Username: `admin`
- Password: `password123`

## Script Usage

```javascript
const tester = new ArticlesAPITester();
await tester.testConnection();
await tester.login("admin", "password123");
await tester.runComprehensiveTests();
```

## Interview Tips

1. Start from Swagger to show API contract quality.
2. Show frontend harness for practical usage flow.
3. Demonstrate unauthorized vs authorized behavior.
4. Mention CI and automated tests as quality signals.
