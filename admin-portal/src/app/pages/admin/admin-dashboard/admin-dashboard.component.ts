import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule, Router } from '@angular/router';
import { AdminAuthService } from '../../../core/services/admin-auth.service';
import { LanguageService } from '../../../core/services/language.service';
import { AdminApiService } from '../../../core/services/admin-api.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './admin-dashboard.component.html',
  styles: [`
    .admin-layout { min-height: 100vh; background-color: #f8f9fa; }
    .admin-navbar { background: white; padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
    .nav-links a { margin-right: 1.5rem; color: #495057; text-decoration: none; font-weight: 500; }
    .nav-links a.active { color: #0d6efd; }
    .badge-read-only { background: #e9ecef; color: #495057; padding: 0.25rem 0.75rem; border-radius: 999px; font-size: 0.75rem; font-weight: 600; margin-left: 1rem; }
    .admin-content { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .dashboard-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 1.5rem; margin-top: 2rem; }
    .stat-card { background: white; padding: 1.5rem; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
    .stat-title { color: #6c757d; font-size: 0.875rem; font-weight: 600; text-transform: uppercase; margin-bottom: 0.5rem; }
    .stat-value { font-size: 2rem; font-weight: 700; color: #212529; }
    .btn-link { background: none; border: none; color: #0d6efd; cursor: pointer; font-weight: 500; }
  `]
})
export class AdminDashboardComponent implements OnInit {
  adminName = '';
  role = '';
  totalOwners = 0;
  totalProjects = 0;
  isLoading = true;
  error = '';

  constructor(
    public languageService: LanguageService,
    private authService: AdminAuthService,
    private apiService: AdminApiService,
    private router: Router
  ) {
    const admin = this.authService.getAdmin();
    if (admin) {
      this.adminName = admin.fullName;
      this.role = admin.role;
    }
  }

  ngOnInit(): void {
    this.apiService.getOwners().subscribe({
      next: (res) => {
        this.totalOwners = res.count;
        this.apiService.getProjects().subscribe({
          next: (pRes) => {
            this.totalProjects = pRes.count;
            this.isLoading = false;
          },
          error: () => {
            this.error = 'admin.unableToLoad';
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.error = 'admin.unableToLoad';
        this.isLoading = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/admin/login']);
  }
}
