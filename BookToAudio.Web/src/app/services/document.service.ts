import { Injectable } from '@angular/core';
import * as PrivacyPolicyData from '../../assets/privacy-policy.json';
import * as TermsOfUseData from '../../assets/terms-of-use.json';

@Injectable({ providedIn: 'root' })

export class DocumentService {

  getDocument(documentType: string): any {
    if (documentType === 'privacy-policy') {
      return PrivacyPolicyData;
    }
    if (documentType === 'terms-of-use') {
      return TermsOfUseData;
    }
    return null;
  }
}
