import { Injectable } from '@angular/core';
import * as PrivacyPolicyData from '../../assets/privacy-policy.json';
import * as TermsOfUseData from '../../assets/terms-of-use.json';
import { PrivacyPolicy, TermsOfUse } from '../constants/route-constants';

@Injectable({ providedIn: 'root' })

export class DocumentService {

  getDocument(documentType: string): any {
    if (documentType === PrivacyPolicy) {
      return PrivacyPolicyData;
    }
    if (documentType === TermsOfUse) {
      return TermsOfUseData;
    }
    throw new Error(`Unknown document type: ${documentType}`);
  }
}
