import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [ CommonModule, RouterModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})

export class FooterComponent {
  year = new Date().getFullYear();
  
  links = [
    { path: '/qa', label: 'Q&A' },
    { path: '/contacts', label: 'Contacts' },
    { path: '/privacy-policy', label: 'Privacy Policy' },
    { path: '/terms-of-use', label: 'Terms of Use' }
  ];
}
