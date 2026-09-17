# PLAN-003 & 004: Task Management & Kanban Board Implementation

## Summary
Implement the complete Task Lifecycle and interactive Kanban Board view with drag-and-drop state persistence, priority indicators, assignee management, and automatic audit event tracking.

## Architecture Impact

### Domain (`AgenticBoard.Domain`)
- `Entities/TaskItem.cs`: `Id`, `ProjectId`, `Title`, `Description`, `Status`, `Priority`, `AssignedUserId`, `CreatedById`, `CreatedAt`, `UpdatedAt`.
- `Enums/TaskItemStatus.cs`: `Backlog`, `Todo`, `InProgress`, `Review`, `Done`.
- `Enums/TaskPriority.cs`: `Low`, `Medium`, `High`, `Critical`.

### Application (`AgenticBoard.Application`)
- `Features/Tasks/Dtos/`:
  - `CreateTaskDto`, `UpdateTaskDto`, `UpdateTaskStatusDto`, `UpdateTaskAssignmentDto`, `TaskItemDto`, `TaskItemDetailDto`.
- `Features/Tasks/Validators/`:
  - `CreateTaskDtoValidator`, `UpdateTaskDtoValidator`, `UpdateTaskStatusDtoValidator`.
- `Features/Tasks/Services/`:
  - `ITaskService` & `TaskService`: Business logic verifying project membership for both caller and assignee, producing audit log entries on status/priority/assignment changes.
- `DependencyInjection.cs`: Register `ITaskService`.

### API (`AgenticBoard.Api`)
- `Controllers/TasksController.cs`:
  - `GET /api/projects/{projectId}/tasks`
  - `POST /api/projects/{projectId}/tasks`
  - `GET /api/tasks/{id}`
  - `PUT /api/tasks/{id}`
  - `PATCH /api/tasks/{id}/status`
  - `PATCH /api/tasks/{id}/assignment`
  - `DELETE /api/tasks/{id}`

### Frontend (`frontend/src/app`)
- `core/models/task.models.ts`: Interfaces for Tasks, Statuses, Priorities, and Request payloads.
- `core/services/task.service.ts`: Task querying, creation, updating, and status patching.
- `features/kanban/kanban-board/kanban-board.component.ts`:
  - 5 columns (`Backlog`, `Todo`, `In Progress`, `Review`, `Done`).
  - `@angular/cdk/drag-drop` integration (`cdkDropListGroup`, `cdkDropList`, `cdkDrag`, `moveItemInArray`, `transferArrayItem`).
  - Optimistic move with rollback on network failure.
  - Quick Task Create dialog.
  - Task Detail view modal with edit actions.
- Embed Kanban board into `ProjectDetailComponent`.

### Tests (`AgenticBoard.Tests`)
- `Tasks/TaskServiceTests.cs`:
  - Project member creates task -> 201.
  - Non-member cannot create task -> 403.
  - Assignment to non-member rejected -> 400.
  - Empty title rejected -> 400.
  - Status change produces audit log with `oldValue` and `newValue`.
  - Priority change produces audit log.
- `Integration/TaskEndpointsTests.cs`:
  - Full API lifecycle with drag-drop PATCH status.

## Implementation Order
1. Application DTOs, validators, and `ITaskService`.
2. `TaskService` implementation with audit logging.
3. `TasksController` in API.
4. Backend automated tests (unit & integration).
5. Frontend models, service, Kanban board component with CDK drag-and-drop.
6. Verification (`dotnet test`, `npm run build`).
7. AI Code Review.
