import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Language } from './voice/LanguageInterface';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
    webUrl: string = 'https://localhost:44382'
    apiUrl: string = '/api/Voice'

    constructor(private httpClient: HttpClient) { }

    public getText():Observable<Language> {
        return this.httpClient.get<Language>(this.webUrl + this.apiUrl);
    }
}
