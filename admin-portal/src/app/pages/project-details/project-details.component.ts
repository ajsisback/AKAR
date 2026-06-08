import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProjectService, ProjectDto, ProjectSettingsDto } from '../../core/services/project.service';
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

    <div class="empty-state card" *ngIf="loading">
      <div class="spinner"></div>
      <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
    </div>
    <div class="empty-state card" *ngIf="error">
      <div class="empty-state-icon">❌</div>
      <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
      <button class="btn btn-outline" (click)="loadProject()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
    </div>
    <div class="card" *ngIf="!loading && !error && project">
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
          <label>{{ 'projects.createdAt' | translate }}</label>
          <div class="value">{{ project.createdAtUtc | date:'medium' }}</div>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════
         PROJECT SETTINGS — Admin Support View (Read-only)
         ═══════════════════════════════════════════════════════ -->
    <div class="settings-section">
      <div class="empty-state card" *ngIf="settingsError" style="margin-bottom: 24px;">
        <div class="empty-state-icon">❌</div>
        <div class="empty-state-title">{{ 'projectSettings.failedToLoad' | translate }}</div>
        <button class="btn btn-outline" (click)="loadProjectSettings()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
      </div>
      <div *ngIf="!settingsError && projectSettings">
      <div class="vault-header">
        <h2 class="section-title">
          <span class="section-icon">⚙️</span>
          {{ 'projectSettings.title' | translate }}
        </h2>
        <span class="badge badge-readonly">{{ 'projectSettings.readOnly' | translate }}</span>
      </div>

      <div class="card">
        <div class="detail-grid">
          <div class="detail-item">
            <label>{{ 'projectSettings.name' | translate }}</label>
            <div class="value">{{ projectSettings.projectName }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'projectSettings.type' | translate }}</label>
            <div class="value">{{ 'projectType.' + projectSettings.projectType | translate }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'projectSettings.currentStage' | translate }}</label>
            <div class="value">
              <span class="badge" [ngClass]="getBadgeClass(projectSettings.currentStage)">
                {{ 'currentStage.' + projectSettings.currentStage | translate }}
              </span>
            </div>
            <div class="value" style="font-size: 0.75rem; color: var(--text-muted); margin-top: 4px;">
              ℹ️ {{ 'projectSettings.stageManagedFromTimeline' | translate }}
            </div>
          </div>
          <div class="detail-item">
            <label>{{ 'projectSettings.city' | translate }}</label>
            <div class="value">{{ projectSettings.city || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'projectSettings.locationText' | translate }}</label>
            <div class="value">{{ projectSettings.locationText || '—' }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'projectSettings.mapLink' | translate }}</label>
            <div class="value" *ngIf="projectSettings.mapLink">
              <a [href]="projectSettings.mapLink" target="_blank" rel="noopener noreferrer" class="link">{{ projectSettings.mapLink }}</a>
            </div>
            <div class="value muted" *ngIf="!projectSettings.mapLink">{{ 'projectSettings.noMapLink' | translate }}</div>
          </div>
          <div class="detail-item">
            <label>{{ 'profile.updatedAt' | translate }}</label>
            <div class="value">{{ projectSettings.updatedAtUtc | date:'medium' }}</div>
          </div>
        </div>
      </div>
    </div>
    <div class="empty-state card" *ngIf="settingsError" style="margin-top: 32px;">
      <div class="empty-state-icon">❌</div>
      <div class="empty-state-title">{{ 'projectSettings.failedToLoad' | translate }}</div>
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

        <div class="empty-state card" *ngIf="timelineLoading">
        <div class="spinner"></div>
        <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
      </div>
      <div class="empty-state card" *ngIf="timelineError">
        <div class="empty-state-icon">❌</div>
        <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
        <button class="btn btn-outline" (click)="loadTimeline()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
      </div>
      <div class="empty-state" *ngIf="!timelineLoading && !timelineError && timelineEvents.length === 0">
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
        <button class="vault-tab" [class.active]="vaultTab === 'search'" (click)="vaultTab = 'search'; searchFiles()">
          {{ 'vault.searchFiles' | translate }}
        </button>
        <button class="vault-tab" [class.active]="vaultTab === 'trash'" (click)="vaultTab = 'trash'; loadTrash()">
          {{ 'vault.trash' | translate }}
        </button>
      </div>

      <!-- FOLDERS TAB -->
      <div class="vault-panel" *ngIf="vaultTab === 'folders'">

        <!-- Folder List -->
        <div class="card vault-card" *ngIf="!selectedFolder">
          <div class="empty-state card" *ngIf="foldersLoading">
            <div class="spinner"></div>
            <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
          </div>
          <div class="empty-state card" *ngIf="foldersError">
            <div class="empty-state-icon">❌</div>
            <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
            <button class="btn btn-outline" (click)="loadFolders()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
          </div>
          <div class="empty-state" *ngIf="!foldersLoading && !foldersError && folders.length === 0">
            <div class="empty-state-icon">📂</div>
            <div class="empty-state-title">{{ 'vault.noFolders' | translate }}</div>
          </div>

          <table class="data-table" *ngIf="!foldersLoading && !foldersError && folders.length > 0">
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

          <div class="empty-state card" *ngIf="filesLoading">
            <div class="spinner"></div>
            <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
          </div>
          <div class="empty-state card" *ngIf="filesError">
            <div class="empty-state-icon">❌</div>
            <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
            <button class="btn btn-outline" (click)="selectFolder(selectedFolder!)" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
          </div>
          <div class="empty-state" *ngIf="!filesLoading && !filesError && folderFiles.length === 0">
            <div class="empty-state-icon">📄</div>
            <div class="empty-state-title">{{ 'vault.noFiles' | translate }}</div>
          </div>

          <table class="data-table" *ngIf="!filesLoading && !filesError && folderFiles.length > 0">
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

      <!-- SEARCH TAB -->
      <div class="vault-panel" *ngIf="vaultTab === 'search'">
        <div class="card vault-card">
          <div class="timeline-filters">
            <div class="filter-group" style="flex: 1; min-width: 200px;">
              <label>{{ 'vault.searchHint' | translate }}</label>
              <input type="text" [(ngModel)]="searchQuery" [placeholder]="'vault.searchHint' | translate" style="background: var(--bg-card); color: var(--text-primary); border: 1px solid var(--border); border-radius: 6px; padding: 6px 10px; font-size: 0.85rem; width: 100%;">
            </div>
            <div class="filter-group">
              <label>{{ 'vault.fileCategory' | translate }}</label>
              <select [(ngModel)]="searchCategory">
                <option value="">{{ 'vault.allCategories' | translate }}</option>
                <option value="Document">{{ 'vault.cat_document' | translate }}</option>
                <option value="Image">{{ 'vault.cat_image' | translate }}</option>
                <option value="Spreadsheet">{{ 'vault.cat_spreadsheet' | translate }}</option>
                <option value="Presentation">{{ 'vault.cat_presentation' | translate }}</option>
                <option value="Archive">{{ 'vault.cat_archive' | translate }}</option>
                <option value="Video">{{ 'vault.cat_video' | translate }}</option>
                <option value="Other">{{ 'vault.cat_other' | translate }}</option>
              </select>
            </div>
            <div class="filter-group">
              <label>{{ 'vault.extension' | translate }}</label>
              <input type="text" [(ngModel)]="searchExtension" placeholder="pdf, png..." style="background: var(--bg-card); color: var(--text-primary); border: 1px solid var(--border); border-radius: 6px; padding: 6px 10px; font-size: 0.85rem; width: 80px;">
            </div>
            <div class="filter-group">
              <label>{{ 'vault.sortBy' | translate }}</label>
              <select [(ngModel)]="searchSortBy">
                <option value="createdAtUtc">{{ 'vault.sortDate' | translate }}</option>
                <option value="originalFileName">{{ 'vault.sortName' | translate }}</option>
                <option value="fileSizeBytes">{{ 'vault.sortSize' | translate }}</option>
                <option value="fileExtension">{{ 'vault.sortExt' | translate }}</option>
              </select>
            </div>
            <div class="filter-group">
              <label>{{ 'vault.sortDirection' | translate }}</label>
              <select [(ngModel)]="searchSortDirection">
                <option value="desc">{{ 'vault.sortDesc' | translate }}</option>
                <option value="asc">{{ 'vault.sortAsc' | translate }}</option>
              </select>
            </div>
            <div class="filter-group" style="flex-direction: row; align-items: center; gap: 8px; padding-bottom: 6px;">
              <input type="checkbox" id="includeDeleted" [(ngModel)]="searchIncludeDeleted">
              <label for="includeDeleted" style="margin:0;">{{ 'vault.includeDeleted' | translate }}</label>
            </div>
            <div style="display: flex; gap: 8px; align-items: flex-end; padding-bottom: 2px;">
              <button class="btn-sm btn-outline" (click)="clearSearchFilters()">{{ 'vault.clearFilters' | translate }}</button>
              <button class="btn-sm btn-accent" style="color:var(--bg-dark); border:none; background:var(--accent);" (click)="searchFiles()">{{ 'vault.applyFilters' | translate }}</button>
            </div>
          </div>

          <div class="empty-state" *ngIf="!searchLoading && (!searchResults || searchResults.items.length === 0)">
            <div class="empty-state-icon">🔍</div>
            <div class="empty-state-title">{{ 'vault.noFiles' | translate }}</div>
            <div class="empty-state-sub" *ngIf="searchResults">{{ 'vault.tryChangingFilters' | translate }}</div>
          </div>

          <table class="data-table" *ngIf="searchResults && searchResults.items.length > 0">
            <thead>
              <tr>
                <th>{{ 'vault.fileName' | translate }}</th>
                <th>{{ 'vault.folderName' | translate }}</th>
                <th>{{ 'vault.fileCategory' | translate }}</th>
                <th>{{ 'vault.fileSize' | translate }}</th>
                <th>{{ 'vault.createdAt' | translate }}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let file of searchResults.items">
                <td>
                  <span class="file-icon">{{ getFileIcon(file.category || file.fileCategory) }}</span>
                  {{ file.originalFileName }}
                  <span *ngIf="file.isDeleted" class="badge badge-cancelled" style="margin-inline-start:8px">{{ 'vault.deletedBadge' | translate }}</span>
                </td>
                <td class="muted">{{ file.folderName || '—' }}</td>
                <td><span class="badge badge-category">{{ file.fileCategory || file.category || '—' }}</span></td>
                <td class="num-cell">{{ formatSize(file.fileSizeBytes || file.sizeBytes) }}</td>
                <td>{{ file.createdAtUtc | date:'mediumDate' }}</td>
                <td>
                  <button class="btn-sm btn-outline" (click)="downloadFile(file)" [title]="'vault.download' | translate">
                    ⬇️
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <div style="text-align:center; padding:12px; font-size:0.85rem; color:var(--text-muted);" *ngIf="searchResults && searchResults.items.length > 0">
            {{ searchResults.totalCount }} {{ 'timeline.events' | translate | slice:0:-2 }}
          </div>
        </div>
      </div>

      <!-- TRASH TAB -->
      <div class="vault-panel" *ngIf="vaultTab === 'trash'">
        <div class="card vault-card">

          <div class="empty-state card" *ngIf="trashLoading">
            <div class="spinner"></div>
            <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
          </div>
          <div class="empty-state card" *ngIf="trashError">
            <div class="empty-state-icon">❌</div>
            <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
            <button class="btn btn-outline" (click)="loadTrash()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
          </div>
          <div class="empty-state" *ngIf="!trashLoading && !trashError && trashEmpty">
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

        <div class="empty-state card" *ngIf="contractsLoading">
          <div class="spinner"></div>
          <div class="empty-state-title" style="margin-top: 16px;">{{ 'common.loading' | translate }}</div>
        </div>
        <div class="empty-state card" *ngIf="contractsError">
          <div class="empty-state-icon">❌</div>
          <div class="empty-state-title">{{ 'common.unableToLoad' | translate }}</div>
          <button class="btn btn-outline" (click)="loadContracts()" style="margin-top: 16px;">{{ 'common.retry' | translate }}</button>
        </div>
        <div class="empty-state" *ngIf="!contractsLoading && !contractsError && contracts.length === 0">
          <div class="empty-state-icon">📑</div>
          <div class="empty-state-title">{{ 'contracts.noContracts' | translate }}</div>
        </div>

        <table class="data-table" *ngIf="!contractsLoading && !contractsError && contracts.length > 0">
          <thead>
            <tr>
              <th>{{ 'contracts.contractTitle' | translate }}</th>
              <th>{{ 'contracts.contractType' | translate }}</th>
              <th>{{ 'contracts.partyName' | translate }}</th>
              <th>{{ 'contracts.contractValue' | translate }}</th>
              <th>{{ 'contracts.status' | translate }}</th>
              <th>{{ 'contracts.pdf' | translate }}</th>
              <th>{{ 'contracts.signed' | translate }}</th>
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
              <td>
                <span *ngIf="c.signedFileId" class="badge badge-signed">{{ 'contracts.signedAvailable' | translate }}</span>
                <span *ngIf="!c.signedFileId" class="muted">{{ 'contracts.signedNotAvailable' | translate }}</span>
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
          <div class="detail-item" *ngIf="selectedContract.signedFileId">
            <label>{{ 'contracts.signedFileId' | translate }}</label>
            <div class="value">{{ selectedContract.signedFileId }}</div>
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
        <div class="contract-pdf-actions" *ngIf="selectedContract.pdfFileId || selectedContract.signedFileId">
          <button *ngIf="selectedContract.pdfFileId" class="btn btn-accent" (click)="downloadContractPdf()" style="margin-inline-end: 12px;">
            ⬇️ {{ 'contracts.downloadPdf' | translate }}
          </button>
          <button *ngIf="selectedContract.signedFileId" class="btn btn-signed" (click)="downloadSignedContractPdf()">
            ⬇️ {{ 'contracts.downloadSignedPdf' | translate }}
          </button>
        </div>
      </div>

    </div>
  `,
  styles: [`
    .link { color: var(--accent); text-decoration: none; word-break: break-all; }
    .link:hover { text-decoration: underline; }

    /* Settings section */
    .settings-section { margin-top: 32px; }

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
    
    .btn-signed {
      background: #3fb950; color: #ffffff;
      border: none; padding: 10px 20px; border-radius: 8px;
      font-size: 0.9rem; font-weight: 600; cursor: pointer;
      font-family: inherit; transition: opacity 0.2s;
    }
    .btn-signed:hover { opacity: 0.85; }
  `]
})
export class ProjectDetailsComponent implements OnInit {
  project: ProjectDto | null = null;
  loading = true;
  error = false;

  projectSettings: ProjectSettingsDto | null = null;
  settingsError = false;

  // Vault state
  vaultTab: 'folders' | 'trash' | 'search' = 'folders';
  folders: FolderDto[] = [];
  foldersLoading = false;
  foldersError = false;
  selectedFolder: FolderDto | null = null;
  folderFiles: FileDto[] = [];
  filesLoading = false;
  filesError = false;
  trash: TrashDto | null = null;
  trashLoading = false;
  trashError = false;

  // File Search state
  searchQuery = '';
  searchCategory = '';
  searchExtension = '';
  searchSortBy = 'createdAtUtc';
  searchSortDirection = 'desc';
  searchIncludeDeleted = false;
  searchResults: any = null;
  searchLoading = false;
  searchError = false;

  // Contracts state
  contracts: ContractDto[] = [];
  contractsLoading = false;
  contractsError = false;
  selectedContract: ContractDto | null = null;
  contractData: any = null;

  // Timeline state
  timelineEvents: TimelineEventDto[] = [];
  timelineLoading = false;
  timelineError = false;
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
      this.loadProject();
      this.loadProjectSettings();
    } else {
      this.loading = false;
    }
  }

  loadProject(): void {
    this.loading = true;
    this.error = false;
    this.projectService.getById(this.projectId).subscribe({
      next: (p) => { this.project = p; this.loading = false; this.loadFolders(); this.loadContracts(); this.loadTimeline(); },
      error: () => { this.loading = false; this.error = true; }
    });
  }

  loadProjectSettings(): void {
    this.settingsError = false;
    this.projectService.getSettings(this.projectId).subscribe({
      next: (s) => { this.projectSettings = s; },
      error: () => { this.settingsError = true; }
    });
  }

  // ── Vault logic ─────────────────────────────────

  loadFolders(): void {
    this.foldersLoading = true;
    this.foldersError = false;
    this.vaultService.getProjectFolders(this.projectId).subscribe({
      next: (f) => { this.folders = f; this.foldersLoading = false; },
      error: () => { this.foldersLoading = false; this.foldersError = true; }
    });
  }

  selectFolder(folder: FolderDto): void {
    this.selectedFolder = folder;
    this.filesLoading = true;
    this.filesError = false;
    this.vaultService.getFolderFiles(this.projectId, folder.id).subscribe({
      next: (files) => { this.folderFiles = files; this.filesLoading = false; },
      error: () => { this.filesLoading = false; this.filesError = true; }
    });
  }

  loadTrash(): void {
    this.trashLoading = true;
    this.trashError = false;
    this.vaultService.getProjectTrash(this.projectId).subscribe({
      next: (t) => { this.trash = t; this.trashLoading = false; },
      error: () => { this.trashLoading = false; this.trashError = true; }
    });
  }

  get trashEmpty(): boolean {
    if (!this.trash) return true;
    return this.trash.deletedFiles.length === 0 && this.trash.deletedFolders.length === 0;
  }

  searchFiles(): void {
    this.searchLoading = true;
    this.searchError = false;
    const params: any = {
      sortBy: this.searchSortBy,
      sortDirection: this.searchSortDirection,
      page: 1,
      pageSize: 50
    };
    if (this.searchQuery) params.q = this.searchQuery;
    if (this.searchCategory) params.fileCategory = this.searchCategory;
    if (this.searchExtension) params.extension = this.searchExtension;
    if (this.searchIncludeDeleted) params.includeDeleted = true;

    this.vaultService.searchProjectFiles(this.projectId, params).subscribe({
      next: (res) => { this.searchResults = res; this.searchLoading = false; },
      error: () => { this.searchLoading = false; this.searchError = true; }
    });
  }

  clearSearchFilters(): void {
    this.searchQuery = '';
    this.searchCategory = '';
    this.searchExtension = '';
    this.searchSortBy = 'createdAtUtc';
    this.searchSortDirection = 'desc';
    this.searchIncludeDeleted = false;
    this.searchFiles();
  }

  downloadFile(file: any): void {
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
    this.contractsError = false;
    this.contractsService.getProjectContracts(this.projectId).subscribe({
      next: (c) => { this.contracts = c; this.contractsLoading = false; },
      error: () => { this.contractsLoading = false; this.contractsError = true; }
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

  downloadSignedContractPdf(): void {
    if (!this.selectedContract?.signedFileId) return;
    this.contractsService.downloadSignedContractPdf(this.projectId, this.selectedContract.signedFileId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = (this.selectedContract?.contractTitle || 'signed_contract') + '.pdf';
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
    this.timelineError = false;
    const filters: { stage?: string; eventType?: string } = {};
    if (this.timelineFilterStage) filters.stage = this.timelineFilterStage;
    if (this.timelineFilterType) filters.eventType = this.timelineFilterType;
    this.timelineService.getProjectTimeline(this.projectId, filters).subscribe({
      next: (events) => { this.timelineEvents = events; this.timelineLoading = false; },
      error: () => { this.timelineLoading = false; this.timelineError = true; }
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
