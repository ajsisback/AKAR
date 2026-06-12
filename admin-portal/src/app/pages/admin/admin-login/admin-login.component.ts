import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminAuthService } from '../../../core/services/admin-auth.service';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './admin-login.component.html',
  styles: [`
    .admin-login-container { min-height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f8f9fa; }
    .login-card { background: white; padding: 2.5rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
    .badge-read-only { background: #e9ecef; color: #495057; padding: 0.25rem 0.75rem; border-radius: 999px; font-size: 0.875rem; font-weight: 500; display: inline-block; margin-bottom: 1.5rem; }
    .form-group { margin-bottom: 1.5rem; }
    .form-label { display: block; margin-bottom: 0.5rem; font-weight: 500; }
    .form-control { width: 100%; padding: 0.75rem; border: 1px solid #ced4da; border-radius: 4px; }
    .btn-primary { width: 100%; padding: 0.75rem; background: #0d6efd; color: white; border: none; border-radius: 4px; font-weight: 500; cursor: pointer; }
    .btn-primary:disabled { opacity: 0.65; cursor: not-allowed; }
    .text-danger { color: #dc3545; font-size: 0.875rem; margin-top: 0.25rem; }
    .alert-danger { background: #f8d7da; color: #842029; padding: 1rem; border-radius: 4px; margin-bottom: 1.5rem; }
  `]
})
export class AdminLoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AdminAuthService,
    private router: Router,
    public languageService: LanguageService,
    private translateService: TranslateService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigate(['/admin/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        const key = err.error?.code === 'AUTH_INVALID_CREDENTIALS' 
          ? 'admin.invalidCredentials' 
          : 'errors.generic';
        this.errorMessage = this.translateService.instant(key);
      }
    });
  }

  isFieldInvalid(field: string): boolean {
    const control = this.loginForm.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
