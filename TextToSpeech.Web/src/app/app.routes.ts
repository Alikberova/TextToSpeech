import { Routes } from '@angular/router';
import { LoginComponent } from './auth-components/login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './auth-components/register/register.component';
import { QaComponent } from './qa/qa.component';
import { ContactsComponent } from './contacts/contacts.component';
import { FeedbackFormComponent } from './feedback-form/feedback-form.component';
import { LegalDocumentComponent } from './legal-document/legal-document.component';
import { RoutesConstants } from './constants/route-constants';
import { TtsFormComponent } from './tts-form/tts-form.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';

export const routes: Routes = [
  { path: '', redirectTo: RoutesConstants.home, pathMatch: 'full' },
  { path: RoutesConstants.login, component: LoginComponent },
  { path: RoutesConstants.register, component: RegisterComponent },
  { path: RoutesConstants.home, component: HomeComponent },
  { path: RoutesConstants.qa, component: QaComponent, pathMatch: 'full' },
  { path: RoutesConstants.contacts, component: ContactsComponent, pathMatch: 'full' },
  { path: RoutesConstants.feedback, component: FeedbackFormComponent, pathMatch: 'full'},
  { path: RoutesConstants.legal_documentType, component: LegalDocumentComponent },
  { path: RoutesConstants.ttsForm, component: TtsFormComponent, pathMatch: 'full' },
  { path: '**', component: PageNotFoundComponent },
];
