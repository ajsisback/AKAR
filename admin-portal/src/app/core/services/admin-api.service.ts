import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminOwnerListItemDto {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  projectsCount: number;
}

export interface AdminOwnerProjectSummaryDto {
  projectId: string;
  projectName: string;
  projectType: string;
  currentStage: string;
  city: string;
  createdAtUtc: string;
}

export interface AdminOwnerDetailDto {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  projects: AdminOwnerProjectSummaryDto[];
}

export interface AdminProjectListItemDto {
  projectId: string;
  ownerId: string;
  ownerName: string;
  projectName: string;
  projectType: string;
  currentStage: string;
  city: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface AdminProjectDetailDto {
  projectId: string;
  ownerId: string;
  ownerName: string;
  projectName: string;
  projectType: string;
  currentStage: string;
  city: string;
  locationText: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  fileCount: number;
  followerCount: number;
  contractCount: number;
  timelineCount: number;
}

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  constructor(private http: HttpClient) {}

  getOwners(): Observable<AdminOwnerListItemDto[]> {
    return this.http.get<AdminOwnerListItemDto[]>(`${environment.apiUrl}/admin/owners`);
  }

  getOwner(id: string): Observable<AdminOwnerDetailDto> {
    return this.http.get<AdminOwnerDetailDto>(`${environment.apiUrl}/admin/owners/${id}`);
  }

  getProjects(): Observable<AdminProjectListItemDto[]> {
    return this.http.get<AdminProjectListItemDto[]>(`${environment.apiUrl}/admin/projects`);
  }

  getProject(id: string): Observable<AdminProjectDetailDto> {
    return this.http.get<AdminProjectDetailDto>(`${environment.apiUrl}/admin/projects/${id}`);
  }
}
