import { Observable } from "rxjs";
import { User } from "../models/dto/user";
import { Injectable } from "@angular/core";
import { ConfigService } from "../services/config.service";
import { ApiService } from "./base-client";

@Injectable({
    providedIn: 'root',
})

export class UserClient {
  private apiUrl = `${this.configService.apiUrl}/users`;

  constructor(private apiService: ApiService, private configService: ConfigService) {}

  loginUser(credentials: { userName: string; password: string }): Observable<any> {
    return this.apiService.post<any>(`${this.apiUrl}/login`, credentials);
  }

  register(user: User): Observable<User> {
    return this.apiService.post<User>(`${this.apiUrl}/register`, user);
  }

  deleteUser(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
  }

  updateUser(id: string, user: User): Observable<User> {
    return this.apiService.put<User>(`${this.apiUrl}/${id}`, user);
  }

  getUserById(id: string): Observable<User> {
    return this.apiService.get<User>(`${this.apiUrl}/${id}`);
  }
}
