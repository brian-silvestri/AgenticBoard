# PLAN-005: Task Comments Implementation

## Summary
Implement threaded comments on tasks allowing project members to collaborate, post updates, and view discussion history, backed by automatic audit logging.

## Architecture Impact

### Domain (`AgenticBoard.Domain`)
- `Entities/TaskComment.cs`: `Id`, `TaskItemId`, `AuthorId`, `Text`, `CreatedAt`.

### Application (`AgenticBoard.Application`)
- `Features/Comments/Dtos/`: `CreateCommentDto`, `CommentDto`.
- `Features/Comments/Validators/`: `CreateCommentDtoValidator`.
- `Features/Comments/Services/`: `ICommentService` & `CommentService`.
- `DependencyInjection.cs`: Register `ICommentService`.

### API (`AgenticBoard.Api`)
- `Controllers/CommentsController.cs`:
  - `GET /api/tasks/{taskId}/comments`
  - `POST /api/tasks/{taskId}/comments`

### Frontend (`frontend/src/app`)
- `core/services/comment.service.ts`: Methods to fetch and submit task comments.
- Integration inside Kanban Task Detail Modal: live comment list, author avatar, timestamp, comment input box.

### Tests (`AgenticBoard.Tests`)
- `Comments/CommentServiceTests.cs`:
  - Project member adds comment -> 201 Created & audit log.
  - Non-member cannot add/view comments -> 403 Forbidden.
  - Empty comment rejected -> 400 Bad Request.

## Implementation Order
1. Application DTOs, validator, service.
2. API controller.
3. Tests.
4. Frontend integration.
5. AI Code Review.
