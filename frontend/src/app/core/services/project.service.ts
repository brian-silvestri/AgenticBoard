import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AddProjectMemberRequest, CreateProjectRequest, Project, ProjectDetail, ProjectMember, UpdateProjectRequest } from '../models/project.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private http = inject(HttpClient);

  readonly projects = signal<Project[]>([]);
  readonly currentProject = signal<ProjectDetail | null>(null);
  readonly isLoading = signal<boolean>(false);

  getMyProjects(): Observable<Project[]> {
    this.isLoading.set(true);
    return this.http.get<Project[]>(`${environment.apiUrl}/projects`).pipe(
      tap(data => {
        this.projects.set(data);
        this.isLoading.set(false);
      })
    );
  }

  getProjectById(id: number): Observable<ProjectDetail> {
    this.isLoading.set(true);
    return this.http.get<ProjectDetail>(`${environment.apiUrl}/projects/${id}`).pipe(
      tap(data => {
        this.currentProject.set(data);
        this.isLoading.set(false);
      })
    );
  }

  createProject(payload: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${environment.apiUrl}/projects`, payload).pipe(
      tap(newProject => {
        this.projects.update(list => [newProject, ...list]);
      })
    );
  }

  updateProject(id: number, payload: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${environment.apiUrl}/projects/${id}`, payload).pipe(
      tap(updated => {
        this.projects.update(list => list.map(p => p.id === id ? updated : p));
        if (this.currentProject()?.id === id) {
          this.currentProject.update(curr => curr ? { ...curr, name: updated.name, description: updated.description } : null);
        }
      })
    );
  }

  addMember(projectId: number, payload: AddProjectMemberRequest): Observable<ProjectMember> {
    return this.http.post<ProjectMember>(`${environment.apiUrl}/projects/${projectId}/members`, payload).pipe(
      tap(newMember => {
        if (this.currentProject()?.id === projectId) {
          this.currentProject.update(curr => curr ? { ...curr, members: [...curr.members, newMember] } : null);
        }
        this.projects.update(list => list.map(p => p.id === projectId ? { ...p, memberCount: p.memberCount + 1 } : p));
      })
    );
  }

  removeMember(projectId: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/projects/${projectId}/members/${userId}`).pipe(
      tap(() => {
        if (this.currentProject()?.id === projectId) {
          this.currentProject.update(curr => curr ? { ...curr, members: curr.members.filter(m => m.userId !== userId) } : null);
        }
        this.projects.update(list => list.map(p => p.id === projectId ? { ...p, memberCount: Math.max(1, p.memberCount - 1) } : p));
      })
    );
  }
}
