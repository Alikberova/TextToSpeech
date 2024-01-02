import { Routes } from '@angular/router';
import { LoginComponent } from './auth-components/login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './auth-components/register/register.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'home', component: HomeComponent },
  { path: '**', component: LoginComponent },
];
