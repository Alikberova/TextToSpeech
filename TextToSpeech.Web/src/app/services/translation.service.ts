import { Injectable } from "@angular/core";
import { EnLanguageCode } from "../constants/language";
import { TranslationClient } from "../http-clients/translation-client";
import { TranslationRequest } from "../models/dto/translation-request";

@Injectable({
    providedIn: 'root'
  })
  export class TranslationService {
    constructor(private translationClient: TranslationClient) {}
  
    translateFromEnglish(shortLangCode: string, text: string, next: (result: string) => void): void {
      const request: TranslationRequest = {
        text: text,
        sourceLanguage: EnLanguageCode,
        targetLanguage: shortLangCode
      }
      this.translationClient.translate(request).subscribe({
        next: (res) => next(res)
      });
    }
}
  