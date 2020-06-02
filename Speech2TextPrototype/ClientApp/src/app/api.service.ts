import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Language, Entity } from './voice/LanguageInterface';
import { ResponseAnswer } from './voice/AnswerInterface';

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

    public getAnswer(question: string, voice: boolean):Observable<ResponseAnswer> {
        return this.httpClient.get(this.webUrl + this.apiUrl + "/" + question + "?voice=" + voice);
    }

    public getEntities(text: string):Observable<Entity[]> {
        return this.httpClient.get<Entity[]>(this.webUrl + this.apiUrl + "/entity/" + text);
    }

    public textToSpeech(text: string) {
        return this.httpClient.get(this.webUrl + this.apiUrl + "/text2speech/" + text);
    }
}
