import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProjectService, ProjectDto } from '../../core/services/project.service';
import {
  DocumentVaultService,
  FolderDto,
  FileDto,
  TrashDto
} from '../../core/services/document-vault.service';
import { ReadyContractsService, ContractDto } from '../../core/services/ready-contracts.service';
import { TimelineService, TimelineEventDto } from '../../core/services/timeline.service';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [CommonModule, TranslateModule, FormsModule, RouterLink],
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

    <!-- ═══════════════════════════════════════════════════════
         PROJECT TIMELINE — Admin Support View (Read-only)
         ═══════════════════════════════════════════════════════ -->
    <div class="timeline-section" *ngIf="project">

      <div class="vault-header">
        <h2 class="section-title">
          <span class="section-icon">📅</span>
          {{ 'timeline.title' | translate }}
        </h2>
        <span class="badge badge-readonly">{{ 'timeline.readOnly' | translate }}</span>
      </div>

      <!-- Stage Progress Card -->
      <div class="card stage-progress-card">
        <div class="stage-header-row">
          <div class="stage-info">
            <label>{{ 'timeline.currentStage' | translate }}</label>
            <div class="stage-value">
              <span class="badge" [ngClass]="getBadgeClass(project.currentStage)">
                {{ 'currentStage.' + project.currentStage | translate }}
              </span>
            </div>
          </div>
        </div>
        <div class="stage-progress-bar">
          <div *ngFor="let s of stages; let i = index" class="stage-step" [class.active]="i <= getStageIndex(project.currentStage)" [class.current]="i === getStageIndex(project.currentStage)">
            <div class="stage-bar"></div>
            <span class="stage-label">{{ 'currentStage.' + s | translate }}</span>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="timeline-filters">
        <div class="filter-group">
          <label>{{ 'timeline.filterByStage' | translate }}</label>
          <select [(ngModel)]="timelineFilterStage" (ngModelChange)="loadTimeline()">
            <option value="">{{ 'timeline.allStages' | translate }}</option>
            <option *ngFor="let s of stages" [value]="s">{{ 'currentStage.' + s | translate }}</option>
          </select>
        </div>
        <div class="filter-group">
          <label>{{ 'timeline.filterByType' | translate }}</label>
          <select [(ngModel)]="timelineFilterType" (ngModelChange)="loadTimeline()">
            <option value="">{{ 'timeline.allTypes' | translate }}</option>
            <option *ngFor="let t of eventTypes" [value]="t">{{ 'timeline.eventType.' + t | translate }}</option>
          </select>
        </div>
        <div class="filter-count">
          {{ timelineEvents.length }} {{ 'timeline.events' | translate }}
        </div>
      </div>

      <!-- Timeline Events Table -->
      <div class="card vault-card">

        <div class="empty-state" *ngIf="timelineEvents.length === 0 && !timelineLoading">
          <div class="empty-state-icon">📅</div>
          <div class="empty-state-title">{{ 'timeline.noEvents' | translate }}</div>
          <div class="empty-state-sub">{{ 'timeline.noEventsSub' | translate }}</div>
        </div>

        <table class="data-table" *ngIf="timelineEvents.length > 0">
          <thead>
            <tr>
              <th>{{ 'timeline.eventTitle' | translate }}</th>
              <th>{{ 'timeline.eventTypeCol' | translate }}</th>
              <th>{{ 'timeline.stage' | translate }}</th>
              <th>{{ 'timeline.source' | translate }}</th>
              <th>{{ 'timeline.eventDate' | translate }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let ev of timelineEvents">
              <td>
                <div class="event-title-cell">
                  <span class="event-icon">{{ getEventIcon(ev.eventType) }}</span>
                  <div>
                    <div class="event-title-text">{{ ev.title }}</div>
                    <div class="event-desc" *ngIf="ev.description">{{ ev.description }}</div>
                  </div>
                </div>
              </td>
              <td>
                <span class="badge" [ngClass]="getEventTypeBadge(ev.eventType)">
                  {{ 'timeline.eventType.' + ev.eventType | translate }}
                </span>
              </td>
              <td>
                <span class="badge" [ngClass]="getBadgeClass(ev.stage)">
                  {{ 'currentStage.' + ev.stage | translate }}
                </span>
              </td>
              <td>
                <span class="badge" [ngClass]="ev.isSystemGenerated ? 'badge-system-event' : 'badge-manual-event'">
                  {{ (ev.isSystemGenerated ? 'timeline.systemEvent' : 'timeline.manualEvent') | translate }}
                </span>
              </td>
              <td>{{ ev.eventDateUtc | date:'medium' }}</td>
            </tr>
          </tbody>
        </table>

      </div>

    </div>

    <!-- ═══════════════════════════════════════════════════════
         DOCUMENT VAULT — Admin Support View (Read-only)
         ═══════════════════════════════════════════════════════ -->
    <div class="vault-section" *ngIf="project">

      <div class="vault-header">
        <h2 class="section-title">
          <span class="section-icon">🗄️</span>
          {{ 'vault.title' | translate }}
        </h2>
        <span class="badge badge-readonly">{{ 'vault.readOnly' | translate }}</span>
      </div>

      <!-- Vault Tabs -->
      <div class="vault-tabs">
        <button class="vault-tab" [class.active]="vaultTab === 'folders'" (click)="vaultTab = 'folders'; selectedFolder = null; folderFiles = []">
          {{ 'vault.folders' | translate }}
        </button>
        <button class="vault-tab" [class.active]="vaultTab === 'trash'" (click)="vaultTab = 'trash'; loadTrash()">
          {{ 'vault.trash' | translate }}
        </button>
      </div>

      <!-- FOLDERS TAB -->
      <div class="vault-panel" *ngIf="vaultTab === 'folders'">

        <!-- Folder List -->
        <div class="card vault-card" *ngIf="!selectedFolder">
          <div class="empty-state" *ngIf="folders.length === 0 && !foldersLoading">
            <div class="empty-state-icon">📂</div>
            <div class="empty-state-title">{{ 'vault.noFolders' | translate }}</div>
          </div>

          <table class="data-table" *ngIf="folders.length > 0">
            <thead>
              <tr>
                <th>{{ 'vault.folderName' | translate }}</th>
                <th>{{ 'vault.folderType' | translate }}</th>
                <th>{{ 'vault.folderKind' | translate }}</th>
                <th>{{ 'vault.fileCount' | translate }}</th>
                <th>{{ 'vault.createdAt' | translate }}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let f of folders">
                <td>
                  <span class="folder-icon">{{ getFolderIcon(f.folderType) }}</span>
                  {{ f.folderName }}
                </td>
                <td><span class="badge badge-type">{{ f.folderType }}</span></td>
                <td>
                  <span class="badge" [ngClass]="f.isSystemFolder ? 'badge-system' : 'badge-custom'">
                    {{ (f.isSystemFolder ? 'vault.systemFolder' : 'vault.customFolder') | translate }}
                  </span>
                </td>
                <td class="num-cell">{{ f.fileCount }}</td>
                <td>{{ f.createdAtUtc | date:'mediumDate' }}</td>
                <td>
                  <button class="btn-sm btn-outline" (click)="selectFolder(f)">
                    {{ 'actions.view' | translate }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Folder File List -->
        <div class="card vault-card" *ngIf="selectedFolder">
          <div class="folder-detail-header">
            <button class="btn-sm btn-outline" (click)="selectedFolder = null; folderFiles = []">
              ← {{ 'actions.back' | translate }}
            </button>
            <h3>
              <span class="folder-icon">{{ getFolderIcon(selectedFolder.folderType) }}</span>
              {{ selectedFolder.folderName }}
            </h3>
          </div>

          <div class="empty-state" *ngIf="folderFiles.length === 0 && !filesLoading">
            <div class="empty-state-icon">📄</div>
            <div class="empty-state-title">{{ 'vault.noFiles' | translate }}</div>
          </div>

          <table class="data-table" *ngIf="folderFiles.length > 0">
            <thead>
              <tr>
                <th>{{ 'vault.fileName' | translate }}</th>
                <th>{{ 'vault.fileCategory' | translate }}</th>
                <th>{{ 'vault.contentType' | translate }}</th>
                <th>{{ 'vault.fileSize' | translate }}</th>
                <th>{{ 'vault.createdAt' | translate }}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let file of folderFiles">
                <td>
                  <span class="file-icon">{{ getFileIcon(file.category) }}</span>
                  {{ file.originalFileName }}
                </td>
                <td><span class="badge badge-category">{{ file.category || '—' }}</span></td>
                <td class="muted">{{ file.contentType }}</td>
                <td class="num-cell">{{ formatSize(file.sizeBytes) }}</td>
                <td>{{ file.createdAtUtc | date:'mediumDate' }}</td>
                <td>
                  <button class="btn-sm btn-outline" (click)="downloadFile(file)" [title]="'vault.download' | translate">
                    ⬇️
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- TRASH TAB -->
      <div class="vault-panel" *ngIf="vaultTab === 'trash'">
        <div class="card vault-card">

          <div class="empty-state" *ngIf="trashEmpty && !trashLoading">
            <div class="empty-state-icon">🗑️</div>
            <div class="empty-state-title">{{ 'vault.noDeletedItems' | translate }}</div>
          </div>

          <!-- Deleted Files -->
          <div *ngIf="trash && trash.deletedFiles.length > 0">
            <h4 class="trash-subtitle">{{ 'vault.deletedFiles' | translate }}</h4>
            <table class="data-table">
              <thead>
                <tr>
                  <th>{{ 'vault.fileName' | translate }}</th>
                  <th>{{ 'vault.fileCategory' | translate }}</th>
                  <th>{{ 'vault.contentType' | translate }}</th>
                  <th>{{ 'vault.fileSize' | translate }}</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let file of trash.deletedFiles">
                  <td>
                    <span class="file-icon">{{ getFileIcon(file.category) }}</span>
                    {{ file.originalFileName }}
                  </td>
                  <td><span class="badge badge-category">{{ file.category || '—' }}</span></td>
                  <td class="muted">{{ file.contentType }}</td>
                  <td class="num-cell">{{ formatSize(file.sizeBytes) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Deleted Folders -->
          <div *ngIf="trash && trash.deletedFolders.length > 0" class="trash-folders-section">
            <h4 class="trash-subtitle">{{ 'vault.deletedFolders' | translate }}</h4>
            <table class="data-table">
              <thead>
                <tr>
                  <th>{{ 'vault.folderName' | translate }}</th>
                  <th>{{ 'vault.folderType' | translate }}</th>
                  <th>{{ 'vault.folderKind' | translate }}</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let f of trash.deletedFolders">
                  <td>
                    <span class="folder-icon">{{ getFolderIcon(f.folderType) }}</span>
                    {{ f.folderName }}
                  </td>
                  <td><span class="badge badge-type">{{ f.folderType }}</span></td>
                  <td>
                    <span class="badge" [ngClass]="f.isSystemFolder ? 'badge-system' : 'badge-custom'">
                      {{ (f.isSystemFolder ? 'vault.systemFolder' : 'vault.customFolder') | translate }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

        </div>
      </div>

    </div>

    <!-- ═══════════════════════════════════════════════════════
         READY CONTRACTS — Admin Support View (Read-only)
         ═══════════════════════════════════════════════════════ -->
    <div class="contracts-section" *ngIf="project">

      <div class="vault-header">
        <h2 class="section-title">
          <span class="section-icon">📑</span>
          {{ 'contracts.title' | translate }}
        </h2>
        <span class="badge badge-readonly">{{ 'contracts.readOnly' | translate }}</span>
      </div>

      <!-- Contract List -->
      <div class="card vault-card" *ngIf="!selectedContract">

        <div class="empty-state" *ngIf="contracts.length === 0 && !contractsLoading">
          <div class="empty-state-icon">📑</div>
          <div class="empty-state-title">{{ 'contracts.noContracts' | translate }}</div>
        </div>

        <table class="data-table" *ngIf="contracts.length > 0">
          <thead>
            <tr>
              <th>{{ 'contracts.contractTitle' | translate }}</th>
              <th>{{ 'contracts.contractType' | translate }}</th>
              <th>{{ 'contracts.partyName' | translate }}</th>
              <th>{{ 'contracts.contractValue' | translate }}</th>
              <th>{{ 'contracts.status' | translate }}</th>
              <th>{{ 'contracts.pdf' | translate }}</th>
              <th>{{ 'contracts.createdAt' | translate }}</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let c of contracts">
              <td>{{ c.contractTitle }}</td>
              <td><span class="badge badge-type">{{ c.contractType || '—' }}</span></td>
              <td>{{ c.partyName || '—' }}</td>
              <td class="num-cell">{{ c.contractValue != null ? c.contractValue + ' ' + ('contracts.sar' | translate) : '—' }}</td>
              <td>
                <span class="badge" [ngClass]="getContractStatusBadge(c.status)">
                  {{ getContractStatusLabel(c.status) | translate }}
                </span>
              </td>
              <td>
                <span *ngIf="c.pdfFileId" class="badge badge-pdf-yes">{{ 'contracts.pdfAvailable' | translate }}</span>
                <span *ngIf="!c.pdfFileId" class="muted">{{ 'contracts.pdfNotAvailable' | translate }}</span>
              </td>
              <td>{{ c.createdAtUtc | date:'mediumDate' }}</td>
              <td>
                <button class="btn-sm btn-outline" (click)="selectContract(c)">
                  {{ 'actions.view' | translate }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Contract Detail Panel -->
      <div class="card vault-card" *ngIf="selectedContract">
        <div class="folder-detail-header">
          <button class="btn-sm btn-outline" (click)="selectedContract = null">
            ← {{ 'actions.back' | translate }}
          </button>
          <h3>{{ selectedContract.contractTitle }}</h3>
          <span class="badge" [ngClass]="getContractStatusBadge(selectedContract.status)">
            {{ getContractStatusLabel(selectedContract.status) | translate }}
          </span>
        </div>

        <div class="detail-grid">
          <div class="detail-item">
            <label>{{ 'contracts.template' | translate }}</label>
            <div class="value">{{ selectedContract.templateNameAr || selectedContract.templateNameEn || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.contractType' | translate }}</label>
            <div class="value">{{ selectedContract.contractType || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.partyName' | translate }}</label>
            <div class="value">{{ selectedContract.partyName || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.partyPhone' | translate }}</label>
            <div class="value">{{ selectedContract.partyPhone || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.partyNationalId' | translate }}</label>
            <div class="value">{{ selectedContract.partyNationalId || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.contractValue' | translate }}</label>
            <div class="value">{{ selectedContract.contractValue != null ? selectedContract.contractValue + ' ' + ('contracts.sar' | translate) : '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.startDate' | translate }}</label>
            <div class="value">{{ selectedContract.startDate ? (selectedContract.startDate | date:'mediumDate') : '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.endDate' | translate }}</label>
            <div class="value">{{ selectedContract.endDate ? (selectedContract.endDate | date:'mediumDate') : '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'contracts.createdAt' | translate }}</label>
            <div class="value">{{ selectedContract.createdAtUtc | date:'medium' }}</div>
          </div>
        </div>

        <!-- Contract Data Sections -->
        <div class="contract-data-sections" *ngIf="contractData">
          <div class="contract-data-section" *ngIf="contractData.scopeOfWork">
            <h4>{{ 'contracts.scopeOfWork' | translate }}</h4>
            <p>{{ contractData.scopeOfWork }}</p>
          </div>
          <div class="contract-data-section" *ngIf="contractData.paymentTerms">
            <h4>{{ 'contracts.paymentTerms' | translate }}</h4>
            <p>{{ contractData.paymentTerms }}</p>
          </div>
          <div class="contract-data-section" *ngIf="contractData.ownerObligations">
            <h4>{{ 'contracts.ownerObligations' | translate }}</h4>
            <p>{{ contractData.ownerObligations }}</p>
          </div>
          <div class="contract-data-section" *ngIf="contractData.contractorObligations">
            <h4>{{ 'contracts.contractorObligations' | translate }}</h4>
            <p>{{ contractData.contractorObligations }}</p>
          </div>
          <div class="contract-data-section" *ngIf="contractData.notes">
            <h4>{{ 'contracts.notes' | translate }}</h4>
            <p>{{ contractData.notes }}</p>
          </div>
        </div>

        <!-- Disclaimer -->
        <div class="contract-disclaimer">
          <strong>⚠️ {{ 'contracts.disclaimer' | translate }}</strong>
          <p>{{ 'contracts.disclaimerText' | translate }}</p>
        </div>

        <!-- PDF Download -->
        <div class="contract-pdf-actions" *ngIf="selectedContract.pdfFileId">
          <button class="btn btn-accent" (click)="downloadContractPdf()">
            ⬇️ {{ 'contracts.downloadPdf' | translate }}
          </button>
        </div>
      </div>

    </div>
  `,
  styles: [`
    .link { color: var(--accent); text-decoration: none; word-break: break-all; }
    .link:hover { text-decoration: underline; }

    /* Vault section */
    .vault-section { margin-top: 32px; }

    .vault-header {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 20px;
    }

    .section-title {
      font-size: 1.25rem; font-weight: 700; color: var(--text-primary);
      display: flex; align-items: center; gap: 8px;
    }
    .section-icon { font-size: 1.4rem; }

    .badge-readonly {
      background: rgba(139,149,165,0.15); color: var(--text-muted);
      font-size: 0.72rem; padding: 4px 10px; border-radius: 12px;
    }

    /* Tabs */
    .vault-tabs {
      display: flex; gap: 4px; margin-bottom: 16px;
      border-bottom: 1px solid var(--border); padding-bottom: 0;
    }

    .vault-tab {
      background: transparent; border: none; border-bottom: 2px solid transparent;
      color: var(--text-secondary); padding: 10px 20px; font-size: 0.9rem;
      cursor: pointer; transition: all 0.2s; font-family: inherit;
    }
    .vault-tab:hover { color: var(--text-primary); }
    .vault-tab.active { color: var(--accent); border-bottom-color: var(--accent); }

    /* Vault card */
    .vault-card { margin-bottom: 16px; }

    /* Folder detail header */
    .folder-detail-header {
      display: flex; align-items: center; gap: 16px; margin-bottom: 16px;
    }
    .folder-detail-header h3 {
      font-size: 1.1rem; font-weight: 600; display: flex; align-items: center; gap: 8px;
    }

    /* Icons */
    .folder-icon { font-size: 1.1rem; }
    .file-icon { font-size: 1rem; }

    /* Badges */
    .badge-system { background: rgba(27,77,62,0.2); color: var(--primary-light); }
    .badge-custom { background: rgba(212,168,67,0.2); color: var(--accent); }
    .badge-type { background: rgba(139,149,165,0.12); color: var(--text-secondary); font-size: 0.72rem; }
    .badge-category { background: rgba(212,168,67,0.12); color: var(--accent-light); font-size: 0.72rem; }

    /* Cells */
    .num-cell { font-variant-numeric: tabular-nums; }
    .muted { color: var(--text-muted); font-size: 0.85rem; }

    /* Small button */
    .btn-sm {
      padding: 6px 14px; font-size: 0.8rem; border-radius: 6px;
      cursor: pointer; transition: all 0.2s; font-family: inherit;
      background: transparent; border: 1px solid var(--border); color: var(--text-secondary);
    }
    .btn-sm:hover { border-color: var(--primary-light); color: var(--text-primary); }

    /* Trash */
    .trash-subtitle {
      font-size: 0.95rem; font-weight: 600; color: var(--text-secondary);
      margin: 16px 0 12px 0;
    }
    .trash-subtitle:first-child { margin-top: 0; }
    .trash-folders-section { margin-top: 24px; }

    /* Contracts section */
    .contracts-section { margin-top: 32px; }

    /* Timeline section */
    .timeline-section { margin-top: 32px; }

    .stage-progress-card { margin-bottom: 16px; }
    .stage-header-row { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; }
    .stage-info label {
      font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase;
      letter-spacing: 0.5px; margin-bottom: 4px; display: block;
    }
    .stage-value { margin-top: 4px; }

    .stage-progress-bar { display: flex; gap: 6px; }
    .stage-step { flex: 1; text-align: center; }
    .stage-bar {
      height: 6px; border-radius: 3px;
      background: var(--border); transition: all 0.3s;
    }
    .stage-step.active .stage-bar { background: var(--accent); }
    .stage-step.current .stage-bar { box-shadow: 0 0 8px rgba(212,168,67,0.4); }
    .stage-label {
      display: block; font-size: 0.7rem; color: var(--text-muted);
      margin-top: 4px;
    }
    .stage-step.active .stage-label { color: var(--text-primary); }
    .stage-step.current .stage-label { font-weight: 700; }

    .timeline-filters {
      display: flex; gap: 12px; align-items: flex-end;
      margin-bottom: 16px; flex-wrap: wrap;
    }
    .filter-group { display: flex; flex-direction: column; gap: 4px; }
    .filter-group label { font-size: 0.75rem; color: var(--text-muted); }
    .filter-group select {
      background: var(--bg-card); color: var(--text-primary);
      border: 1px solid var(--border); border-radius: 6px;
      padding: 6px 10px; font-size: 0.85rem; font-family: inherit;
      min-width: 160px;
    }
    .filter-group select:focus { border-color: var(--accent); outline: none; }
    .filter-count {
      font-size: 0.82rem; color: var(--text-muted);
      margin-inline-start: auto; padding: 6px 0;
    }

    .event-title-cell { display: flex; align-items: flex-start; gap: 8px; }
    .event-icon { font-size: 1.1rem; flex-shrink: 0; margin-top: 2px; }
    .event-title-text { font-weight: 500; font-size: 0.9rem; }
    .event-desc {
      font-size: 0.8rem; color: var(--text-muted);
      margin-top: 2px; max-width: 300px;
      white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
    }
    .empty-state-sub { font-size: 0.82rem; color: var(--text-muted); margin-top: 4px; }

    .badge-stage-changed { background: rgba(212,168,67,0.2); color: var(--accent); font-size: 0.72rem; }
    .badge-manual-note { background: rgba(27,77,62,0.2); color: var(--primary-light); font-size: 0.72rem; }
    .badge-file-uploaded { background: rgba(90,200,250,0.2); color: #5ac8fa; font-size: 0.72rem; }
    .badge-contract-created { background: rgba(46,160,67,0.2); color: #2ea043; font-size: 0.72rem; }
    .badge-contract-pdf { background: rgba(248,81,73,0.2); color: #f85149; font-size: 0.72rem; }
    .badge-follower-added { background: rgba(175,82,222,0.2); color: #af52de; font-size: 0.72rem; }
    .badge-follower-file { background: rgba(100,210,255,0.2); color: #64d2ff; font-size: 0.72rem; }
    .badge-system-event { background: rgba(139,149,165,0.2); color: var(--text-muted); font-size: 0.72rem; }
    .badge-manual-event { background: rgba(27,77,62,0.2); color: var(--primary-light); font-size: 0.72rem; }

    .badge-draft { background: rgba(212,168,67,0.2); color: var(--accent); }
    .badge-ready-pdf { background: rgba(27,77,62,0.2); color: var(--primary-light); }
    .badge-pdf-generated { background: rgba(46,160,67,0.2); color: #2ea043; }
    .badge-signed { background: rgba(63,185,80,0.2); color: #3fb950; }
    .badge-cancelled { background: rgba(248,81,73,0.2); color: #f85149; }
    .badge-pdf-yes { background: rgba(46,160,67,0.15); color: #2ea043; font-size: 0.72rem; }

    .contract-data-sections { margin-top: 20px; }
    .contract-data-section {
      margin-bottom: 16px; padding: 12px;
      background: rgba(139,149,165,0.06); border-radius: 8px;
    }
    .contract-data-section h4 {
      font-size: 0.85rem; font-weight: 600; color: var(--accent);
      margin-bottom: 6px;
    }
    .contract-data-section p {
      font-size: 0.9rem; color: var(--text-secondary);
      white-space: pre-wrap; margin: 0;
    }

    .contract-disclaimer {
      margin-top: 16px; padding: 12px;
      background: rgba(212,168,67,0.08);
      border: 1px solid rgba(212,168,67,0.25);
      border-radius: 8px;
    }
    .contract-disclaimer strong { color: var(--accent); font-size: 0.85rem; }
    .contract-disclaimer p { font-size: 0.82rem; color: var(--text-muted); margin: 6px 0 0 0; }

    .contract-pdf-actions { margin-top: 16px; }
    .btn-accent {
      background: var(--accent); color: var(--bg-dark);
      border: none; padding: 10px 20px; border-radius: 8px;
      font-size: 0.9rem; font-weight: 600; cursor: pointer;
      font-family: inherit; transition: opacity 0.2s;
    }
    .btn-accent:hover { opacity: 0.85; }
  `]
})
export class ProjectDetailsComponent implements OnInit {
  project: ProjectDto | null = null;
  loading = true;

  // Vault state
  vaultTab: 'folders' | 'trash' = 'folders';
  folders: FolderDto[] = [];
  foldersLoading = false;
  selectedFolder: FolderDto | null = null;
  folderFiles: FileDto[] = [];
  filesLoading = false;
  trash: TrashDto | null = null;
  trashLoading = false;

  // Contracts state
  contracts: ContractDto[] = [];
  contractsLoading = false;
  selectedContract: ContractDto | null = null;
  contractData: any = null;

  // Timeline state
  timelineEvents: TimelineEventDto[] = [];
  timelineLoading = false;
  timelineFilterStage = '';
  timelineFilterType = '';
  stages = ['NotStarted', 'Structural', 'Finishing', 'Completed'];
  eventTypes = [
    'StageChanged', 'ManualNote', 'FileUploaded', 'ContractCreated',
    'ContractPdfGenerated', 'FollowerAdded', 'FollowerFileUploaded'
  ];

  private projectId = '';

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private vaultService: DocumentVaultService,
    private contractsService: ReadyContractsService,
    private timelineService: TimelineService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.projectId = id;
      this.projectService.getById(id).subscribe({
        next: (p) => { this.project = p; this.loading = false; this.loadFolders(); this.loadContracts(); this.loadTimeline(); },
        error: () => { this.loading = false; }
      });
    } else {
      this.loading = false;
    }
  }

  // ── Vault logic ─────────────────────────────────

  loadFolders(): void {
    this.foldersLoading = true;
    this.vaultService.getProjectFolders(this.projectId).subscribe({
      next: (f) => { this.folders = f; this.foldersLoading = false; },
      error: () => { this.foldersLoading = false; }
    });
  }

  selectFolder(folder: FolderDto): void {
    this.selectedFolder = folder;
    this.filesLoading = true;
    this.vaultService.getFolderFiles(this.projectId, folder.id).subscribe({
      next: (files) => { this.folderFiles = files; this.filesLoading = false; },
      error: () => { this.filesLoading = false; }
    });
  }

  loadTrash(): void {
    this.trashLoading = true;
    this.vaultService.getProjectTrash(this.projectId).subscribe({
      next: (t) => { this.trash = t; this.trashLoading = false; },
      error: () => { this.trashLoading = false; }
    });
  }

  get trashEmpty(): boolean {
    if (!this.trash) return true;
    return this.trash.deletedFiles.length === 0 && this.trash.deletedFolders.length === 0;
  }

  downloadFile(file: FileDto): void {
    this.vaultService.downloadFile(this.projectId, file.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = file.originalFileName;
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
      }
    });
  }

  // ── Contracts logic ────────────────────────────

  loadContracts(): void {
    this.contractsLoading = true;
    this.contractsService.getProjectContracts(this.projectId).subscribe({
      next: (c) => { this.contracts = c; this.contractsLoading = false; },
      error: () => { this.contractsLoading = false; }
    });
  }

  selectContract(contract: ContractDto): void {
    this.selectedContract = contract;
    this.contractData = null;
    if (contract.contractDataJson) {
      try { this.contractData = JSON.parse(contract.contractDataJson); } catch {}
    }
  }

  downloadContractPdf(): void {
    if (!this.selectedContract?.pdfFileId) return;
    this.contractsService.downloadContractPdf(this.projectId, this.selectedContract.pdfFileId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = (this.selectedContract?.contractTitle || 'contract') + '.pdf';
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
      }
    });
  }

  getContractStatusBadge(status: string): string {
    const map: Record<string, string> = {
      Draft: 'badge-draft',
      ReadyForPdf: 'badge-ready-pdf',
      PdfGenerated: 'badge-pdf-generated',
      SignedUploaded: 'badge-signed',
      Cancelled: 'badge-cancelled'
    };
    return map[status] || 'badge-draft';
  }

  getContractStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Draft: 'contracts.statusDraft',
      ReadyForPdf: 'contracts.statusReadyForPdf',
      PdfGenerated: 'contracts.statusPdfGenerated',
      SignedUploaded: 'contracts.statusSignedUploaded',
      Cancelled: 'contracts.statusCancelled'
    };
    return map[status] || 'contracts.statusDraft';
  }

  // ── Timeline logic ─────────────────────────────

  loadTimeline(): void {
    this.timelineLoading = true;
    const filters: { stage?: string; eventType?: string } = {};
    if (this.timelineFilterStage) filters.stage = this.timelineFilterStage;
    if (this.timelineFilterType) filters.eventType = this.timelineFilterType;
    this.timelineService.getProjectTimeline(this.projectId, filters).subscribe({
      next: (events) => { this.timelineEvents = events; this.timelineLoading = false; },
      error: () => { this.timelineLoading = false; }
    });
  }

  getStageIndex(stage: string): number {
    return this.stages.indexOf(stage);
  }

  getEventIcon(type: string): string {
    const icons: Record<string, string> = {
      StageChanged: '🚩', ManualNote: '📝', FileUploaded: '📤',
      ContractCreated: '📄', ContractPdfGenerated: '📑',
      FollowerAdded: '👤', FollowerFileUploaded: '☁️'
    };
    return icons[type] || '📌';
  }

  getEventTypeBadge(type: string): string {
    const map: Record<string, string> = {
      StageChanged: 'badge-stage-changed',
      ManualNote: 'badge-manual-note',
      FileUploaded: 'badge-file-uploaded',
      ContractCreated: 'badge-contract-created',
      ContractPdfGenerated: 'badge-contract-pdf',
      FollowerAdded: 'badge-follower-added',
      FollowerFileUploaded: 'badge-follower-file'
    };
    return map[type] || 'badge-type';
  }

  // ── Helpers ─────────────────────────────────────

  getBadgeClass(stage: string): string {
    const map: Record<string, string> = {
      NotStarted: 'badge-not-started',
      Structural: 'badge-structural',
      Finishing: 'badge-finishing',
      Completed: 'badge-completed'
    };
    return map[stage] || 'badge-not-started';
  }

  getFolderIcon(type: string): string {
    const icons: Record<string, string> = {
      License: '📋', ProjectLocation: '📍', Contracts: '📑', Drawings: '📐',
      Photos: '📷', Videos: '🎬', Invoices: '🧾', Warranties: '🛡️',
      FollowersInbox: '📬', Trash: '🗑️', Custom: '📁'
    };
    return icons[type] || '📁';
  }

  getFileIcon(category: string): string {
    const icons: Record<string, string> = {
      Document: '📄', Image: '🖼️', Video: '🎥', Other: '📎'
    };
    return icons[category] || '📎';
  }

  formatSize(bytes: number): string {
    if (!bytes || bytes === 0) return '0 B';
    const units = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return (bytes / Math.pow(1024, i)).toFixed(i > 0 ? 1 : 0) + ' ' + units[i];
  }
}
