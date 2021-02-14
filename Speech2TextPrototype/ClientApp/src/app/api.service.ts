import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DisplayTable } from './models/displayTable.model';
import { PredictionData } from './models/predictionData.model';
import { PyTokenize } from './models/pyTokenize.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  // webUrl = 'https://localhost:5001';
  webUrl = 'https://localhost:44382';
  apiUrl = '/api/UserInput';

  constructor(private httpClient: HttpClient) { }

  public getSqlAnswer(pyRes: PyTokenize) {
    return this.httpClient.post(this.webUrl + this.apiUrl + '/sqlAnswer', pyRes);
  }

  public getGroupByAnswer(query: string, groupByFilter: string, uuid: string) {
    return this.httpClient.get(this.webUrl + this.apiUrl + `/groupByAnswer?uuid=${uuid}&sqlQuery=${query}&groupByFilter=${groupByFilter}`);
  }

  public getChartData(uuid: string) {
    return this.httpClient.get(this.webUrl + this.apiUrl + `/chart?uuid=${uuid}`);
  }

  public getTableData(uuid: string) {
    return this.httpClient.get(this.webUrl + this.apiUrl + `/table?uuid=${uuid}`);
  }

  public getPagedData(pageIndex: number, pageSize: number, uuid: string): Observable<DisplayTable> {
    return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/page?pageIndex=${pageIndex}&pageSize=${pageSize}&uuid=${uuid}`);
  }

  public getSortedData(column: string, sortOrder: string, pageIndex: number, pageSize: number, uuid: string): Observable<DisplayTable> {
    return this.httpClient.get<DisplayTable>(this.webUrl + this.apiUrl + `/table/sort?column=${column}&sortOrder=${sortOrder}&pageIndex=${pageIndex}&pageSize=${pageSize}&uuid=${uuid}`);
  }

  public deleteTable(uuid: string) {
    return this.httpClient.get(this.webUrl + this.apiUrl + `/table/delete?uuid=${uuid}`);
  }

  public saveDisplayTableData(data: DisplayTable[]) {
    return this.httpClient.post(this.webUrl + this.apiUrl + '/table/save', data);
  }

  public getCustomSpeechRecognitionWords() {
    return this.httpClient.get(this.webUrl + this.apiUrl + '/speechRecogitionCustomWords');
  }
} 
