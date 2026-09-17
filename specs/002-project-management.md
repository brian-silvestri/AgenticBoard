# SPEC-002: Project Management & Membership

## Context
AgenticBoard operates around collaborative projects. Projects act as organizational and security boundaries for tasks, boards, comments, and audit logs. A user may own multiple projects and participate in others as a member.

## Goal
Enable users to create, view, update, and manage projects, including inviting and removing project members with role-based access control (`Owner` vs. `Member`).

## User Story
- **As an** authenticated user,
- **I want to** create a new project with a name and description,
- **So that** I become its Owner and can organize tasks.
- **As a** project Owner,
- **I want to** add other users to my project by email and manage their membership,
- **So that** we can collaborate within a shared workspace.
- **As a** project Member,
- **I want to** view all projects I belong to and access their boards,
- **So that** I can track assigned work.

## Functional Requirements
1. **Create Project (`POST /api/projects`)**:
   - Accepts `name` and optional `description`.
   - The creator is automatically assigned as `Owner` in `ProjectMembers`.
   - Returns created project details.
2. **List My Projects (`GET /api/projects`)**:
   - Returns all projects where the authenticated user is either `Owner` or `Member`.
   - Users cannot see or enumerate projects they do not belong to.
3. **Get Project Details (`GET /api/projects/{id}`)**:
   - Returns project metadata, member list, and role of the calling user.
   - Enforces membership: non-members receive HTTP 403 Forbidden or HTTP 404 Not Found.
4. **Update Project (`PUT /api/projects/{id}`)**:
   - Allows changing project `name` and `description`.
   - Restricted strictly to project `Owner`.
5. **Add Project Member (`POST /api/projects/{id}/members`)**:
   - Accepts `email` (or `userId`) and `role` (`Member` or `Owner`).
   - Only an existing `Owner` can add members.
   - User must exist in the system; if already a member, returns HTTP 409 Conflict.
6. **Remove Project Member (`DELETE /api/projects/{id}/members/{userId}`)**:
   - Only an existing `Owner` can remove members.
   - An Owner cannot remove themselves if they are the sole owner (to prevent orphaned projects).

## Business Rules
- A project must have at least one `Owner`.
- Only members of a project can view its contents (tasks, comments, activity).
- Role permissions:
  - `Owner`: Full management (edit project, add/remove members, delete project, full task management).
  - `Member`: Collaborative access (view board, create/edit tasks, assign, comment).

## Acceptance Criteria
- [x] Given an authenticated user, creating a project registers them as Owner.
- [x] Given a user calling `GET /api/projects`, only projects they are a member of are returned.
- [x] Given an Owner, adding a valid registered user adds them as a project member.
- [x] Given a non-member attempting to read or modify a project, the request is rejected with 403/404.
- [x] Given a non-owner member attempting to add a new member, the request returns HTTP 403 Forbidden.

## Security Considerations
- Multi-tenancy isolation: Every project query includes tenant/user membership filters (`WHERE EXISTS (SELECT 1 FROM ProjectMembers ...)`).
- Prevent IDOR (Insecure Direct Object Reference) vulnerabilities on project IDs.

## Edge Cases
- Attempting to add an unregistered email returns HTTP 404 with a helpful error message.
- Removing a member currently assigned to active tasks reassigns or unassigns those tasks gracefully without violating foreign key constraints.

## Out of Scope
- Public projects.
- Organization or workspace hierarchies above projects.
