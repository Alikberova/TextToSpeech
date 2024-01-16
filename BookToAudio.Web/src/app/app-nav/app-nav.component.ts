import { Component } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { AppMaterialModule } from '../app.material/app.material.module';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth/auth.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './app-nav.component.html',
  styleUrls: ['./app-nav.component.scss'],
  standalone: true,
  imports: [ AppMaterialModule, CommonModule, RouterModule ]
})
export class AppNavComponent {

  constructor(private breakpointObserver: BreakpointObserver, private authService: AuthService,
    private router: Router) {}

    menu = [
      { name: 'Feedback', route: '/feedback' },
    ];
  
  get isLoggedIn() {
    return this.authService.isAuthenticated();
  }

  //todo adapt the UI and behavior for mobile device
  isHandset$: Observable<boolean> = this.breakpointObserver
    .observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  logoutUser(): void {
    this.authService.removeToken();
    this.redirectToHome();
  }

  redirectToLogin() {
    this.router.navigate(['login']);
  }

  redirectToHome() {
    this.router.navigate(['home']);
  }
}
