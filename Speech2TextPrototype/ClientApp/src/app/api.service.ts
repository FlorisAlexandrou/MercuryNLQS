import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Answer } from './models/Answer.model';
import { DisplayTable } from './models/displayTable.model';

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

    public getAnswer(question: string, sqlQuery: string, voice: boolean): Observable<Answer> {
        if (sqlQuery)
            return this.httpClient.get(this.webUrl + this.apiUrl + `/token/${question}?voiceoutput=${voice}&sqlQuery=${sqlQuery}`);
        return this.httpClient.get(this.webUrl + this.apiUrl + `/token/${question}?voiceoutput=${voice}`);
    }

    public getPagedData(pageIndex: number, pageSize: number): Observable<DisplayTable> {
        return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/page?pageIndex=${pageIndex}&pageSize=${pageSize}`);
    }

    public getSortedData(column: string, sortOrder: string, pageIndex: number, pageSize: number): Observable<DisplayTable> {
        return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/sort?column=${column}&sortOrder=${sortOrder}&pageIndex=${pageIndex}&pageSize=${pageSize}`);
    }

    public textToSpeech(text: string) {
        return this.httpClient.get(this.webUrl + this.apiUrl + "/text2speech/" + text);
    }
}
