import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RoutesConstants } from '../constants/route-constants';

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
    { path: `/${RoutesConstants.qa}`, label: 'Q&A' },
    { path: `/${RoutesConstants.contacts}`, label: 'Contacts' },
    { path: `/${RoutesConstants.privacyPolicy}`, label: 'Privacy Policy' },
    { path: `/${RoutesConstants.termsOfUse}`, label: 'Terms of Use' },
  ];
}
