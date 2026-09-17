# PLAN-000: Foundation & Core Infrastructure Scaffolding

## Summary
Establish the foundational monorepo structure, .NET 8 Clean Architecture backend solution, Angular 20 frontend with Tailwind CSS, container orchestration via Docker Compose, and CI pipeline via GitHub Actions.

## Architecture Impact
- Monorepo directory layout:
  - `/backend`: .NET 8 Web API solution with Clean Architecture layers.
  - `/frontend`: Angular 20 standalone client with Tailwind CSS and Angular CDK.
  - Root: Docker Compose, GitHub Actions, specs, plans, reviews, documentation.

## Backend Files
- `backend/AgenticBoard.sln`
- `backend/AgenticBoard.Domain/AgenticBoard.Domain.csproj`
  - Core Entities: `User`, `Project`, `ProjectMember`, `TaskItem`, `TaskComment`, `AuditLog`.
  - Enums: `ProjectRole`, `TaskStatus`, `TaskPriority`.
- `backend/AgenticBoard.Application/AgenticBoard.Application.csproj`
  - Common interfaces: `IApplicationDbContext`, `IJwtTokenGenerator`, `IPasswordHasher`, `ICurrentUserService`, `IDateTimeProvider`.
  - Common models, exceptions (`ValidationException`, `NotFoundException`, `ForbiddenException`, `ConflictException`).
- `backend/AgenticBoard.Infrastructure/AgenticBoard.Infrastructure.csproj`
  - EF Core `ApplicationDbContext` with Fluent API configurations, indexes, and cascades.
  - Services: `BcryptPasswordHasher` (or PBKDF2), `JwtTokenGenerator`, `SystemDateTimeProvider`.
- `backend/AgenticBoard.Api/AgenticBoard.Api.csproj`
  - `Program.cs`: Service registrations, JWT Authentication/Authorization, Swagger/OpenAPI with Bearer scheme, Global `ExceptionHandlingMiddleware` mapping to `ProblemDetails`, Health checks, CORS.
  - `appsettings.json` and `appsettings.Development.json`.
- `backend/AgenticBoard.Tests/AgenticBoard.Tests.csproj`
  - xUnit test project with FluentAssertions / Moq / Microsoft.AspNetCore.Mvc.Testing / EF Core In-Memory or SQLite for isolated fast unit/integration tests.
- `backend/Dockerfile`

## Frontend Files
- `frontend/package.json` (Angular 20, TypeScript, Tailwind CSS, `@angular/cdk`)
- `frontend/src/app/app.config.ts` (ProvideRouter, ProvideHttpClient with interceptors)
- `frontend/src/app/app.routes.ts`
- `frontend/src/app/app.component.ts` & template
- `frontend/Dockerfile` & `frontend/nginx.conf`

## Database Changes
- Initial database configuration in EF Core with SQL Server provider (and SQLite/InMemory for lightweight testing).

## DevOps
- `docker-compose.yml`: Services `sqlserver`, `backend`, `frontend`.
- `.github/workflows/ci.yml`: Automated build and test jobs for backend and frontend.

## Implementation Order
1. Generate .NET solution and projects targeting `net8.0`.
2. Add necessary NuGet packages (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation`, `Swashbuckle.AspNetCore`, etc.).
3. Generate Angular 20 project in `frontend/` with routing, standalone components, and Tailwind CSS.
4. Add Dockerfiles and `docker-compose.yml`.
5. Add `.github/workflows/ci.yml`.
6. Verify backend builds (`dotnet build backend/AgenticBoard.sln`) and tests run.
7. Conduct AI Code Review for Phase 1.
