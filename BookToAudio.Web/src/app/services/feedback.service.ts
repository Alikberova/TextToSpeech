import {  Injectable } from "@angular/core";
import { FeedbackRequest } from "../models/feedback"
import { HttpClient } from "@angular/common/http";
import { ConfigConstants } from '../constants/config-constants';


@Injectable({
  providedIn: "root"
})

export class FeedbackService {

  private apiUrl = `${ConfigConstants.BaseApiUrl}/email`;

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
