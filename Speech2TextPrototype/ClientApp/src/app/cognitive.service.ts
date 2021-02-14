import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CancellationDetails, CancellationReason, PhraseListGrammar, ResultReason, SpeechConfig, SpeechRecognizer, SpeechSynthesizer } from 'microsoft-cognitiveservices-speech-sdk';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PredictionData } from './models/predictionData.model';
import { PyTokenize } from './models/pyTokenize.model';

@Injectable({
  providedIn: 'root'
})
export class CognitiveService {
  private pythonServiceUrl = 'https://tokens-api.herokuapp.com';

  private speechConfig = SpeechConfig.fromSubscription('38df3e4febac4df48490c9f3d8eaa23f', 'eastus');
  private speechRecognizer = new SpeechRecognizer(this.speechConfig);
  private speechSynthesizer = new SpeechSynthesizer(this.speechConfig)
  private phraseList = PhraseListGrammar.fromRecognizer(this.speechRecognizer);

  private qnaMakerEndpoint = 'https://floris-qnaservice.azurewebsites.net';
  private qnaMakerEndpointKey = "f3536082-ee9c-408f-9948-1fb77b30c0c6";
  private kbId = "f158eca8-f7f3-4c88-a7d7-b7724956df4c";

  private sqlQueryWords = ['show', 'display', 'query', 'illustrate', 'print', 'select'];

  constructor(private httpClient: HttpClient, private apiService: ApiService) {
    // Add custom database related words to speech recognizer
    this.apiService.getCustomSpeechRecognitionWords().subscribe((words: string[]) => {
      words.forEach(word => this.phraseList.addPhrase(word));
    });
  }

  public speechToText() {
    return new Promise((resolve, reject) => {
      this.speechRecognizer.recognizeOnceAsync(result => {
        let text = "";
        switch (result.reason) {
          case ResultReason.RecognizedSpeech:
            text = result.text;
            break;
          case ResultReason.NoMatch:
            text = "Speech could not be recognized.";
            reject(text);
            break;
          case ResultReason.Canceled:
            var cancellation = CancellationDetails.fromResult(result);
            text = "Cancelled: Reason= " + cancellation.reason;
            if (cancellation.reason == CancellationReason.Error) {
              text = "Canceled: " + cancellation.ErrorCode;
            }
            reject(text);
            break;
        }
        resolve(text);
      });
    });
  }

  public textToSpeech(text: string) {
    let keywords = ["M_SALES_VALUE", "M_SALES_VOLUME", "M_SALES_ITEMS",
      "PRODUCT_NAME", "CATEGORY_NAME", "BRAND", "Prediction"]

    // Do not speak keyword text
    if (keywords.includes(text))
      return;

    // Speak specific text according to the frontend
    if (text == "Table")
      text = "Please find the table below";
    else if (text.includes("Chart"))
      text = `Your ${text} is on the way`;

    // Delete # from markdown
    text = text.replace(/#/g, "");

    this.speechSynthesizer.speakTextAsync(text);
  }

  public tokenize(text: string): Observable<PyTokenize> {
    return this.httpClient.get<PyTokenize>(this.pythonServiceUrl + `/tokenize/${text}`);
  }

  public predict(currentData: PredictionData[]): Observable<PredictionData[]> {
    return this.httpClient.post<PredictionData[]>(this.pythonServiceUrl + '/predict', currentData);
  }

  public getChatbotAnswer(question: string) {
    let headers = new HttpHeaders({
      'Authorization': 'EndpointKey ' + this.qnaMakerEndpointKey
    });
    let options = { headers: headers };

    return this.httpClient.post(`${this.qnaMakerEndpoint}/qnamaker/knowledgebases/${this.kbId}/generateAnswer`, { 'question': question }, options);
  }

  public checkIsSql(question: string) {
    let isSql = false;
    question.split(' ').forEach(word => {
      if (this.sqlQueryWords.includes(word.toLowerCase())) {
        isSql = true;
        return;
      }
    });
    return isSql;
  }
}
