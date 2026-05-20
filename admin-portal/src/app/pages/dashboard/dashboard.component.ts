import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';
import { ProjectService, DashboardDto } from '../../core/services/project.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterLink],
  template: `
    <div class="page-header">
      <h1 class="page-title">{{ 'dashboard.title' | translate }}</h1>
    </div>

    <p class="welcome-text" *ngIf="ownerName">{{ 'dashboard.welcome' | translate }}, {{ ownerName }} 👋</p>

    <div class="stat-grid" *ngIf="dashboard">
      <div class="stat-card">
        <div class="stat-value">{{ dashboard.totalProjects }}</div>
        <div class="stat-label">{{ 'dashboard.totalProjects' | translate }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value stat-not-started">{{ dashboard.notStartedCount }}</div>
        <div class="stat-label">{{ 'dashboard.notStarted' | translate }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value stat-structural">{{ dashboard.structuralCount }}</div>
        <div class="stat-label">{{ 'dashboard.structural' | translate }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value stat-finishing">{{ dashboard.finishingCount }}</div>
        <div class="stat-label">{{ 'dashboard.finishing' | translate }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value stat-completed">{{ dashboard.completedCount }}</div>
        <div class="stat-label">{{ 'dashboard.completed' | translate }}</div>
      </div>
    </div>

    <div class="empty-state" *ngIf="dashboard && dashboard.totalProjects === 0">
      <div class="empty-state-icon">🏗️</div>
      <div class="empty-state-title">{{ 'dashboard.noProjects' | translate }}</div>
      <div class="empty-state-text">{{ 'dashboard.createFirst' | translate }}</div>
      <a routerLink="/projects/new" class="btn btn-accent">{{ 'nav.createProject' | translate }}</a>
    </div>
  `,
  styles: [`
    .welcome-text { font-size: 1.1rem; color: var(--text-secondary); margin-bottom: 24px; }
    .stat-not-started { color: var(--text-secondary); }
    .stat-structural { color: var(--warning); }
    .stat-finishing { color: var(--primary-light); }
    .stat-completed { color: var(--success); }
  `]
})
export class DashboardComponent implements OnInit {
  dashboard: DashboardDto | null = null;
  ownerName = '';

  constructor(
    private projectService: ProjectService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.ownerName = this.authService.getOwner()?.fullName || '';
    this.projectService.getDashboard().subscribe({
      next: (d) => this.dashboard = d,
      error: () => {}
    });
  }
}
