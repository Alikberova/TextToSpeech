import {  Injectable, inject } from "@angular/core";
import { FeedbackRequest } from "../models/dto/feedback"
import { HttpClient } from "@angular/common/http";
import { ConfigService } from "./config.service";

@Injectable({
  providedIn: "root"
})

export class FeedbackService {
  private configService = inject(ConfigService);

  private apiUrl = `${this.configService.apiUrl}/email`;

  constructor(private http: HttpClient) { }

  feedbackMessageSend(request: FeedbackRequest) {

    const body = {
      name: request.name,
      userEmail: request.userEmail,
      message: request.message
    }

    return this.http.post<string>(this.apiUrl, body)
  }
}
