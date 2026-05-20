import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProjectDto {
  id: string; ownerId: string; projectName: string; projectType: string;
  city?: string; locationText?: string; mapLink?: string; currentStage: string;
  optionalImageUrl?: string; createdAtUtc: string; updatedAtUtc: string;
}

export interface CreateProjectRequest {
  projectName: string; projectType: string; city?: string;
  locationText?: string; mapLink?: string; currentStage?: string; optionalImageUrl?: string;
}

export interface DashboardDto {
  totalProjects: number; notStartedCount: number; structuralCount: number;
  finishingCount: number; completedCount: number;
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  constructor(private http: HttpClient) {}

  getDashboard(): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${environment.apiUrl}/dashboard`);
  }

  list(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>(`${environment.apiUrl}/projects`);
  }

  getById(id: string): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${environment.apiUrl}/projects/${id}`);
  }

  create(request: CreateProjectRequest): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(`${environment.apiUrl}/projects`, request);
  }
}
