import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Answer } from './models/Answer.model';
import { DisplayTable } from './models/displayTable.model';
import { PredictionData } from './models/predictionData.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
    // webUrl = 'https://localhost:5001';
    webUrl = 'https://localhost:44382';
    apiUrl = '/api/UserInput';
    pythonServiceUrl = 'https://tokens-api.herokuapp.com';

    constructor(private httpClient: HttpClient) { }

    public getText() {
        return this.httpClient.get<string>(this.webUrl + this.apiUrl);
    }

    public getAnswer(question: string, sqlQuery: string, uuid: string, voice: boolean): Observable<Answer> {
        if (sqlQuery)
            return this.httpClient.get(this.webUrl + this.apiUrl + `/token/${question}?uuid=${uuid}&sqlQuery=${sqlQuery}&voiceoutput=${voice}`);
        return this.httpClient.get(this.webUrl + this.apiUrl + `/token/${question}?uuid=${uuid}&voiceoutput=${voice}`);
    }

    public getPagedData(pageIndex: number, pageSize: number, uuid: string ): Observable<DisplayTable> {
        return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/page?pageIndex=${pageIndex}&pageSize=${pageSize}&uuid=${uuid}`);
    }

    public getSortedData(column: string, sortOrder: string, pageIndex: number, pageSize: number, uuid: string): Observable<DisplayTable> {
        return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/sort?column=${column}&sortOrder=${sortOrder}&pageIndex=${pageIndex}&pageSize=${pageSize}&uuid=${uuid}`);
    }

    public textToSpeech(text: string) {
        return this.httpClient.get(this.webUrl + this.apiUrl + '/text2speech/' + text);
    }

    public deleteTable(uuid: string) {
        return this.httpClient.get(this.webUrl + this.apiUrl + `/table/delete?uuid=${uuid}`);
    }

    public predict(currentData: PredictionData[]): Observable<PredictionData[]> {
      return this.httpClient.post<PredictionData[]>(this.pythonServiceUrl + '/predict', currentData);
    }

  public saveDisplayTableData(data: DisplayTable[]) {
    return this.httpClient.post(this.webUrl + this.apiUrl + '/table/save', data);
  }
} 
