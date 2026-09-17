import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ProjectDetail } from '../../../core/models/project.models';
import { TaskItem, TaskItemStatus, TaskPriority, TaskComment } from '../../../core/models/task.models';
import { TaskService } from '../../../core/services/task.service';
import { CommentService } from '../../../core/services/comment.service';

interface KanbanColumn {
  id: TaskItemStatus;
  title: string;
  colorClass: string;
}

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DragDropModule],
  template: `
    <div class="space-y-6">
      <!-- Toolbar: Filters & Quick Actions -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-900/60 border border-slate-800/80 rounded-2xl p-4">
        <div class="flex items-center gap-3">
          <span class="text-xs font-semibold uppercase text-slate-400">Board Overview</span>
          <span class="text-xs text-slate-500">&bull;</span>
          <span class="text-xs text-slate-400">{{ allTasks().length }} total tasks</span>
        </div>

        <div class="flex items-center gap-3">
          <button
            (click)="showCreateModal.set(true)"
            class="px-3.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-xs shadow-md shadow-indigo-600/20 transition-all flex items-center gap-1.5"
          >
            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span>Create Task</span>
          </button>
        </div>
      </div>

      <!-- Error notification if drag & drop fails -->
      @if (errorMessage()) {
        <div class="p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-xs flex items-center justify-between">
          <span>{{ errorMessage() }}</span>
          <button (click)="errorMessage.set(null)" class="text-rose-400 hover:text-white">&times;</button>
        </div>
      }

      <!-- Kanban Columns Grid with Drag & Drop Group -->
      <div
        cdkDropListGroup
        class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4 items-start overflow-x-auto pb-6"
      >
        @for (col of columns; track col.id) {
          <div class="bg-slate-900/70 border border-slate-800/80 rounded-2xl flex flex-col max-h-[calc(100vh-280px)] min-h-[400px]">
            <!-- Column Header -->
            <div class="p-3.5 border-b border-slate-800/80 flex items-center justify-between">
              <div class="flex items-center gap-2">
                <span class="w-2.5 h-2.5 rounded-full {{ col.colorClass }}"></span>
                <h4 class="font-bold text-xs text-white uppercase tracking-wider">{{ col.title }}</h4>
              </div>
              <span class="px-2 py-0.5 rounded-full bg-slate-800 text-slate-400 font-bold text-[10px]">
                {{ getColumnTasks(col.id).length }}
              </span>
            </div>

            <!-- Task List Drop Zone -->
            <div
              cdkDropList
              [id]="col.id"
              [cdkDropListData]="getColumnTasks(col.id)"
              (cdkDropListDropped)="onDrop($event, col.id)"
              class="p-3 space-y-2.5 flex-1 overflow-y-auto min-h-[150px]"
            >
              @for (task of getColumnTasks(col.id); track task.id) {
                <div
                  cdkDrag
                  (click)="openDetailModal(task)"
                  class="bg-slate-800/80 hover:bg-slate-800 border border-slate-700/60 hover:border-indigo-500/40 rounded-xl p-3.5 shadow-md cursor-pointer transition-all group"
                >
                  <div class="flex items-start justify-between gap-2 mb-2">
                    <span class="text-[10px] font-bold text-slate-500 group-hover:text-indigo-400 transition-colors">
                      #{{ task.id }}
                    </span>
                    <span
                      class="px-2 py-0.5 rounded-md text-[10px] font-semibold"
                      [class]="getPriorityClass(task.priority)"
                    >
                      {{ task.priority }}
                    </span>
                  </div>

                  <h5 class="text-xs font-semibold text-white leading-snug line-clamp-2 mb-2.5">
                    {{ task.title }}
                  </h5>

                  <div class="flex items-center justify-between pt-2 border-t border-slate-700/50 text-[11px] text-slate-400">
                    <div class="flex items-center gap-1.5">
                      @if (task.assignedUserName) {
                        <div class="w-5 h-5 rounded-full bg-indigo-500/20 text-indigo-300 font-bold flex items-center justify-center text-[9px]">
                          {{ getInitials(task.assignedUserName) }}
                        </div>
                        <span class="text-[10px] text-slate-300 truncate max-w-[80px]">{{ task.assignedUserName }}</span>
                      } @else {
                        <span class="text-[10px] text-slate-500 italic">Unassigned</span>
                      }
                    </div>

                    @if (task.commentCount > 0) {
                      <span class="flex items-center gap-1 text-[10px] text-slate-400">
                        <svg class="w-3 h-3 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                        </svg>
                        {{ task.commentCount }}
                      </span>
                    }
                  </div>
                </div>
              }

              @if (getColumnTasks(col.id).length === 0) {
                <div class="py-8 text-center border-2 border-dashed border-slate-800/80 rounded-xl">
                  <p class="text-[11px] text-slate-500 font-medium">No tasks</p>
                </div>
              }
            </div>
          </div>
        }
      </div>

      <!-- Create Task Modal -->
      @if (showCreateModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
          <div class="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h3 class="text-base font-bold text-white mb-1">Create Task</h3>
            <p class="text-slate-400 text-xs mb-4">Add a new item to {{ project.name }}</p>

            <form [formGroup]="createTaskForm" (ngSubmit)="onCreateTask()" class="space-y-4">
              <div>
                <label for="t-title" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Title</label>
                <input
                  id="t-title"
                  type="text"
                  formControlName="title"
                  placeholder="Task title..."
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label for="t-desc" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Description (Optional)</label>
                <textarea
                  id="t-desc"
                  rows="3"
                  formControlName="description"
                  placeholder="Add details, acceptance criteria, or technical notes..."
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                ></textarea>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label for="t-prio" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Priority</label>
                  <select
                    id="t-prio"
                    formControlName="priority"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </select>
                </div>

                <div>
                  <label for="t-assignee" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Assignee</label>
                  <select
                    id="t-assignee"
                    formControlName="assignedUserId"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option [ngValue]="null">Unassigned</option>
                    @for (member of project.members; track member.userId) {
                      <option [ngValue]="member.userId">{{ member.fullName }}</option>
                    }
                  </select>
                </div>
              </div>

              <div class="flex items-center justify-end gap-3 pt-3">
                <button
                  type="button"
                  (click)="showCreateModal.set(false)"
                  class="px-4 py-2 rounded-xl border border-slate-700 text-slate-300 text-xs font-semibold hover:bg-slate-800 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  [disabled]="createTaskForm.invalid || isSubmitting()"
                  class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 disabled:opacity-50 transition-all"
                >
                  {{ isSubmitting() ? 'Creating...' : 'Create Task' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }

      <!-- Task Detail / Edit Modal -->
      @if (selectedTask(); as task) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm overflow-y-auto">
          <div class="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-2xl p-6 shadow-2xl relative my-8 max-h-[90vh] flex flex-col">
            <div class="flex items-start justify-between gap-4 mb-4 shrink-0 pb-3 border-b border-slate-800">
              <div>
                <span class="text-xs font-bold text-indigo-400">Task #{{ task.id }}</span>
                <h3 class="text-lg font-bold text-white mt-0.5">{{ task.title }}</h3>
              </div>
              <button (click)="selectedTask.set(null)" class="text-slate-400 hover:text-white text-xl leading-none">&times;</button>
            </div>

            <div class="overflow-y-auto pr-1 flex-1 space-y-6">
              <form [formGroup]="editTaskForm" (ngSubmit)="onUpdateTask(task.id)" class="space-y-4">
                <div>
                  <label for="ed-title" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Title</label>
                  <input
                    id="ed-title"
                    type="text"
                    formControlName="title"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>

                <div>
                  <label for="ed-desc" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Description</label>
                  <textarea
                    id="ed-desc"
                    rows="3"
                    formControlName="description"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  ></textarea>
                </div>

                <div class="grid grid-cols-3 gap-3">
                  <div>
                    <label for="ed-status" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Status</label>
                    <select
                      id="ed-status"
                      formControlName="status"
                      class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    >
                      <option value="Backlog">Backlog</option>
                      <option value="Todo">Todo</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Review">Review</option>
                      <option value="Done">Done</option>
                    </select>
                  </div>

                  <div>
                    <label for="ed-prio" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Priority</label>
                    <select
                      id="ed-prio"
                      formControlName="priority"
                      class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    >
                      <option value="Low">Low</option>
                      <option value="Medium">Medium</option>
                      <option value="High">High</option>
                      <option value="Critical">Critical</option>
                    </select>
                  </div>

                  <div>
                    <label for="ed-assignee" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Assignee</label>
                    <select
                      id="ed-assignee"
                      formControlName="assignedUserId"
                      class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    >
                      <option [ngValue]="null">Unassigned</option>
                      @for (member of project.members; track member.userId) {
                        <option [ngValue]="member.userId">{{ member.fullName }}</option>
                      }
                    </select>
                  </div>
                </div>

                <div class="flex items-center justify-between pt-4 border-t border-slate-800">
                  <button
                    type="button"
                    (click)="onDeleteTask(task.id)"
                    class="px-3.5 py-2 rounded-xl border border-rose-500/30 text-rose-400 hover:bg-rose-500/10 text-xs font-semibold transition-colors"
                  >
                    Delete Task
                  </button>

                  <div class="flex items-center gap-2">
                    <button
                      type="button"
                      (click)="selectedTask.set(null)"
                      class="px-4 py-2 rounded-xl border border-slate-700 text-slate-300 text-xs font-semibold hover:bg-slate-800 transition-colors"
                    >
                      Close
                    </button>
                    <button
                      type="submit"
                      [disabled]="editTaskForm.invalid || isSubmitting()"
                      class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 disabled:opacity-50 transition-all"
                    >
                      {{ isSubmitting() ? 'Saving...' : 'Save Changes' }}
                    </button>
                  </div>
                </div>
              </form>

              <!-- Discussion / Comments Section -->
              <div class="pt-4 border-t border-slate-800 space-y-4">
                <div class="flex items-center justify-between">
                  <h4 class="text-xs font-bold uppercase tracking-wider text-slate-300 flex items-center gap-2">
                    <span>Discussion</span>
                    <span class="px-2 py-0.5 rounded-full bg-slate-800 text-[10px] text-slate-400">{{ taskComments().length }}</span>
                  </h4>
                </div>

                <!-- Add Comment Input -->
                <div class="space-y-2">
                  <textarea
                    [value]="newCommentText()"
                    (input)="newCommentText.set($any($event.target).value)"
                    placeholder="Write a comment or status update..."
                    rows="2"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none"
                  ></textarea>
                  <div class="flex justify-end">
                    <button
                      type="button"
                      (click)="onAddComment(task.id)"
                      [disabled]="!newCommentText().trim() || isPostingComment()"
                      class="px-3.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold disabled:opacity-50 transition-all flex items-center gap-1.5"
                    >
                      <span>{{ isPostingComment() ? 'Posting...' : 'Post Comment' }}</span>
                    </button>
                  </div>
                </div>

                <!-- Comments List -->
                @if (isLoadingComments()) {
                  <div class="space-y-2 py-2">
                    <div class="h-10 bg-slate-800/60 rounded-xl animate-pulse"></div>
                    <div class="h-10 bg-slate-800/60 rounded-xl animate-pulse"></div>
                  </div>
                } @else if (taskComments().length === 0) {
                  <p class="text-xs text-slate-500 italic py-2 text-center">No comments yet. Start the conversation!</p>
                } @else {
                  <div class="space-y-3 pt-1">
                    @for (comment of taskComments(); track comment.id) {
                      <div class="bg-slate-950/60 border border-slate-800/80 rounded-xl p-3 space-y-1">
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2">
                            <div class="w-5 h-5 rounded-full bg-indigo-600/30 border border-indigo-500/30 flex items-center justify-center text-[10px] font-bold text-indigo-300">
                              {{ getInitials(comment.authorName) }}
                            </div>
                            <span class="text-xs font-semibold text-white">{{ comment.authorName }}</span>
                          </div>
                          <span class="text-[10px] text-slate-400">{{ comment.createdAt | date:'MMM d, h:mm a' }}</span>
                        </div>
                        <p class="text-xs text-slate-300 pl-7 leading-relaxed whitespace-pre-wrap">{{ comment.text }}</p>
                      </div>
                    }
                  </div>
                }
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class KanbanBoardComponent implements OnInit {
  @Input({ required: true }) project!: ProjectDetail;

  private taskService = inject(TaskService);
  private commentService = inject(CommentService);
  private fb = inject(FormBuilder);

  readonly columns: KanbanColumn[] = [
    { id: 'Backlog', title: 'Backlog', colorClass: 'bg-slate-400' },
    { id: 'Todo', title: 'Todo', colorClass: 'bg-blue-400' },
    { id: 'InProgress', title: 'In Progress', colorClass: 'bg-amber-400' },
    { id: 'Review', title: 'Review', colorClass: 'bg-purple-400' },
    { id: 'Done', title: 'Done', colorClass: 'bg-emerald-400' },
  ];

  allTasks = signal<TaskItem[]>([]);
  selectedTask = signal<TaskItem | null>(null);
  taskComments = signal<TaskComment[]>([]);
  isLoadingComments = signal<boolean>(false);
  newCommentText = signal<string>('');
  isPostingComment = signal<boolean>(false);
  showCreateModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  createTaskForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(4000)]],
    priority: ['Medium' as TaskPriority, [Validators.required]],
    assignedUserId: [null as number | null]
  });

  editTaskForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(4000)]],
    status: ['Backlog' as TaskItemStatus, [Validators.required]],
    priority: ['Medium' as TaskPriority, [Validators.required]],
    assignedUserId: [null as number | null]
  });

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getTasksByProject(this.project.id).subscribe({
      next: (tasks) => this.allTasks.set(tasks),
      error: (err) => this.errorMessage.set(err?.error?.detail || 'Failed to load tasks.')
    });
  }

  getColumnTasks(status: TaskItemStatus): TaskItem[] {
    return this.allTasks().filter(t => t.status === status);
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case 'Critical': return 'bg-rose-500/10 border border-rose-500/20 text-rose-400';
      case 'High': return 'bg-amber-500/10 border border-amber-500/20 text-amber-400';
      case 'Medium': return 'bg-blue-500/10 border border-blue-500/20 text-blue-400';
      case 'Low': return 'bg-slate-500/10 border border-slate-500/20 text-slate-400';
    }
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  onDrop(event: CdkDragDrop<TaskItem[]>, targetStatus: TaskItemStatus): void {
    if (event.previousContainer === event.container) {
      // Reorder inside same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Moving between columns
      const task = event.previousContainer.data[event.previousIndex];
      const previousStatus = task.status;

      // Optimistic transfer
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      task.status = targetStatus;

      // Persist to backend
      this.taskService.updateTaskStatus(task.id, targetStatus).subscribe({
        next: (updated) => {
          this.allTasks.update(list => list.map(t => t.id === updated.id ? updated : t));
        },
        error: (err) => {
          // Rollback on failure
          task.status = previousStatus;
          transferArrayItem(
            event.container.data,
            event.previousContainer.data,
            event.currentIndex,
            event.previousIndex
          );
          this.errorMessage.set(err?.error?.detail || 'Failed to update task status.');
        }
      });
    }
  }

  openDetailModal(task: TaskItem): void {
    this.selectedTask.set(task);
    this.taskComments.set([]);
    this.newCommentText.set('');
    this.editTaskForm.patchValue({
      title: task.title,
      description: task.description || '',
      status: task.status,
      priority: task.priority,
      assignedUserId: task.assignedUserId || null
    });
    this.loadComments(task.id);
  }

  loadComments(taskId: number): void {
    this.isLoadingComments.set(true);
    this.commentService.getComments(taskId).subscribe({
      next: (comments) => {
        this.taskComments.set(comments);
        this.isLoadingComments.set(false);
      },
      error: () => {
        this.isLoadingComments.set(false);
      }
    });
  }

  onAddComment(taskId: number): void {
    const text = this.newCommentText().trim();
    if (!text || this.isPostingComment()) return;

    this.isPostingComment.set(true);
    this.commentService.addComment(taskId, text).subscribe({
      next: (created) => {
        this.taskComments.update(list => [...list, created]);
        this.newCommentText.set('');
        this.isPostingComment.set(false);
        // Increment comment count in task list
        this.allTasks.update(list => list.map(t => t.id === taskId ? { ...t, commentCount: (t.commentCount || 0) + 1 } : t));
      },
      error: (err) => {
        this.isPostingComment.set(false);
        this.errorMessage.set(err?.error?.detail || 'Failed to post comment.');
      }
    });
  }

  onCreateTask(): void {
    if (this.createTaskForm.invalid) return;

    this.isSubmitting.set(true);
    const { title, description, priority, assignedUserId } = this.createTaskForm.getRawValue();

    this.taskService.createTask(this.project.id, {
      title: title!,
      description: description || undefined,
      priority: priority!,
      assignedUserId: assignedUserId ?? undefined
    }).subscribe({
      next: (created) => {
        this.allTasks.update(list => [created, ...list]);
        this.isSubmitting.set(false);
        this.showCreateModal.set(false);
        this.createTaskForm.reset({ priority: 'Medium', assignedUserId: null });
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.detail || 'Failed to create task.');
      }
    });
  }

  onUpdateTask(id: number): void {
    if (this.editTaskForm.invalid) return;

    this.isSubmitting.set(true);
    const { title, description, status, priority, assignedUserId } = this.editTaskForm.getRawValue();

    this.taskService.updateTask(id, {
      title: title!,
      description: description || undefined,
      status: status!,
      priority: priority!,
      assignedUserId: assignedUserId ?? undefined
    }).subscribe({
      next: (updated) => {
        this.allTasks.update(list => list.map(t => t.id === updated.id ? updated : t));
        this.isSubmitting.set(false);
        this.selectedTask.set(null);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.detail || 'Failed to update task.');
      }
    });
  }

  onDeleteTask(id: number): void {
    if (!confirm('Are you sure you want to delete this task?')) return;

    this.taskService.deleteTask(id).subscribe({
      next: () => {
        this.allTasks.update(list => list.filter(t => t.id !== id));
        this.selectedTask.set(null);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail || 'Failed to delete task.');
      }
    });
  }
}
