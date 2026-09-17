import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-950 px-4 py-12 relative overflow-hidden">
      <!-- Background Ambient Glow -->
      <div class="absolute -top-40 -right-40 w-96 h-96 bg-indigo-600/20 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-40 -left-40 w-96 h-96 bg-purple-600/20 rounded-full blur-3xl pointer-events-none"></div>

      <div class="w-full max-w-md relative z-10">
        <!-- Logo & Title -->
        <div class="text-center mb-8">
          <div class="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-500 shadow-lg shadow-indigo-500/30 mb-3">
            <span class="text-2xl font-black text-white tracking-tighter">AB</span>
          </div>
          <h1 class="text-2xl font-bold text-white tracking-tight">Create your Account</h1>
          <p class="text-slate-400 text-sm mt-1">Start managing projects with AI-assisted discipline</p>
        </div>

        <!-- Card Container -->
        <div class="bg-slate-900/80 backdrop-blur-xl border border-slate-800/80 rounded-2xl p-8 shadow-2xl">
          <!-- Error Alert -->
          @if (errorMessage()) {
            <div class="mb-5 p-3.5 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm flex items-start gap-2.5">
              <svg class="w-5 h-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
              </svg>
              <span>{{ errorMessage() }}</span>
            </div>
          }

          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-4">
            <!-- Full Name -->
            <div>
              <label for="fullName" class="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">Full Name</label>
              <input
                id="fullName"
                type="text"
                formControlName="fullName"
                placeholder="Alex Developer"
                class="w-full px-4 py-2.5 rounded-xl bg-slate-950/60 border border-slate-700/80 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all text-sm"
              />
              @if (registerForm.get('fullName')?.touched && registerForm.get('fullName')?.invalid) {
                <p class="text-rose-400 text-xs mt-1">Full name is required (min 2 characters).</p>
              }
            </div>

            <!-- Email -->
            <div>
              <label for="email" class="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">Email Address</label>
              <input
                id="email"
                type="email"
                formControlName="email"
                placeholder="name@company.com"
                class="w-full px-4 py-2.5 rounded-xl bg-slate-950/60 border border-slate-700/80 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all text-sm"
              />
              @if (registerForm.get('email')?.touched && registerForm.get('email')?.invalid) {
                <p class="text-rose-400 text-xs mt-1">Please enter a valid email address.</p>
              }
            </div>

            <!-- Password -->
            <div>
              <label for="password" class="block text-xs font-semibold uppercase tracking-wider text-slate-300 mb-1.5">Password</label>
              <input
                id="password"
                type="password"
                formControlName="password"
                placeholder="Minimum 8 characters"
                class="w-full px-4 py-2.5 rounded-xl bg-slate-950/60 border border-slate-700/80 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all text-sm"
              />
              <p class="text-slate-500 text-[11px] mt-1">Must be at least 8 characters with uppercase, lowercase, and a number.</p>
              @if (registerForm.get('password')?.touched && registerForm.get('password')?.invalid) {
                <p class="text-rose-400 text-xs mt-1">Password does not meet complexity requirements.</p>
              }
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              [disabled]="registerForm.invalid || isSubmitting()"
              class="w-full py-2.5 px-4 rounded-xl font-medium text-sm text-white bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg shadow-indigo-600/30 transition-all flex items-center justify-center gap-2 mt-2"
            >
              @if (isSubmitting()) {
                <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span>Creating account...</span>
              } @else {
                <span>Create account</span>
              }
            </button>
          </form>

          <!-- Login Link -->
          <div class="mt-6 text-center text-xs text-slate-400">
            Already have an account?
            <a routerLink="/login" class="text-indigo-400 hover:text-indigo-300 font-semibold transition-colors ml-1">Sign in here</a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  errorMessage = signal<string | null>(null);
  isSubmitting = signal<boolean>(false);

  registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { fullName, email, password } = this.registerForm.getRawValue();

    this.authService.register({ fullName: fullName!, email: email!, password: password! }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.detail || 'Registration failed. Please check your information.');
      }
    });
  }
}
