import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ClaimService } from './claim.service';
import { Claim } from './models';

@Component({
 standalone:true, imports:[CommonModule,FormsModule,DatePipe,CurrencyPipe],
 template: `
 <div *ngIf="!auth.user()" class="login card"><h1>Welcome to Chubb Claims</h1><p>Please sign in to continue.</p><button (click)="router.navigate(['/login'])">Go to login</button></div>
 <ng-container *ngIf="auth.user() as user">
 <div class="page-title"><div><h1>{{user.role==='Claimant'?'My claims':'Claims operations'}}</h1><p>{{user.role==='Claimant'?'Report an incident and track your claim.':'Review incoming claims and manage team exposure.'}}</p></div>
 <button *ngIf="user.role==='Claimant'" (click)="showForm=!showForm">＋ New claim</button></div>
 <section *ngIf="user.role==='Claimant' && showForm" class="card">
 <h2>Report an incident</h2><div class="form-grid">
 <label>Claim type<select [(ngModel)]="form.claimType"><option>Motor</option><option>Property</option></select></label>
 <label>Policy number<input [(ngModel)]="form.policyNumber"></label>
 <label>Incident date<input type="date" [(ngModel)]="form.incidentDate"></label>
 <label>Incident location<input [(ngModel)]="form.incidentLocation"></label>
 <label>Estimated amount<input type="number" min="1" [(ngModel)]="form.estimatedAmount"></label>
 <label>Market<select [(ngModel)]="form.market"><option>SG</option><option>AU</option><option>HK</option><option>MY</option><option>TH</option><option>ID</option></select></label>
 <label class="wide">Incident description<textarea [(ngModel)]="form.description" rows="3"></textarea></label>
 </div><div class="actions"><button (click)="createClaim()">Submit claim</button><button class="secondary" (click)="showForm=false">Cancel</button></div><p class="error">{{message}}</p>
 </section>
 <section *ngIf="user.role==='ClaimsOfficer'" class="stats">
 <div class="stat card"><small>Total claims</small><strong>{{dashboard?.totalClaims??0}}</strong></div>
 <div class="stat card"><small>Submitted</small><strong>{{dashboard?.submitted??0}}</strong></div>
 <div class="stat card"><small>Under review</small><strong>{{dashboard?.underReview??0}}</strong></div>
 <div class="stat card"><small>Outstanding exposure</small><strong>{{dashboard?.outstandingLiability??0 | currency:'SGD '}}</strong></div>
 </section>
 <section class="card"><div class="section-head"><h2>{{user.role==='Claimant'?'Your claims':'Claims queue'}}</h2><button class="secondary" (click)="load()">Refresh</button></div>
 <p class="error">{{message}}</p><div class="table-wrap"><table><thead><tr><th>Claim ref</th><th>Type</th><th>Policy</th><th>Incident</th><th>Estimate</th><th>Status</th><th>Actions</th></tr></thead>
 <tbody><tr *ngFor="let c of claims"><td><b>{{c.claimNumber}}</b></td><td>{{c.claimType}}</td><td>{{c.policyNumber}}</td><td>{{c.incidentDate|date:'mediumDate'}}</td><td>{{c.estimatedAmount|currency:c.currency+' '}}</td><td><span class="status">{{c.status}}</span></td><td class="row-actions">
 <ng-container *ngIf="user.role==='ClaimsOfficer'">
 <button class="small" *ngIf="c.status==='Submitted'" (click)="assign(c)">Pick up</button>
 <button class="small" *ngIf="c.status==='UnderReview'||c.status==='InformationRequested'" (click)="approve(c)">Approve</button>
 <button class="small secondary" *ngIf="c.status==='UnderReview'" (click)="requestInfo(c)">Request info</button>
 <button class="small danger" *ngIf="c.status!=='Rejected'&&c.status!=='Settled'" (click)="reject(c)">Reject</button>
 <button class="small" *ngIf="c.status==='Approved'" (click)="settle(c)">Settle</button>
 </ng-container>
 <button class="small secondary" *ngIf="user.role==='Claimant'&&c.status==='InformationRequested'" (click)="provideInfo(c)">Provide info</button>
 </td></tr><tr *ngIf="claims.length===0"><td colspan="7" class="empty">No claims found.</td></tr></tbody></table></div></section>
 </ng-container>`
})
export class HomeComponent implements OnInit {
 auth=inject(AuthService); api=inject(ClaimService); router=inject(Router);
 claims:Claim[]=[]; dashboard:any=null; showForm=false; message='';
 form:any={claimType:'Motor',policyNumber:'',incidentDate:new Date().toISOString().slice(0,10),incidentLocation:'',estimatedAmount:1000,market:'SG',currency:'SGD',description:''};
 ngOnInit(){ if(!this.auth.user()) this.router.navigate(['/login']); else this.load(); }
 load(){this.message=''; if(this.auth.user()?.role==='Claimant') this.api.mine().subscribe(x=>this.claims=x); else {this.api.all().subscribe(x=>this.claims=x);this.api.dashboard().subscribe(x=>this.dashboard=x);} }
 createClaim(){this.api.create({...this.form,incidentDate:new Date(this.form.incidentDate).toISOString()}).subscribe({next:()=>{this.message='Claim submitted successfully.';this.showForm=false;this.load();},error:e=>this.message=e.error?.message||'Unable to submit claim.'});}
 assign(c:Claim){this.api.assign(c.id).subscribe(()=>this.load());}
 approve(c:Claim){const a=Number(prompt('Approved amount',String(c.estimatedAmount)));if(a>0)this.api.approve(c.id,a).subscribe(()=>this.load());}
 reject(c:Claim){const r=prompt('Reason for rejection');if(r)this.api.reject(c.id,r).subscribe(()=>this.load());}
 requestInfo(c:Claim){const r=prompt('What information is required?');if(r)this.api.requestInfo(c.id,r).subscribe(()=>this.load());}
 provideInfo(c:Claim){const r=prompt('Enter the additional information');if(r)this.api.provideInfo(c.id,r).subscribe(()=>this.load());}
 settle(c:Claim){const a=Number(prompt('Settlement amount',String(c.approvedAmount||c.estimatedAmount)));if(a>0)this.api.settle(c.id,a).subscribe(()=>this.load());}
}
