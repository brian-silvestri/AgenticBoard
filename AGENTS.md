# AGENTS.md — Rules and Governance for Coding Agents

This document defines the strict engineering standards, security rules, and operating procedures that any coding agent (or human engineer) must follow when contributing to **AgenticBoard**.

---

## 1. Operating Philosophy

AI does not replace engineering judgment. Software development in this repository is **spec-driven** and **quality-first**.

> **Cardinal Rule:**
> **Never consider generated code correct only because it compiles.**

Compiling code is merely a syntax check. True correctness requires architectural integrity, secure domain invariants, strict input validation, comprehensive automated tests, and clear error semantics.

---

## 2. Feature Workflow Lifecycle

For every feature or architectural change, adhere strictly to this lifecycle:

1. **Read the relevant specification:** Review or draft the specification under `/specs`. Understand the business rules, user stories, acceptance criteria, and edge cases.
2. **Inspect the current architecture:** Review existing domain models, interfaces, application handlers, and frontend components before writing new code.
3. **Understand existing patterns:** Follow established patterns in Clean Architecture, DTO mapping, error handling (`ProblemDetails`), and Angular Signals.
4. **Produce or update the implementation plan:** Create the plan under `/plans/<feature>-plan.md` detailing affected files, DB migrations, API contracts, tests, and security considerations.
5. **Identify affected files & interfaces:** Clearly demarcate boundaries between Domain, Application, Infrastructure, API, and Frontend.
6. **Identify security and data integrity risks:** Evaluate authentication, tenant/membership authorization, SQL injection, overposting, and race conditions.
7. **Implement the smallest coherent change:** Write clean, readable, modular code adhering to the single responsibility principle.
8. **Run backend tests:** Execute unit and integration tests (`dotnet test`).
9. **Run frontend tests:** Run Angular unit tests (`npm test` / headless test runner).
10. **Build backend:** Verify clean compilation (`dotnet build`).
11. **Build frontend:** Verify clean production build (`npm run build`).
12. **Review generated code:** Conduct a formal AI/Agent Code Review documented under `/reviews/<feature>-review.md`.
13. **Review error handling:** Verify consistent HTTP status codes (400, 401, 403, 404, 409, 500) and structured `ProblemDetails` responses.
14. **Review authorization:** Ensure all queries and mutations check project ownership and membership in the backend. Never rely on frontend guards alone.
15. **Review edge cases:** Concurrency, empty lists, boundary values, invalid foreign keys, and soft deletes.
16. **Update documentation:** Update `README.md`, specs, and plans if behavior or endpoints evolved.

---

## 3. Strict Prohibitions & Invariants

- **Never expose secrets:** Never hardcode passwords, API keys, tokens, or private certificates in code or configuration files.
- **Never commit credentials:** Keep connection strings and development secrets in environment variables or user secrets (`appsettings.Development.json` must contain placeholders or local development defaults only).
- **Don't bypass tests to make CI pass:** Never disable, comment out, or weaken test assertions simply to satisfy test runners or CI pipelines.
- **Don't remove validation without justification:** Validation rules at the API (FluentValidation) and Domain levels protect system integrity.
- **Don't weaken authorization rules:** Every project-scoped resource (Tasks, Comments, Activity) must strictly verify that the requesting user is an active member or owner of the associated project.
- **Inspect surrounding code before changing architecture:** Prefer local idioms and established project conventions over introducing foreign libraries or paradigms.
- **Avoid unnecessary abstractions:** Do not introduce generic repositories, complex CQRS event sourcing, or microservice scaffolding when Clean Architecture with direct, testable handlers suffices.

---

## 4. Engineering Standards

### Backend (.NET 8 / ASP.NET Core)
- Target: `.NET 8` (`net8.0`).
- Layers:
  - `AgenticBoard.Domain`: Pure domain models, enums, domain exceptions. No external dependencies.
  - `AgenticBoard.Application`: Use cases, DTOs, interfaces, validation (FluentValidation), business logic.
  - `AgenticBoard.Infrastructure`: EF Core `DbContext`, migrations, repositories, password hashing (`BCrypt` or `IPasswordHasher`), JWT token generation, logging, system clock.
  - `AgenticBoard.Api`: Controllers/endpoints, middleware (Global Exception / ProblemDetails), Swagger/OpenAPI, Dependency Injection configuration.
  - `AgenticBoard.Tests`: Automated unit and integration tests verifying domain invariants, authorization boundaries, and API workflows.
- Password Security: Industry-standard cryptographically secure password hashing (PBKDF2/BCrypt/Argon2). Never store plaintext passwords.
- Responses: RFC 7807 `ProblemDetails` for errors.

### Frontend (Angular 20 / TypeScript / Tailwind CSS)
- Framework: Angular 20 Standalone Components.
- State & Reactivity: Angular Signals (`signal`, `computed`, `effect`) and RxJS where appropriate (e.g. `HttpClient`).
- Styling: Modern Tailwind CSS design system. Professional, high-contrast, polished UI with dark/light harmony, cards, status badges, and loading skeletons.
- Security: JWT HTTP Interceptor, Route Guards (`AuthGuard`), XSS prevention.

### Database & Migrations (SQL Server)
- Relational integrity with foreign keys, composite indexes, and cascading behavior where appropriate.
- Audit fields: `CreatedAt`, `UpdatedAt`, `CreatedBy`.
- Dedicated `AuditLog` entity capturing entity changes (`TaskCreated`, `StatusChanged`, `PriorityChanged`, `AssignedUserChanged`, `CommentAdded`, etc.).
