import { InjectionToken } from '@angular/core';

export const API_URL = new InjectionToken<string>('API_URL');
export const SERVER_URL = new InjectionToken<string>('SERVER_URL');

export const GUEST_JWT_KEY = 'guest_jwt';
