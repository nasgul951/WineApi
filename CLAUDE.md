# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build & run
dotnet restore
dotnet build
dotnet run --project src/WineApi

# Tests
dotnet test
dotnet test --logger "console;verbosity=detailed"
dotnet test --filter "FullyQualifiedName~AuthControllerTests"   # specific class
dotnet test --filter "FullyQualifiedName~WineApi.Tests.Unit"    # namespace subset

# EF Core migrations (run from solution root)
dotnet ef database update --project src/WineApi
dotnet ef migrations add <Name> --project src/WineApi
dotnet ef migrations script --project src/WineApi --idempotent

# Local MySQL (Docker)
cd src/WineApi/mysql && docker-compose up -d
```

## Architecture

3-layer ASP.NET Core 8.0 REST API:

- **Controllers** (`src/WineApi/Controllers/`) — thin; delegate entirely to services. Three controllers: `AuthController`, `WineController`, `UserController`.
- **Services** (`src/WineApi/Service/`) — all business logic. Injected via interfaces (`IAuthService`, `IWineService`, `IUserService`).
- **Data** (`src/WineApi/Data/`) — EF Core `WineContext` with four entities: `Wine`, `Bottle`, `Storage`, `User`. MySQL via Pomelo provider.

Supporting pieces:
- **Model/** — DTOs/request-response models (separate from EF entities).
- **Filters/** — `[UsePaging]` and `[UseFiltering]` action filters that automatically apply pagination/sorting/filtering to controller action results.
- **Middleware/** — `ExceptionMiddleware` converts custom exception types to HTTP status codes (centralized error handling).
- **Helpers/** — `TokenAuthenticationHandler` (custom bearer token auth) and crypto helpers.
- **Extensions/** — LINQ extensions including `IfThenWhere` for composable optional filtering and `ApplyJsonFilter` for reflection-based dynamic filtering.

## Key Patterns

- **Custom auth**: Bearer tokens are validated by `TokenAuthenticationHandler`; role constants are in `Constants/`. Admin-only endpoints use `[Authorize(Roles = ...)]`.
- **Paging**: Decorate controller actions with `[UsePaging]` attributes the action filter intercepts the result and apply the query parameters automatically.
- **Conditional LINQ**: Use the `IfThenWhere` extension (in `Extensions/`) rather than building nullable conditional chains manually.
- **Test fixtures**: `WineContextFixture` provides an in-memory EF Core database. `TestDataBuilder` constructs test entities. `WineApiFactory` is a `WebApplicationFactory` for integration tests.

## Configuration

Database connection lives in `appsettings.json` under `DbOptions` (`Server`, `User`, `Password`, `Database`).
