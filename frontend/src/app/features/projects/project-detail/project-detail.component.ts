import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectMembersComponent } from '../project-members/project-members.component';
import { KanbanBoardComponent } from '../../kanban/kanban-board/kanban-board.component';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, ProjectMembersComponent, KanbanBoardComponent],
  template: `
    @if (projectService.isLoading() && !projectService.currentProject()) {
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div class="animate-pulse space-y-4">
          <div class="h-8 bg-slate-800 rounded w-1/4"></div>
          <div class="h-4 bg-slate-800 rounded w-1/2"></div>
        </div>
      </div>
    } @else if (projectService.currentProject(); as project) {
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- Project Header Card -->
        <div class="bg-slate-900/80 border border-slate-800/80 rounded-2xl p-6 mb-8 relative">
          <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <div class="flex items-center gap-3 mb-2">
                <a routerLink="/projects" class="text-xs text-slate-400 hover:text-white transition-colors flex items-center gap-1">
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                  </svg>
                  <span>Projects</span>
                </a>
                <span class="text-slate-600">/</span>
                <span class="text-xs font-semibold text-indigo-400">{{ project.name }}</span>
              </div>

              <div class="flex items-center gap-3">
                <h1 class="text-2xl sm:text-3xl font-bold text-white tracking-tight">{{ project.name }}</h1>
                <span
                  [class]="project.myRole === 'Owner' 
                    ? 'px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-500/10 border border-indigo-500/20 text-indigo-400' 
                    : 'px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-800 border border-slate-700 text-slate-300'"
                >
                  {{ project.myRole }}
                </span>
              </div>

              <p class="text-slate-400 text-xs sm:text-sm mt-2 max-w-3xl leading-relaxed">
                {{ project.description || 'No description provided for this project.' }}
              </p>
            </div>

            @if (project.myRole === 'Owner') {
              <button
                (click)="openEditModal(project.name, project.description)"
                class="px-3.5 py-2 rounded-xl border border-slate-700 hover:bg-slate-800 text-slate-300 hover:text-white text-xs font-semibold transition-colors flex items-center gap-1.5 self-start md:self-auto"
              >
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                <span>Edit Project</span>
              </button>
            }
          </div>

          <!-- Navigation Tabs -->
          <div class="flex items-center gap-2 mt-6 pt-4 border-t border-slate-800 overflow-x-auto">
            <button
              (click)="activeTab.set('board')"
              [class]="activeTab() === 'board'
                ? 'px-4 py-2 rounded-xl bg-indigo-600 text-white font-semibold text-xs shadow-md shadow-indigo-600/20'
                : 'px-4 py-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 text-xs font-semibold transition-colors'"
            >
              Kanban Board
            </button>

            <button
              (click)="activeTab.set('members')"
              [class]="activeTab() === 'members'
                ? 'px-4 py-2 rounded-xl bg-indigo-600 text-white font-semibold text-xs shadow-md shadow-indigo-600/20'
                : 'px-4 py-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 text-xs font-semibold transition-colors'"
            >
              Members ({{ project.members.length }})
            </button>

            <button
              (click)="activeTab.set('activity')"
              [class]="activeTab() === 'activity'
                ? 'px-4 py-2 rounded-xl bg-indigo-600 text-white font-semibold text-xs shadow-md shadow-indigo-600/20'
                : 'px-4 py-2 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 text-xs font-semibold transition-colors'"
            >
              Activity Trail
            </button>
          </div>
        </div>

        <!-- Tab Content -->
        @if (activeTab() === 'members') {
          <app-project-members [project]="project" />
        } @else if (activeTab() === 'board') {
          <app-kanban-board [project]="project" />
        } @else if (activeTab() === 'activity') {
          <div id="activity-container">
            <div class="text-center py-12 bg-slate-900/40 border border-slate-800/80 rounded-2xl p-8">
              <div class="w-12 h-12 rounded-xl bg-purple-500/10 text-purple-400 flex items-center justify-center mx-auto mb-3">
                <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h3 class="text-lg font-semibold text-white">Activity Trail</h3>
              <p class="text-slate-400 text-xs mt-1 max-w-sm mx-auto">Audit logging records all project events and state changes.</p>
            </div>
          </div>
        }

        <!-- Edit Project Modal -->
        @if (showEditModal()) {
          <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
            <div class="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-md p-6 shadow-2xl">
              <h3 class="text-base font-bold text-white mb-1">Edit Project Settings</h3>
              <p class="text-slate-400 text-xs mb-4">Update name or description for this workspace</p>

              <form [formGroup]="editForm" (ngSubmit)="onUpdateProject(project.id)" class="space-y-4">
                <div>
                  <label for="e-name" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Project Name</label>
                  <input
                    id="e-name"
                    type="text"
                    formControlName="name"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>

                <div>
                  <label for="e-desc" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Description</label>
                  <textarea
                    id="e-desc"
                    rows="3"
                    formControlName="description"
                    class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  ></textarea>
                </div>

                <div class="flex items-center justify-end gap-3 pt-3">
                  <button
                    type="button"
                    (click)="showEditModal.set(false)"
                    class="px-4 py-2 rounded-xl border border-slate-700 text-slate-300 text-xs font-semibold hover:bg-slate-800 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    [disabled]="editForm.invalid || isSubmitting()"
                    class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 disabled:opacity-50 transition-all"
                  >
                    {{ isSubmitting() ? 'Saving...' : 'Save Changes' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        }
      </div>
    }
  `
})
export class ProjectDetailComponent implements OnInit {
  projectService = inject(ProjectService);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  activeTab = signal<'board' | 'members' | 'activity'>('board');
  showEditModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  editForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    description: ['', [Validators.maxLength(1000)]]
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.projectService.getProjectById(id).subscribe();
    }
  }

  openEditModal(name: string, description?: string): void {
    this.editForm.patchValue({
      name,
      description: description || ''
    });
    this.showEditModal.set(true);
  }

  onUpdateProject(id: number): void {
    if (this.editForm.invalid) return;

    this.isSubmitting.set(true);
    const { name, description } = this.editForm.getRawValue();

    this.projectService.updateProject(id, { name: name!, description: description || undefined }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showEditModal.set(false);
      },
      error: () => {
        this.isSubmitting.set(false);
      }
    });
  }
}
