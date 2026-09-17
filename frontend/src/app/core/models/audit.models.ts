export interface AuditLogItem {
  id: number;
  entityName: string;
  entityId: number;
  action: string;
  projectId?: number;
  userId?: number;
  userName: string;
  userEmail: string;
  details?: string;
  timestamp: string;
}
