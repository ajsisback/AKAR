import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminLoginRequest { email: string; password: string; }
export interface AdminDto { id: string; fullName: string; email: string; role: string; }
export interface AdminAuthResponse { token: string; admin: AdminDto; }

@Injectable({ providedIn: 'root' })
export class AdminAuthService {
  private readonly TOKEN_KEY = 'akar_admin_token';
  private readonly ADMIN_KEY = 'akar_admin_user';
  private currentAdmin = new BehaviorSubject<AdminDto | null>(this.getStoredAdmin());

  admin$ = this.currentAdmin.asObservable();

  constructor(private http: HttpClient) {}

  login(request: AdminLoginRequest): Observable<AdminAuthResponse> {
    return this.http.post<AdminAuthResponse>(`${environment.apiUrl}/admin/auth/login`, request).pipe(
      tap(res => this.storeAuth(res))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.ADMIN_KEY);
    this.currentAdmin.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.userType === 'Admin';
    } catch {
      return false;
    }
  }

  getAdmin(): AdminDto | null {
    return this.currentAdmin.value;
  }

  private storeAuth(res: AdminAuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.ADMIN_KEY, JSON.stringify(res.admin));
    this.currentAdmin.next(res.admin);
  }

  private getStoredAdmin(): AdminDto | null {
    const stored = localStorage.getItem(this.ADMIN_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
