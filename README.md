# Articles API

A .NET 8 Web API project implementing Clean Architecture (Onion Architecture) for managing articles with filtering and pagination.

## 🏗️ Architecture

This project follows **Onion Architecture** with the following layers:

- **Core Layer**: Domain models, business logic, and abstractions
- **Infrastructure Layer**: Data access and external services
- **Presentation Layer**: API controllers and DTOs
- **Shared Layer**: Common models and utilities

## 🚀 Features

- ✅ **Article Management**: CRUD operations for articles
- ✅ **Pagination**: Built-in pagination support (max 50 items per page)
- ✅ **Filtering**: Filter articles by tags and published status
- ✅ **AutoMapper**: DTO mapping for clean API responses
- ✅ **Entity Framework**: SQL Server database with migrations
- ✅ **Swagger**: Interactive API documentation
- ✅ **Unit Tests**: TDD approach with xUnit, Moq, and FluentAssertions
- ✅ **Clean Architecture**: Separation of concerns with dependency injection

## 📋 API Endpoints

### Articles
- `GET /api/articles` - Get all articles with pagination
- `GET /api/articles/{id}` - Get article by ID
- `GET /api/articles/title/{title}` - Get article by title
- `GET /api/articles/tag/{tag}` - Get articles by tag with pagination
- `GET /api/articles/published` - Get published articles with pagination

### Query Parameters
- `pageNumber` (default: 1) - Page number
- `pageSize` (default: 10, max: 50) - Items per page

## 🛠️ Technology Stack

- **.NET 8**
- **Entity Framework Core 8.0**
- **SQL Server**
- **AutoMapper**
- **xUnit** (Testing)
- **Moq** (Mocking)
- **FluentAssertions** (Assertions)

## 📦 Project Structure

```
Articles_API_Task_01/
├── Articles.Api/                 # Web API project
│   ├── Controllers/             # API controllers
│   ├── Mapping/                 # AutoMapper profiles
│   └── Program.cs              # Application startup
├── Core/                        # Core business logic
│   ├── Domain/                 # Domain models
│   ├── Services/               # Business services
│   └── Services.Abstraction/   # Service interfaces
├── Infrastructure/              # External concerns
│   ├── Persistence/            # Data access layer
│   └── Presentation/           # Presentation layer
├── Shared/                      # Shared components
│   ├── DTOs/                   # Data transfer objects
│   └── Models/                 # Shared models
└── Articles.Tests/              # Unit tests
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Articles_API_Task_01
   ```

2. **Update connection string**
   Edit `Articles.Api/appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update --project Infrastructure/Persistence --startup-project Articles.Api
   ```

4. **Run the application**
   ```bash
   dotnet run --project Articles.Api
   ```

5. **Access Swagger UI**
   Open your browser and navigate to: `http://localhost:5212/swagger`

### Running Tests
```bash
dotnet test
```

## 📊 Database Schema

### Articles Table
- `Id` (int, PK) - Primary key
- `Title` (nvarchar(200)) - Article title (unique)
- `Description` (nvarchar(500)) - Article description
- `Content` (nvarchar(max)) - Article content
- `Tags` (nvarchar(500)) - Article tags
- `Author` (nvarchar(100)) - Article author
- `CreatedAt` (datetime2) - Creation date
- `UpdatedAt` (datetime2) - Last update date
- `IsPublished` (bit) - Publication status
- `ViewCount` (int) - View count

### Indexes
- `IX_Articles_Title` (Unique) - For fast title lookups
- `IX_Articles_Tags` - For tag filtering
- `IX_Articles_IsPublished` - For published articles filtering

## 🧪 Testing

The project includes comprehensive unit tests covering:
- Article service operations
- Pagination functionality
- Repository patterns
- Business logic validation

Run tests with:
```bash
dotnet test
```

## 🔧 Configuration

### Pagination
- Default page size: 10
- Maximum page size: 50
- Configurable in `Shared/Models/PaginationParameters.cs`

### Database
- SQL Server with Entity Framework Core
- Connection string in `appsettings.json`
- Migrations in `Infrastructure/Persistence/Migrations/`

## 📝 API Examples

### Get all articles (page 1, 10 items)
```http
GET /api/articles?pageNumber=1&pageSize=10
```

### Get articles by tag
```http
GET /api/articles/tag/technology?pageNumber=1&pageSize=5
```

### Get published articles
```http
GET /api/articles/published?pageNumber=1&pageSize=20
```

## 🎯 Clean Architecture Benefits

- **Testability**: Easy to unit test with dependency injection
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Easy to change data access layer (SQL Server, PostgreSQL, Oracle)
- **Scalability**: Modular design supports growth
- **Independence**: Core business logic independent of external concerns

## 📄 License

This project is created for educational purposes. 