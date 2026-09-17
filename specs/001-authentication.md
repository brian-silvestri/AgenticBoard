# SPEC-001: Authentication & User Identity

## Context
AgenticBoard is a multi-user project and task management system. In order to attribute tasks, enforce project membership boundaries, and maintain auditable trails, the platform requires a secure authentication and user management mechanism.

## Goal
Provide secure user registration, credential verification (login), cryptographic password protection, JSON Web Token (JWT) issuance, current user profile retrieval, and client-side session management.

## User Story
- **As a** new developer or team member,
- **I want to** register an account with my email, full name, and a secure password,
- **So that** I can access projects, manage tasks, and collaborate with my team.
- **As an** existing user,
- **I want to** log in with my credentials to obtain a secure session token,
- **So that** I can interact with authorized resources.

## Functional Requirements
1. **User Registration (`POST /api/auth/register`)**:
   - Accepts `email`, `fullName`, and `password`.
   - Validates email format, unique email constraint, and password strength (minimum 8 characters, at least 1 uppercase, 1 lowercase, 1 number).
   - Hashes password using cryptographically secure hashing (PBKDF2/BCrypt) with random salt.
   - Creates a user record in the database.
   - Returns a JWT bearer token and user profile details upon successful registration.
2. **User Login (`POST /api/auth/login`)**:
   - Accepts `email` and `password`.
   - Validates existence and verifies password hash against stored hash.
   - Returns HTTP 401 Unauthorized for invalid credentials with generic failure messages (preventing user enumeration).
   - Issues a signed JWT token containing standard claims (`sub` = UserId, `email`, `name`).
3. **Current User Profile (`GET /api/auth/me`)**:
   - Requires valid JWT `Authorization: Bearer <token>`.
   - Resolves authenticated `UserId` from claims and returns user details (`id`, `email`, `fullName`, `createdAt`).
4. **Client-side Session**:
   - Angular stores token securely (local storage / memory) and attaches it automatically via HTTP Interceptor to all outgoing API requests.
   - Frontend `AuthGuard` restricts access to protected views (`/dashboard`, `/projects`, etc.).
   - Logout clears the stored session and redirects to `/login`.

## Business Rules
- Emails must be normalized (lowercased, trimmed) and unique across the system.
- Passwords must never be stored, logged, or serialized in plaintext.
- JWT tokens have a defined expiration (e.g., 24 hours in development/production).
- Unauthenticated requests to protected endpoints must return HTTP 401.

## Acceptance Criteria
- [x] Given a valid registration payload, when submitted, a new user is created and a valid JWT token is returned.
- [x] Given a duplicate email, registration returns HTTP 409 Conflict with structured `ProblemDetails`.
- [x] Given valid credentials, login returns HTTP 200 OK with a valid JWT token.
- [x] Given invalid credentials, login returns HTTP 401 Unauthorized without disclosing whether the email exists.
- [x] Given an authenticated user calling `GET /api/auth/me`, the system returns the corresponding profile.
- [x] Given an unauthenticated or expired request to `GET /api/auth/me`, the system returns HTTP 401.

## Security Considerations
- Cryptographic salt per password.
- JWT signing key must be at least 256 bits (HMAC-SHA256).
- Protection against timing attacks in password verification.
- CORS policy restricts allowed origins.
- Rate limiting / lockout considerations for production.

## Edge Cases
- Mixed-case emails (e.g., `User@Example.com` vs `user@example.com`) handled consistently.
- Malformed JWT tokens must be rejected gracefully with HTTP 401.
- Whitespace in names or emails trimmed automatically.

## Out of Scope
- OAuth2 / Social login (Google/GitHub SSO).
- Email verification via SMTP.
- Multi-factor authentication (MFA).
