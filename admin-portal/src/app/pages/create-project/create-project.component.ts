import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../core/services/project.service';

@Component({
  selector: 'app-create-project',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, RouterLink],
  template: `
    <div class="page-header">
      <h1 class="page-title">{{ 'nav.createProject' | translate }}</h1>
      <a routerLink="/projects" class="btn btn-outline">{{ 'actions.back' | translate }}</a>
    </div>

    <div class="card form-card">
      <div *ngIf="errorMessage" class="alert alert-error">{{ errorMessage }}</div>

      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label>{{ 'projects.name' | translate }} *</label>
          <input type="text" class="form-control" formControlName="projectName"
                 [class.invalid]="form.get('projectName')?.invalid && form.get('projectName')?.touched">
          <span class="error-text" *ngIf="form.get('projectName')?.hasError('required') && form.get('projectName')?.touched">
            {{ 'validation.required' | translate }}
          </span>
        </div>

        <div class="form-group">
          <label>{{ 'projects.type' | translate }} *</label>
          <select class="form-control" formControlName="projectType">
            <option value="">—</option>
            <option value="Villa">{{ 'projectType.Villa' | translate }}</option>
            <option value="Duplex">{{ 'projectType.Duplex' | translate }}</option>
            <option value="SmallBuilding">{{ 'projectType.SmallBuilding' | translate }}</option>
          </select>
          <span class="error-text" *ngIf="form.get('projectType')?.hasError('required') && form.get('projectType')?.touched">
            {{ 'validation.required' | translate }}
          </span>
        </div>

        <div class="form-group">
          <label>{{ 'projects.city' | translate }}</label>
          <input type="text" class="form-control" formControlName="city">
        </div>

        <div class="form-group">
          <label>{{ 'projects.location' | translate }}</label>
          <input type="text" class="form-control" formControlName="locationText">
        </div>

        <div class="form-group">
          <label>{{ 'projects.mapLink' | translate }}</label>
          <input type="url" class="form-control" formControlName="mapLink">
        </div>

        <div class="form-group">
          <label>{{ 'projects.stage' | translate }}</label>
          <select class="form-control" formControlName="currentStage">
            <option value="NotStarted">{{ 'currentStage.NotStarted' | translate }}</option>
            <option value="Structural">{{ 'currentStage.Structural' | translate }}</option>
            <option value="Finishing">{{ 'currentStage.Finishing' | translate }}</option>
            <option value="Completed">{{ 'currentStage.Completed' | translate }}</option>
          </select>
        </div>

        <div class="form-group">
          <label>{{ 'projects.imageUrl' | translate }}</label>
          <input type="url" class="form-control" formControlName="optionalImageUrl">
        </div>

        <button type="submit" class="btn btn-primary" [disabled]="form.invalid || loading">
          {{ 'actions.create' | translate }}
        </button>
      </form>
    </div>
  `,
  styles: [`
    .form-card { max-width: 600px; }
    .alert-error { background: rgba(255,59,48,0.1); border: 1px solid var(--danger); color: var(--danger); padding: 12px; border-radius: var(--radius-sm); margin-bottom: 20px; font-size: 0.85rem; }
    select.form-control { appearance: auto; }
  `]
})
export class CreateProjectComponent {
  form: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private router: Router,
    private translate: TranslateService
  ) {
    this.form = this.fb.group({
      projectName: ['', [Validators.required]],
      projectType: ['', [Validators.required]],
      city: [''],
      locationText: [''],
      mapLink: [''],
      currentStage: ['NotStarted'],
      optionalImageUrl: ['']
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    this.projectService.create(this.form.value).subscribe({
      next: (project) => { this.router.navigate(['/projects', project.id]); },
      error: (err) => {
        this.loading = false;
        const code = err.error?.title || 'generic';
        this.translate.get(`errors.${code}`).subscribe(msg => this.errorMessage = msg);
      }
    });
  }
}
