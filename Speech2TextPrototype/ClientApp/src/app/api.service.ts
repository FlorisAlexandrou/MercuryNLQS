import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ResponseAnswer } from './voice/AnswerInterface';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
    webUrl: string = 'https://localhost:44382'
    apiUrl: string = '/api/UserInput'

    constructor(private httpClient: HttpClient) { }

    public getText() {
        return this.httpClient.get<string>(this.webUrl + this.apiUrl);
    }

    public getAnswer(question: string, voice: boolean):Observable<ResponseAnswer> {
        return this.httpClient.get(this.webUrl + this.apiUrl + "/token/" + question + "?voiceoutput=" + voice);
    }

    public textToSpeech(text: string) {
        return this.httpClient.get(this.webUrl + this.apiUrl + "/text2speech/" + text);
    }
}
