# Copilot Instructions for C# Web API Project

## General Coding Guidelines
- Use **C# 12** syntax and features where appropriate.
- Follow **.NET 8** coding conventions and naming standards.
- Always include **XML documentation comments** for public classes, methods, and properties.
- Use **async/await** for all I/O-bound operations.
- Avoid blocking calls (`.Result`, `.Wait()`).
- Prefer **dependency injection** over static classes or service locators.
- Ensure **nullability** is enabled (`<Nullable>enable</Nullable>` in `.csproj`).

## Project Architecture
- Follow **Clean Architecture** principles:
  - **Domain**: Entities, Value Objects, Interfaces.
  - **Application**: Use Cases, DTOs, Service Interfaces.
  - **Infrastructure**: EF Core, external services, persistence.
  - **API**: Controllers, Filters, Middleware.
- Keep controllers **thin** — delegate business logic to services.
- Use **DTOs** for request/response models; never expose EF entities directly.

## API Development Rules
- Use **attribute routing** (`[HttpGet]`, `[HttpPost]`, etc.).
- Always validate input models using **FluentValidation** or `DataAnnotations`.
- Return appropriate **HTTP status codes**:
  - `200 OK` for successful GET/PUT/DELETE.
  - `201 Created` for successful POST with location header.
  - `400 Bad Request` for validation errors.
  - `404 Not Found` when resource is missing.
  - `500 Internal Server Error` for unhandled exceptions.
- Use **ProblemDetails** for error responses.

## Security
- Always use **JWT Bearer Authentication** for protected endpoints.
- Never log sensitive data (passwords, tokens, PII).
- Use **HTTPS** in all environments.
- Sanitize all user inputs before processing.

## Entity Framework Core
- Use **Migrations** for schema changes.
- Use **AsNoTracking()** for read-only queries.
- Avoid N+1 queries — use `.Include()` or projection.
- Use **CancellationToken** in async DB calls.

## Testing
- Write **unit tests** for services and controllers.
- Use **xUnit** and **Moq** for mocking dependencies.
- Ensure at least **80% code coverage**.

## Example Patterns
- **Controller Example**:
  ```csharp
  [ApiController]
  [Route("[controller]")]
  public class ProductsController : ControllerBase
  {
      private readonly IProductService _service;

      public ProductsController(IProductService service)
      {
          _service = service;
      }

      [HttpGet("{id:int}")]
      public async Task<IActionResult> GetProduct(int id, CancellationToken ct)
      {
          var product = await _service.GetByIdAsync(id, ct);
          return product is null ? NotFound() : Ok(product);
      }
  }
