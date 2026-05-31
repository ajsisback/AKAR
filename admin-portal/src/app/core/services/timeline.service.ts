import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TimelineEventDto {
  id: string;
  projectId: string;
  eventType: string;
  stage: string;
  title: string;
  description: string | null;
  isSystemGenerated: boolean;
  sourceType: string | null;
  sourceId: string | null;
  eventDateUtc: string;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class TimelineService {
  constructor(private http: HttpClient) {}

  /** Get project timeline events with optional filters. */
  getProjectTimeline(
    projectId: string,
    filters?: { stage?: string; eventType?: string }
  ): Observable<TimelineEventDto[]> {
    let params = new HttpParams();
    if (filters?.stage) params = params.set('stage', filters.stage);
    if (filters?.eventType) params = params.set('eventType', filters.eventType);

    return this.http.get<TimelineEventDto[]>(
      `${environment.apiUrl}/projects/${projectId}/timeline`,
      { params }
    );
  }
}
