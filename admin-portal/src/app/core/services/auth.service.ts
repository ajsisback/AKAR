import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest { email: string; password: string; }
export interface RegisterRequest { fullName: string; email: string; phone: string; password: string; companyName?: string; }
export interface OwnerDto { id: string; fullName: string; email: string; phone: string; companyName?: string; isActive: boolean; createdAtUtc: string; }
export interface AuthResponse { token: string; owner: OwnerDto; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'akar_token';
  private readonly OWNER_KEY = 'akar_owner';
  private currentOwner = new BehaviorSubject<OwnerDto | null>(this.getStoredOwner());

  owner$ = this.currentOwner.asObservable();

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(res => this.storeAuth(res))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, request).pipe(
      tap(res => this.storeAuth(res))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.OWNER_KEY);
    this.currentOwner.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getOwner(): OwnerDto | null {
    return this.currentOwner.value;
  }

  private storeAuth(res: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.OWNER_KEY, JSON.stringify(res.owner));
    this.currentOwner.next(res.owner);
  }

  private getStoredOwner(): OwnerDto | null {
    const stored = localStorage.getItem(this.OWNER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
