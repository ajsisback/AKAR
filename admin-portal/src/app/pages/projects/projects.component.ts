import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { RouterLink } from '@angular/router';
import { ProjectService, ProjectDto } from '../../core/services/project.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterLink],
  template: `
    <div class="page-header">
      <h1 class="page-title">{{ 'projects.title' | translate }}</h1>
      <a routerLink="/projects/new" class="btn btn-accent">{{ 'projects.createNew' | translate }}</a>
    </div>

    <div class="card" *ngIf="projects.length > 0">
      <table class="data-table">
        <thead>
          <tr>
            <th>{{ 'projects.name' | translate }}</th>
            <th>{{ 'projects.type' | translate }}</th>
            <th>{{ 'projects.city' | translate }}</th>
            <th>{{ 'projects.stage' | translate }}</th>
            <th>{{ 'projects.createdAt' | translate }}</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let p of projects">
            <td>{{ p.projectName }}</td>
            <td>{{ 'projectType.' + p.projectType | translate }}</td>
            <td>{{ p.city || '—' }}</td>
            <td>
              <span class="badge" [ngClass]="getBadgeClass(p.currentStage)">
                {{ 'currentStage.' + p.currentStage | translate }}
              </span>
            </td>
            <td>{{ p.createdAtUtc | date:'mediumDate' }}</td>
            <td>
              <a [routerLink]="['/projects', p.id]" class="btn btn-outline btn-sm">
                {{ 'actions.view' | translate }}
              </a>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="empty-state card" *ngIf="projects.length === 0 && !loading">
      <div class="empty-state-icon">📂</div>
      <div class="empty-state-title">{{ 'projects.noProjects' | translate }}</div>
      <div class="empty-state-text">{{ 'projects.createFirst' | translate }}</div>
      <a routerLink="/projects/new" class="btn btn-accent">{{ 'projects.createNew' | translate }}</a>
    </div>
  `,
  styles: [`
    .btn-sm { padding: 6px 14px; font-size: 0.8rem; }
  `]
})
export class ProjectsComponent implements OnInit {
  projects: ProjectDto[] = [];
  loading = true;

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    this.projectService.list().subscribe({
      next: (p) => { this.projects = p; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  getBadgeClass(stage: string): string {
    const map: Record<string, string> = {
      NotStarted: 'badge-not-started',
      Structural: 'badge-structural',
      Finishing: 'badge-finishing',
      Completed: 'badge-completed'
    };
    return map[stage] || 'badge-not-started';
  }
}
