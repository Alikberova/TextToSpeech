import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth-components/login/login.component';
import { RegisterComponent } from './components/auth-components/register/register.component';
import { QaComponent } from './components/qa/qa.component';
import { ContactsComponent } from './components/contacts/contacts.component';
import { RoutesConstants } from './constants/route-constants';
import { FeedbackFormComponent } from './components/feedback-form/feedback-form.component';
import { HomeComponent } from './components/home/home.component';
import { LegalDocumentComponent } from './components/legal-document/legal-document.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { TtsFormComponent } from './components/tts-form/tts-form.component';

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
