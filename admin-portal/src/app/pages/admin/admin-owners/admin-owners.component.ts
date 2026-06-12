import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule, Router } from '@angular/router';
import { AdminAuthService } from '../../../core/services/admin-auth.service';
import { LanguageService } from '../../../core/services/language.service';
import { AdminApiService, AdminOwnerListItemDto } from '../../../core/services/admin-api.service';

@Component({
  selector: 'app-admin-owners',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './admin-owners.component.html',
  styles: [`
    .admin-layout { min-height: 100vh; background-color: #f8f9fa; }
    .admin-navbar { background: white; padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
    .nav-links a { margin-right: 1.5rem; color: #495057; text-decoration: none; font-weight: 500; }
    .nav-links a.active { color: #0d6efd; }
    .badge-read-only { background: #e9ecef; color: #495057; padding: 0.25rem 0.75rem; border-radius: 999px; font-size: 0.75rem; font-weight: 600; margin-left: 1rem; }
    .admin-content { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .table-card { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); overflow: hidden; margin-top: 2rem; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { padding: 1rem; text-align: left; border-bottom: 1px solid #dee2e6; }
    .table th { background: #f8f9fa; font-weight: 600; color: #495057; }
    .table tbody tr:hover { background-color: #f8f9fa; cursor: pointer; }
    .btn-link { background: none; border: none; color: #0d6efd; cursor: pointer; font-weight: 500; }
    .btn-sm { padding: 0.25rem 0.5rem; font-size: 0.875rem; border-radius: 4px; background: #e9ecef; color: #212529; text-decoration: none; }
    .btn-sm:hover { background: #dee2e6; }
  `]
})
export class AdminOwnersComponent implements OnInit {
  adminName = '';
  role = '';
  owners: AdminOwnerListItemDto[] = [];
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
      next: (owners) => {
        this.owners = owners;
        this.isLoading = false;
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
