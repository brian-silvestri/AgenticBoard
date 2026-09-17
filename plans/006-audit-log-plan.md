# PLAN-006: Audit Trail & Project Activity Implementation

## Summary
Expose the project activity stream capturing immutable events (`ProjectCreated`, `ProjectUpdated`, `MemberAdded`, `MemberRemoved`, `TaskCreated`, `StatusChanged`, `PriorityChanged`, `AssignedUserChanged`, `CommentAdded`, `TaskDeleted`), presenting human-readable timeline feeds in the API and Angular frontend.

## Architecture Impact

### Domain (`AgenticBoard.Domain`)
- `Entities/AuditLog.cs`: `Id`, `ProjectId`, `EntityType`, `EntityId`, `Action`, `OldValue`, `NewValue`, `PerformedById`, `PerformedByName`, `Timestamp`.

### Application (`AgenticBoard.Application`)
- `Features/Audit/Dtos/`: `AuditLogDto(int Id, int ProjectId, string EntityType, string EntityId, string Action, string? OldValue, string? NewValue, int? PerformedById, string? PerformedByName, DateTime Timestamp)`.
- `Features/Audit/Services/`: `IAuditLogService` & `AuditLogService`.
- `DependencyInjection.cs`: Register `IAuditLogService`.

### API (`AgenticBoard.Api`)
- `Controllers/ActivityController.cs`:
  - `GET /api/projects/{projectId}/activity`: Returns chronological audit records (newest first).

### Frontend (`frontend/src/app`)
- `core/models/audit.models.ts`: Interface for Activity / AuditLog.
- `core/services/audit.service.ts`: Querying activity feed.
- `features/projects/project-activity/project-activity.component.ts`: Visual timeline component with event icons, relative time formatting, and formatted action descriptions.
- Embed in `ProjectDetailComponent` under "Activity Trail" tab.

### Tests (`AgenticBoard.Tests`)
- `Audit/AuditLogServiceTests.cs`:
  - Activity query returns chronological events.
  - Non-member cannot access activity -> 403.
  - Verifies status change and priority change audit events exist.

## Implementation Order
1. Application DTOs and `IAuditLogService`.
2. `AuditLogService` and `ActivityController`.
3. Backend automated tests.
4. Frontend models, service, timeline component.
5. AI Code Review.
