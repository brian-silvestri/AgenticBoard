import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Welcome Header -->
      <div class="bg-gradient-to-r from-slate-900 to-indigo-950/40 border border-slate-800 rounded-2xl p-6 sm:p-8 mb-8 relative overflow-hidden">
        <div class="relative z-10">
          <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 text-xs font-semibold uppercase tracking-wider mb-3">
            <span class="w-2 h-2 rounded-full bg-indigo-400 animate-pulse"></span>
            Spec-Driven &bull; AI-First
          </div>
          <h1 class="text-2xl sm:text-3xl font-bold text-white tracking-tight">
            Hello, {{ authService.currentUser()?.fullName || 'Developer' }}
          </h1>
          <p class="text-slate-400 text-sm max-w-2xl mt-1.5 leading-relaxed">
            Welcome to your AgenticBoard workspace. Here you can collaborate on agile projects, prioritize tasks on Kanban boards, and track transparent audit trails.
          </p>

          <div class="flex items-center gap-3 mt-6">
            <a
              routerLink="/projects"
              class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-xs shadow-lg shadow-indigo-600/30 transition-all"
            >
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
              </svg>
              <span>Explore Projects</span>
            </a>
          </div>
        </div>
      </div>

      <!-- Quick Metrics Grid -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <div class="bg-slate-900/60 border border-slate-800/80 rounded-xl p-5">
          <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Architecture</div>
          <div class="text-xl font-bold text-white">Clean Architecture</div>
          <p class="text-xs text-slate-500 mt-1">.NET 8 &bull; EF Core &bull; SQL Server</p>
        </div>

        <div class="bg-slate-900/60 border border-slate-800/80 rounded-xl p-5">
          <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Frontend Core</div>
          <div class="text-xl font-bold text-white">Angular 20 Standalone</div>
          <p class="text-xs text-slate-500 mt-1">Signals &bull; Reactive Forms &bull; Tailwind</p>
        </div>

        <div class="bg-slate-900/60 border border-slate-800/80 rounded-xl p-5">
          <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Workflow</div>
          <div class="text-xl font-bold text-white">Spec-Driven Development</div>
          <p class="text-xs text-slate-500 mt-1">Specs &bull; Plans &bull; Agentic &bull; Automated Tests</p>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent {
  authService = inject(AuthService);
}
