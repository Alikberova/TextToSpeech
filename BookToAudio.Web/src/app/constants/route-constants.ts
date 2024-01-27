const legal = 'legal';

export const PrivacyPolicy = 'privacy-policy';
export const TermsOfUse = 'terms-of-use';
export const DocumentType = 'docType';

export const RoutesConstants = {
    home: 'home',
    login: 'login',
    register: 'register',
    qa: 'qa',
    contacts: 'contacts',
    feedback: 'feedback',
    ttsForm: 'tts-form',
    legal_documentType: `${legal}/:${DocumentType}`,
    privacyPolicy: `${legal}/${PrivacyPolicy}`,
    termsOfUse: `${legal}/${TermsOfUse}`,
};
