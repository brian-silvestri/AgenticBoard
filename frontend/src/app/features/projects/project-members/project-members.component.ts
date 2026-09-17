import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProjectDetail, ProjectRole } from '../../../core/models/project.models';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-project-members',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="bg-slate-900/60 border border-slate-800/80 rounded-2xl p-6">
      <!-- Header -->
      <div class="flex items-center justify-between gap-4 mb-6">
        <div>
          <h3 class="text-base font-bold text-white">Project Members ({{ project.members.length }})</h3>
          <p class="text-xs text-slate-400 mt-0.5">Collaborators who have access to this board and tasks</p>
        </div>

        @if (project.myRole === 'Owner') {
          <button
            (click)="showAddModal.set(true)"
            class="px-3.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-xs shadow-md shadow-indigo-600/20 transition-all flex items-center gap-1.5"
          >
            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span>Add Member</span>
          </button>
        }
      </div>

      <!-- Error banner if any -->
      @if (errorMessage()) {
        <div class="mb-4 p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-xs flex items-center justify-between">
          <span>{{ errorMessage() }}</span>
          <button (click)="errorMessage.set(null)" class="text-rose-400 hover:text-white">&times;</button>
        </div>
      }

      <!-- Members Table -->
      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead>
            <tr class="border-b border-slate-800 text-slate-400 uppercase tracking-wider">
              <th class="pb-3 font-semibold">User</th>
              <th class="pb-3 font-semibold">Role</th>
              <th class="pb-3 font-semibold hidden sm:table-cell">Joined</th>
              @if (project.myRole === 'Owner') {
                <th class="pb-3 font-semibold text-right">Actions</th>
              }
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-800/60">
            @for (member of project.members; track member.userId) {
              <tr class="hover:bg-slate-800/30 transition-colors">
                <td class="py-3.5 pr-4">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-slate-800 border border-slate-700 text-indigo-400 font-bold flex items-center justify-center text-xs">
                      {{ getInitials(member.fullName) }}
                    </div>
                    <div>
                      <div class="font-semibold text-white">{{ member.fullName }}</div>
                      <div class="text-slate-500 text-[11px]">{{ member.email }}</div>
                    </div>
                  </div>
                </td>
                <td class="py-3.5 pr-4">
                  <span
                    [class]="member.role === 'Owner' 
                      ? 'px-2 py-0.5 rounded-full text-[10px] font-semibold bg-indigo-500/10 border border-indigo-500/20 text-indigo-400' 
                      : 'px-2 py-0.5 rounded-full text-[10px] font-semibold bg-slate-800 border border-slate-700 text-slate-300'"
                  >
                    {{ member.role }}
                  </span>
                </td>
                <td class="py-3.5 pr-4 text-slate-400 hidden sm:table-cell">
                  {{ member.joinedAt | date:'mediumDate' }}
                </td>
                @if (project.myRole === 'Owner') {
                  <td class="py-3.5 text-right">
                    @if (member.role !== 'Owner' || getOwnerCount() > 1) {
                      <button
                        (click)="onRemoveMember(member.userId)"
                        title="Remove member"
                        class="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
                      >
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    }
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Add Member Modal -->
      @if (showAddModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
          <div class="bg-slate-900 border border-slate-800 rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h3 class="text-base font-bold text-white mb-1">Add Team Member</h3>
            <p class="text-slate-400 text-xs mb-4">Invite an existing user to collaborate on {{ project.name }}</p>

            <form [formGroup]="addMemberForm" (ngSubmit)="onAddMember()" class="space-y-4">
              <div>
                <label for="m-email" class="block text-xs font-semibold uppercase text-slate-300 mb-1">User Email</label>
                <input
                  id="m-email"
                  type="email"
                  formControlName="email"
                  placeholder="colleague@example.com"
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label for="m-role" class="block text-xs font-semibold uppercase text-slate-300 mb-1">Project Role</label>
                <select
                  id="m-role"
                  formControlName="role"
                  class="w-full px-3.5 py-2 rounded-xl bg-slate-950 border border-slate-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="Member">Member (can create, edit, comment on tasks)</option>
                  <option value="Owner">Owner (full administration permissions)</option>
                </select>
              </div>

              <div class="flex items-center justify-end gap-3 pt-3">
                <button
                  type="button"
                  (click)="showAddModal.set(false)"
                  class="px-4 py-2 rounded-xl border border-slate-700 text-slate-300 text-xs font-semibold hover:bg-slate-800 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  [disabled]="addMemberForm.invalid || isSubmitting()"
                  class="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 disabled:opacity-50 transition-all"
                >
                  {{ isSubmitting() ? 'Adding...' : 'Add Member' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `
})
export class ProjectMembersComponent {
  @Input({ required: true }) project!: ProjectDetail;

  private projectService = inject(ProjectService);
  private fb = inject(FormBuilder);

  showAddModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  addMemberForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    role: ['Member' as ProjectRole, [Validators.required]]
  });

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  getOwnerCount(): number {
    return this.project.members.filter(m => m.role === 'Owner').length;
  }

  onAddMember(): void {
    if (this.addMemberForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { email, role } = this.addMemberForm.getRawValue();

    this.projectService.addMember(this.project.id, { email: email!, role: role! }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showAddModal.set(false);
        this.addMemberForm.reset({ role: 'Member' });
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.detail || 'Failed to add member.');
      }
    });
  }

  onRemoveMember(userId: number): void {
    if (!confirm('Are you sure you want to remove this member from the project?')) return;

    this.errorMessage.set(null);
    this.projectService.removeMember(this.project.id, userId).subscribe({
      error: (err) => {
        this.errorMessage.set(err?.error?.detail || 'Failed to remove member.');
      }
    });
  }
}
