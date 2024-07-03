import {  Injectable } from "@angular/core";
import { FeedbackRequest } from "../models/dto/feedback"
import { ConfigService } from "../services/config.service";
import { ApiService } from "./base-client";

@Injectable({
  providedIn: "root"
})

export class EmailClient {
  private apiUrl = `${this.configService.apiUrl}/email`;

  constructor(private apiService: ApiService, private configService: ConfigService) {}

  sendEmail(request: FeedbackRequest) {
    const body = {
      name: request.name,
      userEmail: request.userEmail,
      message: request.message
    };

    return this.apiService.post<string>(this.apiUrl, body);
  }
}