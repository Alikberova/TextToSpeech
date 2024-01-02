import { HttpClient } from "@angular/common/http";
import { Constants } from "../constants";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root',
})

export class AudioClient {
    private apiUrl = `${Constants.ApiUrl}/audio`;

    constructor(private http: HttpClient) {}
  
    downloadMp3(): void {
        this.http.get(`${this.apiUrl}/downloadmp3`, { responseType: 'blob' })
            .subscribe(blob => {
                const url = window.URL.createObjectURL(blob);
                const anchor = document.createElement('a');
                anchor.download = 'output.mp3';
                anchor.href = url;
                anchor.click();
                window.URL.revokeObjectURL(url);
            });
    }

    getAudioStreamUrl(fileId: string): string {
        return `${this.apiUrl}/streammp3/${fileId}`;
      }
}
