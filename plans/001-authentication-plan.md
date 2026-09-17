# PLAN-001: Authentication & User Identity Implementation

## Summary
Implement end-to-end authentication for AgenticBoard: domain entity `User`, repository and EF Core data mapping, PBKDF2/BCrypt password hashing, JWT token generation and verification, centralized RFC 7807 `ProblemDetails` error handling, Angular auth state with Signals, JWT interceptor, auth route guards, and responsive login/register interfaces.

## Architecture Impact

### Domain (`AgenticBoard.Domain`)
- `Entities/User.cs`: Core user model with `Id`, `Email`, `FullName`, `PasswordHash`, `CreatedAt`, `UpdatedAt`.
- `Common/BaseEntity.cs`: Common timestamped base entity.

### Application (`AgenticBoard.Application`)
- `Common/Interfaces/IApplicationDbContext.cs`: EF Core database abstraction.
- `Common/Interfaces/IPasswordHasher.cs`: Cryptographic password hashing contract.
- `Common/Interfaces/IJwtTokenGenerator.cs`: JWT generation contract.
- `Common/Interfaces/ICurrentUserService.cs`: Authenticated identity accessor.
- `Common/Interfaces/IDateTimeProvider.cs`: System clock abstraction.
- `Common/Exceptions/`: `ValidationException`, `NotFoundException`, `ConflictException`, `ForbiddenException`, `UnauthorizedException`.
- `Features/Auth/Dtos/`: `RegisterDto`, `LoginDto`, `AuthResponseDto`, `UserDto`.
- `Features/Auth/Validators/`: `RegisterDtoValidator`, `LoginDtoValidator`.
- `Features/Auth/Services/IAuthService.cs` & `AuthService.cs`: Application service executing business logic.

### Infrastructure (`AgenticBoard.Infrastructure`)
- `Persistence/ApplicationDbContext.cs`: EF Core DbContext with User entity configuration.
- `Persistence/Configurations/UserConfiguration.cs`: Table schema, column lengths, unique lowercase email index.
- `Services/BcryptPasswordHasher.cs`: Secure salted password hashing via BCrypt.
- `Services/JwtTokenGenerator.cs`: HMAC-SHA256 JWT generator.
- `Services/DateTimeProvider.cs`: UTC time provider.
- `Services/CurrentUserService.cs`: Extracts claims from `IHttpContextAccessor`.
- `Persistence/DbInitializer.cs`: Development seeding (`demo@agenticboard.local`).

### API (`AgenticBoard.Api`)
- `Controllers/AuthController.cs`: Endpoints `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`.
- `Middleware/ExceptionHandlingMiddleware.cs`: Standardized RFC 7807 `ProblemDetails`.
- `Program.cs`: Wire JWT Bearer authentication, Swagger JWT Security Definition, CORS, and dependency injection.

### Frontend (`frontend/src/app`)
- `core/models/auth.models.ts`: User and auth response interfaces.
- `core/services/auth.service.ts`: Angular Signals-based state (`currentUser`, `token`, `isAuthenticated`).
- `core/interceptors/jwt.interceptor.ts`: Attaches Bearer token to API requests.
- `core/guards/auth.guard.ts` & `guest.guard.ts`: Route protection.
- `features/auth/login/login.component.ts` & `.html`: Professional login form with Tailwind CSS.
- `features/auth/register/register.component.ts` & `.html`: Registration form with validation.

### Tests (`AgenticBoard.Tests`)
- `Auth/PasswordHasherTests.cs`: Verifies hashing, salting, and verification.
- `Auth/AuthServiceTests.cs`: Registration duplicate checks, valid login, invalid password 401 handling.
- `Integration/AuthEndpointsTests.cs`: Integration tests validating HTTP status codes and payloads.

## Security Considerations
- Zero exposure of password hashes in any API response or DTO.
- Sanitized generic error messages on authentication failures.
- Unique normalized email check preventing collisions.
- Protected `/me` endpoint strictly requiring valid JWT.

## Implementation Order
1. Domain entity and Application interfaces / DTOs / validators / exceptions.
2. Infrastructure implementations (DbContext, BCrypt, JWT, Clock).
3. API controller, middleware, and Program.cs registration.
4. Backend automated tests and validation.
5. Frontend services, interceptor, guards, and components.
6. Frontend build validation.
7. AI Code Review.
