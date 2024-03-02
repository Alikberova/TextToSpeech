import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { HttpClient, HttpBackend } from '@angular/common/http';
import { AppConfig } from '../models/app-config';

@Injectable({
  providedIn: 'root',
})

export class ConfigService {
  //We create the configuration with default values in case anything fails
  private configuration: any;

  private http: HttpClient;

  constructor(private readonly httpHandler: HttpBackend) {
    this.http = new HttpClient(this.httpHandler);
  }

  //This function will get the current config for the environment
  async setConfig(): Promise<void | AppConfig> {
    try {
          const config = await firstValueFrom(this.http.get<AppConfig>('./app-config.json'));
          return (this.configuration = config);
      } catch (error) {
          console.error(error);
      }
  }

  //We're going to use this function to read the config.
  getConfig(): AppConfig {
    return this.configuration;
  }

  get baseUrl(): string {
    return this.configuration.baseUrl;
  }

  get apiUrl(): string {
    return `${this.configuration.baseUrl}${this.configuration.apiEndpoint}`;
  }
}