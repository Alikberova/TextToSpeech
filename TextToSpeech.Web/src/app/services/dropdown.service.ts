import { Injectable } from "@angular/core";
import { DropdownConfig, DropdownItem } from "../models/dropdown-config";
import { VoiceClient } from "../http-clients/voice-client";
import { Languages, EnUSLanguageCode } from "../constants/language";
import { OpenAiVoices, Narakeet } from "../constants/tts-constants";
import { Observable, map } from "rxjs";

@Injectable({
    providedIn: "root"
  })
  
  export class DropdownService {
    constructor (private voiceClient: VoiceClient) {}
    selectedLanguageCode = EnUSLanguageCode;
    defaultSelectedLanguageIndex: number = 0;
    readonly headingLanguage = 'voice language';
    readonly headingVoice = 'voice';
    readonly headingApi = 'speech service';
    
    getConfig(config: DropdownConfig | null= null,
        selectedIndex: number | null = null,
        dropDownList: DropdownItem[] | undefined = undefined,
        heading: string | undefined = undefined): DropdownConfig {
        config ??= {
            selectedIndex: 0,
            valueField: 'id',
            labelField: 'optionDescription',
            dropDownList: [],
            heading: ''
        };
        if (selectedIndex !== null) {
            config.selectedIndex = selectedIndex;
        }
        if (dropDownList) {
            config.dropDownList = dropDownList
        }
        if (heading) {
            config.heading = heading;
        }
        config = { ...config }; 
        return config;
    }
    
    getSelectedValueFromIndex(config: DropdownConfig): string {
        const option = config.dropDownList[config.selectedIndex];
        return option.optionDescription;
    }
    
    getLangAndVoiceConfigForNarakeet(languageConfig: DropdownConfig, voiceConfig: DropdownConfig):
        Observable<{ languageConfig: DropdownConfig, voiceConfig: DropdownConfig }> {
        
        return this.voiceClient.getVoices(Narakeet).pipe(
            map((voices) => {
                // Create and deduplicate language dropdown options
                const uniqueLanguages = Array.from(
                    new Map(
                        voices.map(voice => [voice.languageCode, { language: voice.language, languageCode: voice.languageCode }])
                    ).values()
                ).sort((a, b) => a.language.localeCompare(b.language));

                const languageOptions = uniqueLanguages.map((lang, index) => ({ id: index, optionDescription: lang.language }));
                this.setDefaultSelectedLanguageIndex(languageOptions);
                const selectedLangIndex = this.getSelectedLangIndex(languageConfig);
                languageConfig = this.getConfig(languageConfig, selectedLangIndex, languageOptions, this.headingLanguage);

                // Create voice dropdown options based on the selected language
                const selectedLanguageCode = uniqueLanguages[languageConfig.selectedIndex].languageCode;
                this.selectedLanguageCode = selectedLanguageCode;
                const voiceOptions = voices
                    .filter(v => v.languageCode === selectedLanguageCode)
                    .map((v, index) => ({ id: index, optionDescription: v.name }));
                
                voiceConfig = this.getConfig(voiceConfig, null, voiceOptions, this.headingVoice);

                return { languageConfig, voiceConfig };
            })
        );
    }

    getLangAndVoiceConfigForOpenAi(languageConfig: DropdownConfig, voiceConfig: DropdownConfig): { languageConfigRes: DropdownConfig, voiceConfigRes: DropdownConfig } {
        const languageOptions = Languages.map((lang, index) => ({ id: index, optionDescription: lang.language }));
        const voiceOptions = OpenAiVoices.map((v, index) => ({ id: index, optionDescription: v }));
        
        this.setDefaultSelectedLanguageIndex(languageOptions);
        const selectedLangIndex = this.getSelectedLangIndex(languageConfig);

        const languageConfigRes = this.getConfig(languageConfig, selectedLangIndex, languageOptions, this.headingLanguage);
        this.selectedLanguageCode = Languages[languageConfigRes.selectedIndex].languageCode;

        const voiceConfigRes = this.getConfig(voiceConfig, null, voiceOptions, this.headingVoice);
        
        return { languageConfigRes, voiceConfigRes };
    }

    private setDefaultSelectedLanguageIndex(menuItems: DropdownItem[]) {
        this.defaultSelectedLanguageIndex = menuItems.findIndex(item => item.optionDescription.startsWith("English"));
    }

    private getSelectedLangIndex(languageConfig: DropdownConfig)
    {
        if (!languageConfig || languageConfig.selectedIndex < 0) {
            return this.defaultSelectedLanguageIndex;
        }
        return languageConfig.selectedIndex;
    }
}
