import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
@Component({
 standalone: true, imports: [FormsModule, CommonModule],
 template: `<section class="login card"><h1>Claims portal</h1><p>Sign in to manage or track an insurance claim.</p>
 <label>Email</label><input [(ngModel)]="email" type="email" placeholder="name@example.com">
 <label>Password</label><input [(ngModel)]="password" type="password" placeholder="Password">
 <p class="error" *ngIf="error">{{error}}</p><button (click)="submit()" [disabled]="busy">{{busy?'Signing in…':'Sign in'}}</button>
 <div class="hint"><b>Demo claimant</b> claimant@demo.com / Passw0rd!<br><b>Demo officer</b> officer@demo.com / Passw0rd!</div></section>`,
})
export class LoginComponent {
 email='claimant@demo.com'; password='Passw0rd!'; error=''; busy=false;
 auth=inject(AuthService); router=inject(Router);
 submit() { this.busy=true; this.error=''; this.auth.login(this.email,this.password).subscribe({next:()=>{this.busy=false;this.router.navigate(['/']);},error:()=>{this.busy=false;this.error='Login failed. Check your email and password.';}}); }
}
