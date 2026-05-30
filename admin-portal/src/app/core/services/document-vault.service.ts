import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FolderDto {
  id: string;
  projectId: string;
  folderName: string;
  folderType: string;
  isSystemFolder: boolean;
  fileCount: number;
  createdAtUtc: string;
}

export interface FileDto {
  id: string;
  projectFolderId: string;
  originalFileName: string;
  category: string;
  contentType: string;
  sizeBytes: number;
  isDeleted: boolean;
  createdAtUtc: string;
}

export interface TrashDto {
  deletedFiles: FileDto[];
  deletedFolders: FolderDto[];
}

@Injectable({ providedIn: 'root' })
export class DocumentVaultService {
  constructor(private http: HttpClient) {}

  /** List all active folders for a project. */
  getProjectFolders(projectId: string): Observable<FolderDto[]> {
    return this.http.get<FolderDto[]>(
      `${environment.apiUrl}/projects/${projectId}/folders`
    );
  }

  /** List files in a specific folder. */
  getFolderFiles(projectId: string, folderId: string): Observable<FileDto[]> {
    return this.http.get<FileDto[]>(
      `${environment.apiUrl}/projects/${projectId}/folders/${folderId}/files`
    );
  }

  /** Get trash summary for a project. */
  getProjectTrash(projectId: string): Observable<TrashDto> {
    return this.http.get<TrashDto>(
      `${environment.apiUrl}/projects/${projectId}/trash`
    );
  }

  /** Download file as Blob — uses Authorization header via interceptor, never JWT in URL. */
  downloadFile(projectId: string, fileId: string): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/projects/${projectId}/files/${fileId}/download`,
      { responseType: 'blob' }
    );
  }
}
