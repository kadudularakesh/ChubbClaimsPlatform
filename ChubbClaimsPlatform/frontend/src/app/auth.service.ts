import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { LoginResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  user = signal<LoginResponse | null>(this.read());
  constructor(private http: HttpClient) {}
  private read(): LoginResponse | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    return { token, name: localStorage.getItem('name') || '', role: localStorage.getItem('role') || '' };
  }
  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(tap(x => {
      localStorage.setItem('token', x.token); localStorage.setItem('name', x.name); localStorage.setItem('role', x.role); this.user.set(x);
    }));
  }
  logout() { localStorage.clear(); this.user.set(null); }
}
