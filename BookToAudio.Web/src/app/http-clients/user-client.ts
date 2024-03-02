import { HttpClient } from "@angular/common/http";
import { Observable, catchError } from "rxjs";
import { User } from "../models/user";
import { ConfigConstants } from "../constants/config-constants";
import { Injectable, inject } from "@angular/core";
import { ConfigService } from "../services/config-service";

@Injectable({
    providedIn: 'root',
})

export class UserClient {
  private configService = inject(ConfigService);

  private apiUrl = `${this.configService.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  loginUser(credentials: { userName: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials);
  }

  register(user: User): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/register`, user);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  updateUser(id: string, user: User): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, user);
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }
}
