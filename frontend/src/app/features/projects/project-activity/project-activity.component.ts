import { Component, OnInit, input, signal, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { AuditService } from '../../../core/services/audit.service';
import { AuditLogItem } from '../../../core/models/audit.models';

@Component({
  selector: 'app-project-activity',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-base font-semibold text-slate-900 dark:text-white">Activity Log & Audit Trail</h3>
          <p class="text-xs text-slate-500 dark:text-slate-400">Chronological history of changes, task movements, and comments across this project.</p>
        </div>
        <button
          (click)="loadActivity()"
          class="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-slate-700 dark:text-slate-300 bg-white dark:bg-slate-800 border border-slate-300 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition"
        >
          <svg class="w-3.5 h-3.5" [class.animate-spin]="isLoading()" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Refresh
        </button>
      </div>

      <!-- Loading State -->
      @if (isLoading()) {
        <div class="space-y-4 py-4">
          @for (i of [1, 2, 3, 4]; track i) {
            <div class="flex items-start gap-3 animate-pulse">
              <div class="w-8 h-8 rounded-full bg-slate-200 dark:bg-slate-800"></div>
              <div class="flex-1 space-y-2">
                <div class="h-3.5 bg-slate-200 dark:bg-slate-800 rounded w-1/3"></div>
                <div class="h-3 bg-slate-200 dark:bg-slate-800 rounded w-2/3"></div>
              </div>
            </div>
          }
        </div>
      } @else if (activities().length === 0) {
        <div class="p-8 text-center bg-white dark:bg-slate-900 rounded-xl border border-dashed border-slate-300 dark:border-slate-800">
          <div class="w-12 h-12 mx-auto rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-400 mb-3">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <p class="text-sm font-medium text-slate-700 dark:text-slate-300">No activity recorded yet</p>
          <p class="text-xs text-slate-500 mt-1">Actions on tasks, status changes, and comments will appear here in real time.</p>
        </div>
      } @else {
        <!-- Activity Timeline -->
        <div class="relative pl-6 border-l-2 border-slate-200 dark:border-slate-800 space-y-6">
          @for (item of activities(); track item.id) {
            <div class="relative group">
              <!-- Timeline indicator dot -->
              <span
                class="absolute -left-[31px] top-1 flex items-center justify-center w-6 h-6 rounded-full text-white text-[11px] shadow-sm ring-4 ring-white dark:ring-slate-950"
                [ngClass]="getActionColor(item.action)"
              >
                <i [ngClass]="getActionIcon(item.action)"></i>
              </span>

              <div class="bg-white dark:bg-slate-900 border border-slate-200/80 dark:border-slate-800 rounded-lg p-3.5 shadow-sm hover:shadow transition">
                <div class="flex items-center justify-between mb-1">
                  <div class="flex items-center gap-2">
                    <span class="inline-flex items-center justify-center w-5 h-5 rounded-full bg-slate-200 dark:bg-slate-800 text-[10px] font-bold text-slate-700 dark:text-slate-300">
                      {{ (item.userName || 'U')[0].toUpperCase() }}
                    </span>
                    <span class="text-xs font-semibold text-slate-900 dark:text-white">
                      {{ item.userName || 'System' }}
                    </span>
                    <span class="text-[10px] px-2 py-0.5 rounded-full font-medium" [ngClass]="getActionBadgeClass(item.action)">
                      {{ formatAction(item.action) }}
                    </span>
                  </div>
                  <span class="text-[11px] text-slate-400">
                    {{ item.timestamp | date:'MMM d, y, h:mm a' }}
                  </span>
                </div>

                @if (item.details) {
                  <p class="text-xs text-slate-600 dark:text-slate-300 mt-1 pl-7 leading-relaxed">
                    {{ item.details }}
                  </p>
                }
              </div>
            </div>
          }
        </div>
      }
    </div>
  `
})
export class ProjectActivityComponent implements OnInit {
  projectId = input.required<number>();

  private readonly auditService = inject(AuditService);

  activities = signal<AuditLogItem[]>([]);
  isLoading = signal<boolean>(false);

  ngOnInit(): void {
    this.loadActivity();
  }

  loadActivity(): void {
    this.isLoading.set(true);
    this.auditService.getProjectActivity(this.projectId()).subscribe({
      next: (items) => {
        this.activities.set(items);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  formatAction(action: string): string {
    switch (action) {
      case 'TaskCreated': return 'Task Created';
      case 'StatusChanged': return 'Status Changed';
      case 'PriorityChanged': return 'Priority Changed';
      case 'AssignedUserChanged': return 'Assignment Changed';
      case 'TaskDeleted': return 'Task Deleted';
      case 'CommentAdded': return 'Comment Added';
      default: return action;
    }
  }

  getActionColor(action: string): string {
    switch (action) {
      case 'TaskCreated': return 'bg-emerald-500';
      case 'StatusChanged': return 'bg-blue-500';
      case 'PriorityChanged': return 'bg-amber-500';
      case 'AssignedUserChanged': return 'bg-purple-500';
      case 'TaskDeleted': return 'bg-rose-500';
      case 'CommentAdded': return 'bg-indigo-500';
      default: return 'bg-slate-500';
    }
  }

  getActionBadgeClass(action: string): string {
    switch (action) {
      case 'TaskCreated': return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/50 dark:text-emerald-300';
      case 'StatusChanged': return 'bg-blue-50 text-blue-700 dark:bg-blue-950/50 dark:text-blue-300';
      case 'PriorityChanged': return 'bg-amber-50 text-amber-700 dark:bg-amber-950/50 dark:text-amber-300';
      case 'AssignedUserChanged': return 'bg-purple-50 text-purple-700 dark:bg-purple-950/50 dark:text-purple-300';
      case 'TaskDeleted': return 'bg-rose-50 text-rose-700 dark:bg-rose-950/50 dark:text-rose-300';
      case 'CommentAdded': return 'bg-indigo-50 text-indigo-700 dark:bg-indigo-950/50 dark:text-indigo-300';
      default: return 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300';
    }
  }

  getActionIcon(action: string): string {
    return '';
  }
}
