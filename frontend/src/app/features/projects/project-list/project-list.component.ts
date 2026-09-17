import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header & Action -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 class="text-2xl sm:text-3xl font-bold text-white tracking-tight">Your Projects</h1>
          <p class="text-slate-400 text-sm mt-1">Manage workspaces, boards, and collaborate with your team</p>
        </div>

        <button
          (click)="showCreateModal.set(true)"
          class="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm shadow-lg shadow-indigo-600/30 transition-all self-start sm:self-auto"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          <span>New Project</span>
        </button>
      </div>

      <!-- Projects Grid -->
      @if (projectService.isLoading() && projectService.projects().length === 0) {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (item of [1,2,3]; track item) {
            <div class="animate-pulse bg-slate-900/60 border border-slate-800/60 rounded-2xl p-6 h-48"></div>
          }
        </div>
      } @else if (projectService.projects().length === 0) {
        <div class="text-center py-16 bg-slate-900/40 border border-slate-800/80 rounded-2xl p-8">
          <div class="w-12 h-12 rounded-xl bg-slate-800 text-slate-400 flex items-center justify-center mx-auto mb-3">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
            </svg>
          </div>
          <h3 class="text-lg font-semibold text-white">No projects found</h3>
          <p class="text-slate-400 text-sm mt-1 max-w-sm mx-auto">You are not a member of any projects yet. Create your first project to get started.</p>
          <button
            (click)="showCreateModal.set(true)"
            class="mt-4 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 transition-all"
          >
            Create First Project
          </button>
        </div>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (project of projectService.projects(); track project.id) {
            <div class="bg-slate-900/80 border border-slate-800/80 hover:border-slate-700/80 rounded-2xl p-6 shadow-lg transition-all flex flex-col justify-between group">
              <div>
                <div class="flex items-start justify-between gap-2 mb-3">
                  <span
                    [class]="project.myRole === 'Owner' 
                      ? 'px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-indigo-500/10 border border-indigo-500/20 text-indigo-400' 
                      : 'px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-slate-800 border border-slate-700 text-slate-300'"
                  >
                    {{ project.myRole }}
                  </span>

                  <span class="text-xs text-slate-500">
                    Created by {{ project.createdByName }}
                  </span>
                </div>

                <h3 class="text-lg font-bold text-white group-hover:text-indigo-400 transition-colors">
                  {{ project.name }}
                </h3>
                <p class="text-slate-400 text-xs mt-1.5 line-clamp-2 leading-relaxed">
                  {{ project.description || 'No description provided.' }}
                </p>
              </div>

              <div class="mt-6 pt-4 border-t border-slate-800/80 flex items-center justify-between">
                <div class="flex items-center gap-3 text-xs text-slate-400">
                  <span class="flex items-center gap-1">
                    <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                    </svg>
                    {{ project.memberCount }} {{ project.memberCount === 1 ? 'member' : 'members' }}
                  </span>
                  <span>&bull;</span>
                  <span class="flex items-center gap-1">
                    <svg class="w-3.5 h-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                    {{ project.taskCount }} tasks
                  </span>
                </div>

                <a
                  [routerLink]="['/projects', project.id]"
                  class="inline-flex items-center gap-1 text-xs font-semibold text-indigo-400 hover:text-indigo-300 transition-colors"
                >
                  <span>Open</span>
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </a>
              </div>
            </div>
          }
        </div>
      }

      <!-- Create Project Modal -->
      @if (showCreateModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
          <div class="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-md p-6 shadow-2xl relative">
            <h3 class="text-lg font-bold text-white mb-1">Create New Project</h3>
            <p class="text-slate-400 text-xs mb-4">Set up a workspace for tasks and team collaboration</p>

            <form [formGroup]="createForm" (ngSubmit)="onCreateProject()" class="space-y-4">
              <div>
                <label for="p-name" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Project Name</label>
                <input
                  id="p-name"
                  type="text"
                  formControlName="name"
                  placeholder="e.g. NextGen Microservices"
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label for="p-desc" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Description (Optional)</label>
                <textarea
                  id="p-desc"
                  rows="3"
                  formControlName="description"
                  placeholder="Briefly describe the purpose of this project..."
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                ></textarea>
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
                  [disabled]="createForm.invalid || isSubmitting()"
                  class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 disabled:opacity-50 transition-all"
                >
                  {{ isSubmitting() ? 'Creating...' : 'Create Project' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `
})
export class ProjectListComponent implements OnInit {
  projectService = inject(ProjectService);
  private fb = inject(FormBuilder);

  showCreateModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  createForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    description: ['', [Validators.maxLength(1000)]]
  });

  ngOnInit(): void {
    this.projectService.getMyProjects().subscribe();
  }

  onCreateProject(): void {
    if (this.createForm.invalid) return;

    this.isSubmitting.set(true);
    const { name, description } = this.createForm.getRawValue();

    this.projectService.createProject({ name: name!, description: description || undefined }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showCreateModal.set(false);
        this.createForm.reset();
      },
      error: () => {
        this.isSubmitting.set(false);
      }
    });
  }
}
