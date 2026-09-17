import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskComment } from '../models/task.models';

export interface CreateCommentRequest {
  text: string;
}

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getComments(taskId: number): Observable<TaskComment[]> {
    return this.http.get<TaskComment[]>(`${this.baseUrl}/tasks/${taskId}/comments`);
  }

  addComment(taskId: number, text: string): Observable<TaskComment> {
    return this.http.post<TaskComment>(`${this.baseUrl}/tasks/${taskId}/comments`, { text });
  }
}
