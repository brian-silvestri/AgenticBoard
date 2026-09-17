# SPEC-003: Task Management

## Context
Tasks are the fundamental units of work in AgenticBoard. Tasks exist exclusively within the context of a project and can only be created, assigned, updated, and deleted by members of that project.

## Goal
Provide a comprehensive task lifecycle API and domain model allowing project members to create, update, assign, re-prioritize, and transition tasks through statuses.

## User Story
- **As a** project member,
- **I want to** create a task with title, description, priority, and optional assignee,
- **So that** our team can plan and track actionable work.
- **As a** project member,
- **I want to** update the status, priority, and assigned user of a task,
- **So that** our team remains aligned on progress.

## Functional Requirements
1. **Task Attributes**:
   - `Id`: Unique task identifier (int or Guid).
   - `ProjectId`: Associated project (required).
   - `Title`: Non-empty string (max 200 chars).
   - `Description`: Optional rich text or markdown (max 4000 chars).
   - `Status`: Enum (`Backlog`, `Todo`, `InProgress`, `Review`, `Done`).
   - `Priority`: Enum (`Low`, `Medium`, `High`, `Critical`).
   - `AssignedUserId`: Optional user ID. Must belong to the project.
   - `CreatedById`: User ID who created the task.
   - `CreatedAt`: UTC timestamp.
   - `UpdatedAt`: UTC timestamp.
2. **Endpoints**:
   - `GET /api/projects/{projectId}/tasks`: List all tasks for a project with filters by status or assignee.
   - `POST /api/projects/{projectId}/tasks`: Create a new task within the project.
   - `GET /api/tasks/{id}`: Get full task details including assigned user and recent comments.
   - `PUT /api/tasks/{id}`: Update title, description, status, priority, and assigned user.
   - `PATCH /api/tasks/{id}/status`: Dedicated endpoint to update task status (used by Kanban drag-and-drop).
   - `PATCH /api/tasks/{id}/assignment`: Dedicated endpoint to reassign task.
   - `DELETE /api/tasks/{id}`: Delete a task (restricted to project members or Owner).

## Business Rules
- Only members of the target project can create, update, or view its tasks.
- If an assignee is specified, the assigned user **must** be an active member of that project.
- Transitioning to any status is permitted, but status changes must trigger audit events.
- Priority changes must trigger audit events.

## Acceptance Criteria
- [x] Given a project member, creating a task with valid title and priority succeeds and returns 201 Created.
- [x] Given a non-member, any task operation returns 403 Forbidden.
- [x] Given an empty title, task creation is rejected with 400 Bad Request.
- [x] Given an assignee who is not a project member, task creation/update fails with 400/422 Unprocessable Entity.
- [x] Given a status change, the new status is persisted and an audit log entry is produced.

## Security Considerations
- Prevent cross-project leakage: `tasks/{id}` must verify the caller's membership in `task.Project`.
- Input sanitization on titles and descriptions.

## Edge Cases
- Assigning to null (unassigned) is valid.
- Attempting to update a deleted task returns 404 Not Found.
- Concurrency conflicts: update returns latest `UpdatedAt`.

## Out of Scope
- Sub-tasks or hierarchical task trees.
- Time tracking or estimates.
