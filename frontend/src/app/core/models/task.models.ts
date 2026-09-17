export type TaskItemStatus = 'Backlog' | 'Todo' | 'InProgress' | 'Review' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface TaskComment {
  id: number;
  taskItemId: number;
  authorId: number;
  authorName: string;
  text: string;
  createdAt: string;
}

export interface TaskItem {
  id: number;
  projectId: number;
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  assignedUserId?: number;
  assignedUserName?: string;
  createdById: number;
  createdByName: string;
  createdAt: string;
  updatedAt?: string;
  commentCount: number;
}

export interface TaskItemDetail extends TaskItem {
  comments: TaskComment[];
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: TaskPriority;
  assignedUserId?: number;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  assignedUserId?: number;
}

export interface UpdateTaskStatusRequest {
  status: TaskItemStatus;
}
