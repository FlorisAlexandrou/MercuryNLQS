import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '../api.service';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription, Observable } from 'rxjs';
import { FormControl } from '@angular/forms';
import { startWith, map, finalize } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CognitiveService } from '../cognitive.service';
import { SqlAnswer } from '../models/sqlAnswer.model';
import { qna } from '../models/qna.model';
import { DisplayTable } from '../models/displayTable.model';

@Component({
  selector: 'app-UserInput-component',
  templateUrl: './user-input.component.html',
  styleUrls: ['./user-input.component.scss']
})
export class UserInputComponent implements OnInit, OnDestroy {
  public thinking: boolean = false;
  public listening: boolean = false;
  public voiceOutput: boolean = false;
  public chatbotAnswer: qna;
  public groupByAnswer: DisplayTable[] = [];
  public sqlAnswer: SqlAnswer;
  public submittedQuestion: string;
  public questions: string[] = [];
  private sqlQuery = '';
  private subscriptions: Subscription[] = [];
  questionFC = new FormControl();
  public uuid = '';
  autoCompleteQuestions: Observable<string[]>;

  constructor(private apiService: ApiService, private _snackBar: MatSnackBar, private cognitiveService: CognitiveService) { }

  ngOnInit() {
    this.generate_UUID();

    this.autoCompleteQuestions = this.questionFC.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value))
    );
  }

  /** Turn speech to text and submit the question */
  public async Speech2Text() {
    if (this.listening)
      return;
    this.listening = true;

    await this.cognitiveService.speechToText().then((res: string) => {
      this.questionFC.setValue(res);
      this.getAnswer();
    })
      .catch((res: string) => {
        this._snackBar.open(res, "okay", { duration: 3000 });
      })
      .finally(() => this.listening = false);
  }

  /** Submit user query and get answer which is detected by the visualization component */
  public getAnswer() {
    const _question = this.questionFC.value;
    if (_question && !this.thinking) {
      this.submittedQuestion = _question;
      this.thinking = true;
      let isSql = this.cognitiveService.checkIsSql(_question);

      if (isSql) {
        // Tokenize with python
        this.subscriptions.push(this.cognitiveService.tokenize(_question).subscribe(pyRes => {
          // Get query results
          this.subscriptions.push(this.apiService.getSqlAnswer(pyRes).subscribe((res: SqlAnswer) => {
            this.sqlAnswer = res;
            this.thinking = false;
          }));
        }));
      }
      else {
        this.getChatbotAnswer(_question);
      }


      if (!this.questions.includes(_question))
        this.questions.push(_question);
      this.questionFC.reset();
    }
  }

  public getChatbotAnswer(question: string) {
    this.thinking = true;
    this.subscriptions.push(this.cognitiveService.getChatbotAnswer(question).subscribe((res: qna) => {
      this.chatbotAnswer = res;
      if (this.voiceOutput)
        this.cognitiveService.textToSpeech(res.answers[0].answer);
    },
      (error) => {
        this._snackBar.open("Something went wrong! Please try again and check your internet connection.", "okay", { duration: 3000 });
      },
      () => this.thinking = false));
  }

  public groupBy(groupByFilter: string) {
    this.thinking = true;
    this.subscriptions.push(this.apiService.getGroupByAnswer(this.sqlQuery, groupByFilter, this.uuid).subscribe((error: string) => {
      if (error) {
        this.getChatbotAnswer(error);
        return;
      }

      this.getChatbotAnswer("Output Type");
      this.thinking = false;
    }));
  }

  /** Switch text to speech on and off */
  public toggleVoice(event: MatSlideToggleChange) {
    this.voiceOutput = event.checked;
  }

  /** Get the prompt value and submit it as a question */
  public answerPrompt(answerToQuestion: string) {
    this.questionFC.setValue(answerToQuestion);
    this.getAnswer();
  }

  /**
   * Save the query to pass back to backend
   * @param query The SQL query received from the backend
   */
  public saveQuery(query: string) {
    this.sqlQuery = query;
  }

  /** Generate a unique id for each user to allow concurrent use of the display table */
  private generate_UUID() {
    var dt = new Date().getTime();
    this.uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      var r = (dt + Math.random() * 16) % 16 | 0;
      dt = Math.floor(dt / 16);
      return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
  }

  /**
   * 
   * @param value
   */
  private _filter(value: string): string[] {
    if (value) {
      const filterValue = this._normalizeValue(value);
      return this.questions.filter(question => this._normalizeValue(question).includes(filterValue));
    }
  }

  private _normalizeValue(value: string): string {
    return value.toLowerCase().replace(/\s/g, '');
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
