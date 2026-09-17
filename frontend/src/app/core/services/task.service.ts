import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateTaskRequest, TaskItem, TaskItemDetail, TaskItemStatus, UpdateTaskRequest } from '../models/task.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private http = inject(HttpClient);

  getTasksByProject(projectId: number, status?: TaskItemStatus, assignedUserId?: number): Observable<TaskItem[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (assignedUserId) params = params.set('assignedUserId', assignedUserId.toString());

    return this.http.get<TaskItem[]>(`${environment.apiUrl}/projects/${projectId}/tasks`, { params });
  }

  getTaskById(id: number): Observable<TaskItemDetail> {
    return this.http.get<TaskItemDetail>(`${environment.apiUrl}/tasks/${id}`);
  }

  createTask(projectId: number, payload: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${environment.apiUrl}/projects/${projectId}/tasks`, payload);
  }

  updateTask(id: number, payload: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${environment.apiUrl}/tasks/${id}`, payload);
  }

  updateTaskStatus(id: number, status: TaskItemStatus): Observable<TaskItem> {
    return this.http.patch<TaskItem>(`${environment.apiUrl}/tasks/${id}/status`, { status });
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/tasks/${id}`);
  }
}
