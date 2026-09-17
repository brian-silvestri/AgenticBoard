export type ProjectRole = 'Owner' | 'Member';

export interface ProjectMember {
  userId: number;
  email: string;
  fullName: string;
  role: ProjectRole;
  joinedAt: string;
}

export interface Project {
  id: number;
  name: string;
  description?: string;
  createdById: number;
  createdByName: string;
  createdAt: string;
  myRole: ProjectRole;
  memberCount: number;
  taskCount: number;
}

export interface ProjectDetail {
  id: number;
  name: string;
  description?: string;
  createdById: number;
  createdByName: string;
  createdAt: string;
  myRole: ProjectRole;
  members: ProjectMember[];
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string;
}

export interface AddProjectMemberRequest {
  email: string;
  role: ProjectRole;
}
