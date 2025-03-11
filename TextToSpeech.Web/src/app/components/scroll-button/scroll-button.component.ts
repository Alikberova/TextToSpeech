import { NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-scroll-button',
    imports: [NgIf, MatIconModule, MatButtonModule],
    templateUrl: './scroll-button.component.html',
    styleUrl: './scroll-button.component.scss'
})
export class ScrollButtonComponent {
  isScrolled: boolean = false;

  @HostListener('window:scroll', [])
  onWindowScroll() {
    const verticalOffset = window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0;
    this.isScrolled = verticalOffset > 100;
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
