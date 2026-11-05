import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Claim } from './models';
@Injectable({ providedIn: 'root' })
export class ClaimService {
  constructor(private http: HttpClient) {}
  mine() { return this.http.get<Claim[]>('/api/claims/mine'); }
  all() { return this.http.get<Claim[]>('/api/claims'); }
  create(data: any) { return this.http.post<Claim>('/api/claims', data); }
  assign(id: string) { return this.http.post(`/api/claims/${id}/assign`, {}); }
  requestInfo(id: string, reason: string) { return this.http.post(`/api/claims/${id}/request-information`, { reason }); }
  provideInfo(id: string, reason: string) { return this.http.post(`/api/claims/${id}/provide-information`, { reason }); }
  approve(id: string, amount: number) { return this.http.post(`/api/claims/${id}/approve`, { amount }); }
  reject(id: string, reason: string) { return this.http.post(`/api/claims/${id}/reject`, { reason }); }
  settle(id: string, amount: number) { return this.http.post(`/api/claims/${id}/settle`, { amount }); }
  dashboard() { return this.http.get<any>('/api/dashboard/officer'); }
}
