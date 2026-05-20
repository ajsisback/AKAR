import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProjectService, ProjectDto } from '../../core/services/project.service';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterLink],
  template: `
    <div class="page-header">
      <h1 class="page-title">{{ 'projects.details' | translate }}</h1>
      <a routerLink="/projects" class="btn btn-outline">{{ 'actions.back' | translate }}</a>
    </div>

    <div class="card" *ngIf="project">
      <div class="detail-grid">
        <div class="detail-item">
          <label>{{ 'projects.name' | translate }}</label>
          <div class="value">{{ project.projectName }}</div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.type' | translate }}</label>
          <div class="value">{{ 'projectType.' + project.projectType | translate }}</div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.stage' | translate }}</label>
          <div class="value">
            <span class="badge" [ngClass]="getBadgeClass(project.currentStage)">
              {{ 'currentStage.' + project.currentStage | translate }}
            </span>
          </div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.city' | translate }}</label>
          <div class="value">{{ project.city || '—' }}</div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.location' | translate }}</label>
          <div class="value">{{ project.locationText || '—' }}</div>
        </div>
        <div class="detail-item" *ngIf="project.mapLink">
          <label>{{ 'projects.mapLink' | translate }}</label>
          <div class="value"><a [href]="project.mapLink" target="_blank" class="link">{{ project.mapLink }}</a></div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.createdAt' | translate }}</label>
          <div class="value">{{ project.createdAtUtc | date:'medium' }}</div>
        </div>
        <div class="detail-item">
          <label>{{ 'projects.updatedAt' | translate }}</label>
          <div class="value">{{ project.updatedAtUtc | date:'medium' }}</div>
        </div>
      </div>
    </div>

    <div class="empty-state card" *ngIf="!project && !loading">
      <div class="empty-state-icon">❌</div>
      <div class="empty-state-title">{{ 'errors.PROJECT_NOT_FOUND' | translate }}</div>
    </div>
  `,
  styles: [`
    .link { color: var(--accent); text-decoration: none; word-break: break-all; }
    .link:hover { text-decoration: underline; }
  `]
})
export class ProjectDetailsComponent implements OnInit {
  project: ProjectDto | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.projectService.getById(id).subscribe({
        next: (p) => { this.project = p; this.loading = false; },
        error: () => { this.loading = false; }
      });
    } else {
      this.loading = false;
    }
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
