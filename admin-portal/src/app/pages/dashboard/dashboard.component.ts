import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';
import { ProjectService, DashboardDto } from '../../core/services/project.service';
import { AuthService } from '../../core/services/auth.service';
import { OwnerProfileService, OwnerProfileDto } from '../../core/services/owner-profile.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterLink],
  template: `
    <div class="page-header">
      <h1 class="page-title">{{ 'dashboard.title' | translate }}</h1>
    </div>

    <p class="welcome-text" *ngIf="ownerName">{{ 'dashboard.welcome' | translate }}, {{ ownerName }} 👋</p>

    <div class="empty-state card" *ngIf="loadingDash">
      <div class="spinner"></div>
      <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
    </div>

    <div class="empty-state card" *ngIf="errorDash">
      <div class="empty-state-icon">❌</div>
      <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
      <button class="btn btn-outline" (click)="loadDash()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
    </div>

    <div class="stat-grid" *ngIf="!loadingDash && !errorDash && dashboard && dashboard.totalProjects > 0">
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

    <div class="empty-state" *ngIf="!loadingDash && !errorDash && dashboard && dashboard.totalProjects === 0">
      <div class="empty-state-icon">🏗️</div>
      <div class="empty-state-title">{{ 'dashboard.noProjects' | translate }}</div>
      <div class="empty-state-text">{{ 'dashboard.createFirst' | translate }}</div>
      <a routerLink="/projects/new" class="btn btn-accent">{{ 'nav.createProject' | translate }}</a>
    </div>

    <!-- Owner Profile Support View -->
    <div class="empty-state card" *ngIf="loadingProfile" style="margin-top: 32px;">
      <div class="spinner"></div>
      <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
    </div>

    <div class="card" style="margin-top: 32px;" *ngIf="!loadingProfile && !errorProfile && profile">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
        <h2 class="section-title" style="font-size: 1.25rem; margin: 0;">
          👤 {{ 'profile.title' | translate }}
        </h2>
        <span class="badge" style="background: rgba(139,149,165,0.15); color: var(--text-muted); font-size: 0.72rem;">
          {{ 'profile.readOnly' | translate }}
        </span>
      </div>
      
      <div class="detail-grid" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px;">
        <div class="detail-item">
          <label style="font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;">{{ 'profile.fullName' | translate }}</label>
          <div class="value" style="margin-top: 4px;">{{ profile.fullName }}</div>
        </div>
        <div class="detail-item">
          <label style="font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;">{{ 'profile.email' | translate }}</label>
          <div class="value" style="margin-top: 4px;">{{ profile.email }}</div>
        </div>
        <div class="detail-item">
          <label style="font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;">{{ 'profile.phone' | translate }}</label>
          <div class="value" style="margin-top: 4px;">{{ profile.phone }}</div>
        </div>
        <div class="detail-item">
          <label style="font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;">{{ 'profile.createdAt' | translate }}</label>
          <div class="value" style="margin-top: 4px;">{{ profile.createdAtUtc | date:'medium' }}</div>
        </div>
        <div class="detail-item" *ngIf="profile.updatedAtUtc">
          <label style="font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;">{{ 'profile.updatedAt' | translate }}</label>
          <div class="value" style="margin-top: 4px;">{{ profile.updatedAtUtc | date:'medium' }}</div>
        </div>
      </div>
    </div>
    <div class="empty-state card" *ngIf="errorProfile" style="margin-top: 32px;">
      <div class="empty-state-icon">❌</div>
      <div class="empty-state-title">{{ 'profile.failedToLoad' | translate }}</div>
      <button class="btn btn-outline" (click)="loadProfile()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
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
  profile: OwnerProfileDto | null = null;
  loadingDash = true;
  errorDash = false;
  loadingProfile = true;
  errorProfile = false;

  constructor(
    private projectService: ProjectService,
    private authService: AuthService,
    private ownerProfileService: OwnerProfileService
  ) {}

  ngOnInit(): void {
    this.ownerName = this.authService.getOwner()?.fullName || '';
    this.loadDash();
    this.loadProfile();
  }

  loadDash(): void {
    this.loadingDash = true;
    this.errorDash = false;
    this.projectService.getDashboard().subscribe({
      next: (d) => { this.dashboard = d; this.loadingDash = false; },
      error: () => { this.loadingDash = false; this.errorDash = true; }
    });
  }

  loadProfile(): void {
    this.loadingProfile = true;
    this.errorProfile = false;
    this.ownerProfileService.getProfile().subscribe({
      next: (p) => { this.profile = p; this.loadingProfile = false; },
      error: () => { this.loadingProfile = false; this.errorProfile = true; }
    });
  }
}
