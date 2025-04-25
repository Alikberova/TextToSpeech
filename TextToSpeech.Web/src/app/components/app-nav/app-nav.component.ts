import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { RoutesConstants } from '../../constants/route-constants';
import { AuthService } from '../../services/auth/auth.service';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { FooterComponent } from "../footer/footer.component";

@Component({
    selector: 'app-nav',
    templateUrl: './app-nav.component.html',
    styleUrls: ['./app-nav.component.scss'],
    imports: [CommonModule,
        RouterModule,
        MatMenuModule,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        MatSidenavModule,
        MatListModule,
        FooterComponent]
})
export class AppNavComponent implements OnInit {

  constructor(private breakpointObserver: BreakpointObserver, private authService: AuthService,
    private router: Router) {}

  ngOnInit(): void {
    // if (!this.isLoggedIn) {
    //   this.menu.push({ name: 'Login', route: `/${RoutesConstants.login}` })
    // }
  }

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
