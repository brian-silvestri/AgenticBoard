# SPEC-005: Task Comments & Discussions

## Context
Collaboration requires targeted communication directly where the work takes place. Team members need to ask clarifying questions, leave technical notes, and provide feedback on individual tasks.

## Goal
Enable project members to view and add threaded comments within any task belonging to their project.

## User Story
- **As a** project member,
- **I want to** write comments on a task,
- **So that** I can share updates, context, and feedback with other team members.
- **As a** project member,
- **I want to** view the history of comments on a task in chronological order,
- **So that** I understand the conversation timeline.

## Functional Requirements
1. **Comment Attributes**:
   - `Id`: Unique comment ID.
   - `TaskId`: Associated task ID.
   - `AuthorId`: User ID of author.
   - `AuthorName`: Display name of author.
   - `Text`: Comment body (markdown or plain text, 1-2000 chars).
   - `CreatedAt`: UTC timestamp.
2. **Endpoints**:
   - `GET /api/tasks/{taskId}/comments`: Retrieve chronological list of comments for the task.
   - `POST /api/tasks/{taskId}/comments`: Post a new comment.
3. **Auditing**:
   - Adding a comment triggers a `CommentAdded` event in the project audit trail.

## Business Rules
- Only users who are members of the task's project can read or post comments.
- Comments cannot be empty or whitespace only.
- Comments are immutable for the initial version.

## Acceptance Criteria
- [x] Given a project member, posting a non-empty comment succeeds and returns 201 Created.
- [x] Given a non-member, accessing or posting comments returns 403 Forbidden.
- [x] Comments are listed chronologically (oldest to newest or newest with clear timestamps).
- [x] Posting a comment records an audit log entry.

## Security Considerations
- XSS prevention on comment text rendering.
- Validation that the task exists and caller has access to the task's project.

## Edge Cases
- Long comments wrap cleanly without breaking UI layout.
- Submitting empty or purely whitespace comments is prevented client-side and rejected with 400 server-side.
