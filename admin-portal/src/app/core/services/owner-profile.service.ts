import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OwnerProfileDto {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  companyName?: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

@Injectable({ providedIn: 'root' })
export class OwnerProfileService {
  constructor(private http: HttpClient) {}

  getProfile(): Observable<OwnerProfileDto> {
    return this.http.get<OwnerProfileDto>(`${environment.apiUrl}/owner/profile`);
  }
}
