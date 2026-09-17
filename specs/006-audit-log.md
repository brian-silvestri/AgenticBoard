# SPEC-006: Project Audit Trail & Activity Log

## Context
Traceability and accountability are paramount in professional project management software. Stakeholders and team members need to see who did what and when across project tasks and memberships.

## Goal
Implement an automatic, tamper-evident audit logging mechanism that records significant events, and expose an activity stream in both the API and frontend.

## User Story
- **As a** project member or owner,
- **I want to** view a chronological stream of project activities,
- **So that** I know who changed task statuses, updated priorities, assigned users, or joined the project.

## Functional Requirements
1. **Audited Events**:
   - `ProjectCreated`: Initial project setup.
   - `MemberAdded`: New member added with their role.
   - `MemberRemoved`: Member removed from project.
   - `TaskCreated`: Task initialized.
   - `TaskUpdated`: Title or description edited.
   - `StatusChanged`: Transition from old status to new status.
   - `PriorityChanged`: Change in task priority level.
   - `AssignedUserChanged`: Task assigned or unassigned.
   - `CommentAdded`: Comment posted on a task.
2. **Audit Schema**:
   - `Id`: Unique log ID.
   - `ProjectId`: Associated project ID.
   - `EntityType`: Target entity name (`Task`, `Project`, `ProjectMember`, `TaskComment`).
   - `EntityId`: String identifier of target entity.
   - `Action`: Standard action name.
   - `OldValue`: Optional previous value (JSON or string).
   - `NewValue`: Optional updated value (JSON or string).
   - `PerformedById`: User ID who executed the action.
   - `PerformedByName`: Snapshot of the user's name for historical display.
   - `Timestamp`: UTC timestamp.
3. **Endpoints**:
   - `GET /api/projects/{projectId}/activity`: List recent activity entries for the project (paginated or top N).
4. **Frontend View**:
   - Dedicated "Activity" tab / panel in project view showing readable human descriptions:
     - *"Brian changed status of Task #12 from InProgress to Done 5 minutes ago"*
     - *"Alice assigned Task #14 to Bob"*

## Business Rules
- Audit logs are append-only; they cannot be updated or deleted by users.
- Only members of the project can read its activity stream.

## Acceptance Criteria
- [x] Every task status change automatically generates an audit entry with `oldValue` and `newValue`.
- [x] Every priority change generates an audit entry.
- [x] Project activity endpoint returns ordered events (newest first).
- [x] Calling activity endpoint by a non-member returns 403 Forbidden.

## Security Considerations
- Read-only access to audit logs.
- Sanitize values before storing to prevent log injection.
