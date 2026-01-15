# WineApi

A RESTful Web API built with ASP.NET Core 8.0 for managing a wine collection inventory system with mysql database. Features wine catalog management, bottle storage tracking, and user authentication with role-based access control.

## Features

- **Wine Management** - CRUD operations for wines with filtering by varietal, vineyard, and vintage
- **Bottle Tracking** - Track bottles with 3D bin coordinates (storage, bin X/Y, depth)
- **User Authentication** - Token-based authentication with 7-day expiration
- **Role-Based Access** - Admin-only endpoints for user management
- **Pagination & Filtering** - Built-in support for paginated queries with sorting

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: MySQL with Entity Framework Core 8.0 (Pomelo provider)
- **Documentation**: Swagger/OpenAPI
- **Testing**: NUnit, Moq, FluentAssertions

## Project Structure

```
WineApi/
├── src/WineApi/
│   ├── Controllers/       # API endpoints (Wine, Auth, User)
│   ├── Service/           # Business logic layer
│   ├── Data/              # EF Core DbContext and entities
│   ├── Model/             # DTOs and request/response models
│   ├── Extensions/        # Extension methods
│   ├── Filters/           # Paging and filtering action filters
│   ├── Middleware/        # Exception handling middleware
│   └── Migrations/        # EF Core migrations
├── tests/WineApi.Tests/
│   ├── Unit/              # Unit tests for controllers and services
│   ├── Integration/       # Integration tests with in-memory database
│   └── Fixtures/          # Test utilities and factories
└── .github/workflows/     # CI/CD pipelines
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- MySQL Server - you can run mysql locally via the docker compose file in src/WineApi/mysql

### Configuration

Configure the database connection in `appsettings.json`:

```json
{
  "DbOptions": {
    "Server": "localhost",
    "User": "your_user",
    "Password": "your_password",
    "Database": "wine_db"
  }
}
```

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/WineApi

# Run tests
dotnet test
```

### Database Migrations

This project uses Entity Framework Core for database migrations. Install the EF Core tools globally if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

Run migrations from the solution root:

```bash
# Apply all pending migrations
dotnet ef database update --project src/WineApi

# Create a new migration
dotnet ef migrations add <MigrationName> --project src/WineApi

# Remove the last migration (if not applied)
dotnet ef migrations remove --project src/WineApi

# Generate SQL script for migrations
dotnet ef migrations script --project src/WineApi

# Generate Idempotent SQL script that will only apply migrations not already run
dotnet ef migrations script --project src/WineApi --idempotent
```

### Running Tests

```bash
# All tests
dotnet test

# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Specific test class
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```
