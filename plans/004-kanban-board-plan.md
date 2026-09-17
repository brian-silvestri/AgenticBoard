# PLAN-004: Kanban Board View Implementation

## Summary
Implement the interactive Kanban Board frontend component utilizing Angular CDK Drag & Drop (`@angular/cdk/drag-drop`), bound to `PATCH /api/tasks/{id}/status`.

## Architecture Impact
- Standalone Angular Component: `KanbanBoardComponent`.
- Angular CDK Drag & Drop directives: `cdkDropListGroup`, `cdkDropList`, `cdkDrag`.
- Connected column lists for:
  1. `Backlog`
  2. `Todo`
  3. `InProgress`
  4. `Review`
  5. `Done`
- Optimistic drag-and-drop state update with error rollback and toast notification.
- Quick create task modal and task detail inspection modal.

## Implementation Order
- Defined alongside Task Management (Plan 003).
- Embedded within `/projects/:id` (and accessible via direct route `/projects/:id/board`).
