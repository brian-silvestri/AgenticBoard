# PLAN-002: Project Management & Membership Implementation

## Summary
Implement multi-tenant project management allowing authenticated users to create projects, list their associated projects, retrieve project details with role validation, update projects, and manage project members (Owner vs. Member roles) with automatic audit event tracking.

## Architecture Impact

### Domain (`AgenticBoard.Domain`)
- `Entities/Project.cs`: Already defined in Phase 1/2 with `Members`, `Tasks`, `AuditLogs`.
- `Entities/ProjectMember.cs`: Composite primary key `(ProjectId, UserId)` with `ProjectRole`.

### Application (`AgenticBoard.Application`)
- `Features/Projects/Dtos/`:
  - `CreateProjectDto`, `UpdateProjectDto`, `ProjectDto`, `ProjectDetailDto`, `ProjectMemberDto`, `AddProjectMemberDto`.
- `Features/Projects/Validators/`:
  - `CreateProjectDtoValidator`, `UpdateProjectDtoValidator`, `AddProjectMemberDtoValidator`.
- `Features/Projects/Services/`:
  - `IProjectService` & `ProjectService`: Business logic enforcing tenant isolation and ownership rules.
- `DependencyInjection.cs`: Register `IProjectService`.

### API (`AgenticBoard.Api`)
- `Controllers/ProjectsController.cs`:
  - `GET /api/projects`: List user's projects.
  - `POST /api/projects`: Create project (creator becomes Owner).
  - `GET /api/projects/{id}`: Detailed view (enforces membership).
  - `PUT /api/projects/{id}`: Update project metadata (enforces Owner).
  - `POST /api/projects/{id}/members`: Add member by email (enforces Owner).
  - `DELETE /api/projects/{id}/members/{userId}`: Remove member (enforces Owner; prevents orphan project).

### Frontend (`frontend/src/app`)
- `core/models/project.models.ts`: Interfaces for Project, Member, and Request DTOs.
- `core/services/project.service.ts`: Angular service for project API calls.
- `features/projects/project-list/project-list.component.ts`: Projects directory card grid + Create Project modal.
- `features/projects/project-detail/project-detail.component.ts`: Project container with header, role indicator, and tabs (`Board`, `Members`, `Activity`).
- `features/projects/project-members/project-members.component.ts`: Team management panel with Add Member modal.

### Tests (`AgenticBoard.Tests`)
- `Projects/ProjectServiceTests.cs`:
  - User creates project -> creator is Owner.
  - Unauthorized user cannot read project.
  - Owner adds registered user -> member added.
  - Non-owner cannot add member.
  - Adding duplicate member throws `ConflictException`.
  - Removing sole owner throws `AppException`.
- `Integration/ProjectEndpointsTests.cs`:
  - HTTP status codes (200, 201, 400, 403, 404, 409).

## Implementation Order
1. Application DTOs, validators, and `IProjectService` interface.
2. `ProjectService` implementation with audit logging.
3. `ProjectsController` in API.
4. Backend automated tests (unit & integration).
5. Frontend models, service, and UI components.
6. Verify builds and tests.
7. AI Code Review.
