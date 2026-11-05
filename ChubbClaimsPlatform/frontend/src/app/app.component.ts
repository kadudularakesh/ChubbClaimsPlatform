import { Component, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { CommonModule } from '@angular/common';
@Component({
 selector: 'app-root', standalone: true, imports: [RouterOutlet, CommonModule],
 template: `<header><div class="brand">CHUBB <span>Claims Portal</span></div><div *ngIf="auth.user() as u" class="user">{{u.name}} · {{u.role}} <button class="secondary" (click)="logout()">Sign out</button></div></header><main><router-outlet /></main><footer>Chubb APAC · Claims Management MVP</footer>`
})
export class AppComponent {
 auth = inject(AuthService); router = inject(Router);
 logout() { this.auth.logout(); this.router.navigate(['/login']); }
}
