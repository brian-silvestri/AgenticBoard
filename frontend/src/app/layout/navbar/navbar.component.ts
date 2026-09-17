import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="bg-slate-900/90 backdrop-blur-md border-b border-slate-800 sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <!-- Brand & Nav Links -->
        <div class="flex items-center gap-8">
          <a routerLink="/dashboard" class="flex items-center gap-2.5 group">
            <div class="w-8 h-8 rounded-lg bg-gradient-to-tr from-indigo-600 to-violet-500 flex items-center justify-center shadow-md shadow-indigo-500/20 group-hover:scale-105 transition-transform">
              <span class="text-white font-black text-sm tracking-tight">AB</span>
            </div>
            <span class="font-bold text-white text-lg tracking-tight group-hover:text-indigo-300 transition-colors">AgenticBoard</span>
          </a>

          <nav class="hidden md:flex items-center gap-1">
            <a
              routerLink="/dashboard"
              routerLinkActive="bg-slate-800 text-white"
              [routerLinkActiveOptions]="{ exact: true }"
              class="px-3 py-1.5 rounded-lg text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800/60 transition-colors"
            >
              Dashboard
            </a>
            <a
              routerLink="/projects"
              routerLinkActive="bg-slate-800 text-white"
              class="px-3 py-1.5 rounded-lg text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800/60 transition-colors"
            >
              Projects
            </a>
          </nav>
        </div>

        <!-- User Controls -->
        <div class="flex items-center gap-4">
          @if (authService.currentUser(); as user) {
            <div class="flex items-center gap-3">
              <div class="hidden sm:flex flex-col text-right">
                <span class="text-xs font-semibold text-white leading-tight">{{ user.fullName }}</span>
                <span class="text-[11px] text-slate-400">{{ user.email }}</span>
              </div>
              
              <!-- Avatar Circle -->
              <div class="w-9 h-9 rounded-full bg-indigo-500/20 border border-indigo-500/40 text-indigo-300 flex items-center justify-center font-bold text-xs">
                {{ getInitials(user.fullName) }}
              </div>

              <!-- Logout Button -->
              <button
                (click)="logout()"
                title="Log out"
                class="p-2 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
              >
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          } @else {
            <div class="flex items-center gap-2">
              <a
                routerLink="/login"
                class="px-3.5 py-1.5 rounded-lg text-xs font-medium text-slate-300 hover:text-white hover:bg-slate-800 transition-colors"
              >
                Sign In
              </a>
              <a
                routerLink="/register"
                class="px-3.5 py-1.5 rounded-lg text-xs font-medium text-white bg-indigo-600 hover:bg-indigo-500 transition-colors"
              >
                Sign Up
              </a>
            </div>
          }
        </div>
      </div>
    </header>
  `
})
export class NavbarComponent {
  authService = inject(AuthService);

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  logout(): void {
    this.authService.logout();
  }
}
