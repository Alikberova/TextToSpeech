import { Routes } from '@angular/router';
import { LoginComponent } from './auth-components/login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './auth-components/register/register.component';
import { QaComponent } from './qa/qa.component';
import { ContactsComponent } from './contacts/contacts.component';
import { FeedbackFormComponent } from './feedback-form/feedback-form.component';
import { LegalDocumentComponent } from './legal-document/legal-document.component';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'home', component: HomeComponent },
  { path: 'qa', component: QaComponent, pathMatch: 'full' },
  { path: 'contacts', component: ContactsComponent, pathMatch: 'full' },
  { path: "feedback", component: FeedbackFormComponent, pathMatch: 'full'},
  { path: 'legal/:docType', component: LegalDocumentComponent },
  { path: '**', component: HomeComponent },
];
