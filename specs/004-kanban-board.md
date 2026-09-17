# SPEC-004: Kanban Board View

## Context
Visualizing work flow is critical for agile project management. The Kanban board provides an intuitive column-based representation of tasks grouped by their current status, with drag-and-drop capability.

## Goal
Implement a responsive, interactive Kanban board in Angular using Angular CDK Drag & Drop, backed by the status patch endpoint.

## User Story
- **As a** project member,
- **I want to** view project tasks organized into distinct status columns (`Backlog`, `Todo`, `In Progress`, `Review`, `Done`),
- **So that** I can assess project bottlenecks and progress at a glance.
- **As a** project member,
- **I want to** drag and drop a task card from one column to another,
- **So that** the task status immediately updates on the board and in the backend.

## Functional Requirements
1. **Board Layout**:
   - 5 standard columns:
     1. Backlog
     2. Todo
     3. In Progress
     4. Review
     5. Done
   - Column headers display status title and real-time task count badge.
2. **Task Cards**:
   - Displays task `Title`, `Priority` badge (color-coded: Low=slate, Medium=blue, High=amber, Critical=red), `AssignedUser` avatar/initials, and task key/ID.
   - Clicking a card opens the Task Detail view / modal.
3. **Drag & Drop Interactions**:
   - Utilizes `@angular/cdk/drag-drop` (`cdkDropListGroup`, `cdkDropList`, `cdkDrag`).
   - Dragging between columns invokes an optimistic or immediate state transition.
   - Calls `PATCH /api/tasks/{id}/status` with the new status.
   - If the API call fails, the card reverts back to its original column with an error toast notification.
4. **Quick Task Creation**:
   - "New Task" button opening a clean creation dialog pre-filled with the active project.

## Acceptance Criteria
- [x] Board loads all tasks for the selected project and sorts them into respective status columns.
- [x] Dragging a task between columns triggers `PATCH /api/tasks/{id}/status`.
- [x] Successful API response updates the local state and displays visual confirmation.
- [x] Failed API request displays error toast and restores original status column.
- [x] Responsive layout collapses gracefully or scrolls horizontally on smaller viewports.

## Security & Reliability Considerations
- Only authenticated project members can drag/drop. If a user is not authorized, the API returns 403 and the UI rolls back the move.
- Debounce or guard against rapid multiple drops.

## Edge Cases
- Moving a task within the same column is a no-op (no API call needed).
- Moving into the same status doesn't produce redundant audit logs.
