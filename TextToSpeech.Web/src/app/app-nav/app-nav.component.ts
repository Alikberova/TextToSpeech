import { Component } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth/auth.service';
import { Router, RouterModule } from '@angular/router';
import { RoutesConstants } from '../constants/route-constants';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-nav',
  templateUrl: './app-nav.component.html',
  styleUrls: ['./app-nav.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, MatMenuModule, MatToolbarModule, MatButtonModule]
})
export class AppNavComponent {

  constructor(private breakpointObserver: BreakpointObserver, private authService: AuthService,
    private router: Router) {}

  menu = [
      { name: 'Generate Speech', route: `/${RoutesConstants.ttsForm}` },
      { name: 'Feedback', route: `/${RoutesConstants.feedback}` },
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
    this.router.navigate([RoutesConstants.login]);
  }

  redirectToHome() {
    this.router.navigate([RoutesConstants.home]);
  }
}
