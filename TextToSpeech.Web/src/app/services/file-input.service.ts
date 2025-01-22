import { Injectable, ViewChild, ElementRef } from "@angular/core";
import { AcceptableFileTypes, MaxInputLength } from "../constants/tts-constants";

@Injectable({
    providedIn: 'root'
  })
  export class FileInputService {
    uploadedFile: File | undefined;
    @ViewChild('fileInput') fileInput!: ElementRef;

    clearFileSelection() {
        this.uploadedFile = null!;
        if (this.fileInput && this.fileInput.nativeElement) {
        this.fileInput.nativeElement.value = '';
        }
    }

    validateFile(file: File): string | null {
        if (!AcceptableFileTypes.some(type => file.name.endsWith(type))) {
            return `Oops! 🙈 Looks like I can't work my magic on this file type. Stick with ${AcceptableFileTypes.join(' or ')} files for the best results, okay? 🚀`;
        }

        if (file.size > MaxInputLength) {
            return `The file is too large. Cannot process file that exceeds ${MaxInputLength} characters.`;
        }

        return null;
    }
}