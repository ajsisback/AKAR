import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterLink],
  template: `
    <div class="auth-card card">
      <h2 class="auth-title">{{ 'auth.register' | translate }}</h2>
      <p class="auth-subtitle">{{ 'app.subtitle' | translate }}</p>

      <div *ngIf="errorMessage" class="alert alert-error">{{ errorMessage }}</div>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label>{{ 'auth.fullName' | translate }}</label>
          <input type="text" class="form-control" formControlName="fullName"
                 [class.invalid]="form.get('fullName')?.invalid && form.get('fullName')?.touched">
          <span class="error-text" *ngIf="form.get('fullName')?.hasError('required') && form.get('fullName')?.touched">
            {{ 'validation.required' | translate }}
          </span>
        </div>
        <div class="form-group">
          <label>{{ 'auth.email' | translate }}</label>
          <input type="email" class="form-control" formControlName="email"
                 [class.invalid]="form.get('email')?.invalid && form.get('email')?.touched">
          <span class="error-text" *ngIf="form.get('email')?.hasError('email') && form.get('email')?.touched">
            {{ 'validation.emailInvalid' | translate }}
          </span>
        </div>
        <div class="form-group">
          <label>{{ 'auth.phone' | translate }}</label>
          <input type="tel" class="form-control" formControlName="phone"
                 [class.invalid]="form.get('phone')?.invalid && form.get('phone')?.touched">
        </div>
        <div class="form-group">
          <label>{{ 'auth.password' | translate }}</label>
          <input type="password" class="form-control" formControlName="password"
                 [class.invalid]="form.get('password')?.invalid && form.get('password')?.touched">
          <span class="error-text" *ngIf="form.get('password')?.hasError('minlength') && form.get('password')?.touched">
            {{ 'validation.passwordMin' | translate }}
          </span>
        </div>
        <div class="form-group">
          <label>{{ 'auth.companyName' | translate }}</label>
          <input type="text" class="form-control" formControlName="companyName">
        </div>
        <button type="submit" class="btn btn-primary btn-full" [disabled]="form.invalid || loading">
          {{ 'auth.registerButton' | translate }}
        </button>
      </form>

      <p class="auth-link">
        {{ 'auth.hasAccount' | translate }}
        <a routerLink="/login">{{ 'auth.loginHere' | translate }}</a>
      </p>
    </div>
  `,
  styles: [`
    .auth-card { max-width: 420px; width: 100%; padding: 40px; }
    .auth-title { font-size: 1.5rem; margin-bottom: 8px; color: var(--accent); }
    .auth-subtitle { color: var(--text-muted); margin-bottom: 32px; font-size: 0.9rem; }
    .btn-full { width: 100%; }
    .auth-link { text-align: center; margin-top: 20px; color: var(--text-muted); font-size: 0.85rem; }
    .auth-link a { color: var(--accent); text-decoration: none; }
    .alert-error { background: rgba(255,59,48,0.1); border: 1px solid var(--danger); color: var(--danger); padding: 12px; border-radius: var(--radius-sm); margin-bottom: 20px; font-size: 0.85rem; }
  `]
})
export class RegisterComponent {
  form: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private translate: TranslateService
  ) {
    this.form = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      companyName: ['']
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    this.auth.register(this.form.value).subscribe({
      next: () => { this.router.navigate(['/dashboard']); },
      error: (err) => {
        this.loading = false;
        const code = err.error?.title || 'generic';
        this.translate.get(`errors.${code}`).subscribe(msg => this.errorMessage = msg);
      }
    });
  }
}
