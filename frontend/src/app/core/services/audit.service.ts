import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuditLogItem } from '../models/audit.models';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getProjectActivity(projectId: number, limit: number = 50): Observable<AuditLogItem[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<AuditLogItem[]>(`${this.baseUrl}/projects/${projectId}/activity`, { params });
  }
}
