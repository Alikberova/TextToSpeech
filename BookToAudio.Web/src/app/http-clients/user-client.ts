import { HttpClient } from "@angular/common/http";
import { Observable, catchError } from "rxjs";
import { User } from "../models/user";
import { Сonstants } from "../constants";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root',
})

export class UserClient {
    private apiUrl = `${Сonstants.ApiUrl}/users`;

    constructor(private http: HttpClient) {}
  
    loginUser(credentials: { username: string; password: string }): Observable<any> {
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
    
    checkIfUserExists(userName: string): Observable<boolean> {
      return this.http.post<boolean>(`${this.apiUrl}/userExists`, { userName: userName })
        .pipe(
          catchError((error) => {
            console.error('Error checking user existence:', error);
            throw error;
          })
        );
    }
}
