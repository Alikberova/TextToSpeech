import { Component } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { AppMaterialModule } from '../app.material/app.material.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav',
  templateUrl: './app-nav.component.html',
  styleUrls: ['./app-nav.component.scss'],
  standalone: true,
  imports: [ AppMaterialModule, CommonModule ]
})
export class AppNavComponent {

  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(private breakpointObserver: BreakpointObserver) {}
  // You can define your menu item here.
  menu = ['Menu Item 1', 'Menu Item 2', 'Menu Item 3', 'Menu Item 4'];
}
