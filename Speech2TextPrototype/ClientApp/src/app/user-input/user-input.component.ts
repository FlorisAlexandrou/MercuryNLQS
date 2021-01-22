import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '../api.service';
import { Answer } from '../models/Answer.model'
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription, Observable } from 'rxjs';
import { FormControl } from '@angular/forms';
import { startWith, map } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-UserInput-component',
  templateUrl: './user-input.component.html',
  styleUrls: ['./user-input.component.scss']
})
export class UserInputComponent implements OnInit, OnDestroy {
  public thinking: boolean = false;
  public listening: boolean = false;
  public voiceOutput: boolean = false;
  public responseAnswer: Answer;
  public submittedQuestion: string;
  public questions: string[] = [];
  private sqlQuery = '';
  private subscriptions: Subscription[] = [];
  questionFC = new FormControl();
  public uuid = '';
  autoCompleteQuestions: Observable<string[]>;

  constructor(private apiService: ApiService, private _snackBar: MatSnackBar) { }

  ngOnInit() {
    this.generate_UUID();

    this.autoCompleteQuestions = this.questionFC.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value))
    );
  }

  /** Turn speech to text and submit the question */
  public Speech2Text() {
    if (this.listening)
      return;
    this.subscriptions.push(this.apiService.getText().subscribe((res) => {
      this.listening = false;
      if (res && res == "Speech could not be recognized.") {
        this._snackBar.open(res, "okay", {
          duration: 3000,
        });
        return;
      }
      this.questionFC.setValue(res);
      this.getAnswer();
    }));
    this.listening = true;
  }

  /** Submit user query and get answer which is detected by the visualization component */
  public getAnswer() {
    const _question = this.questionFC.value;
    if (_question && !this.thinking) {
      this.submittedQuestion = _question;
      this.thinking = true;
      this.subscriptions.push(this.apiService.getAnswer(_question, this.sqlQuery, this.uuid, this.voiceOutput).subscribe((res) => {
        this.responseAnswer = res;
        this.thinking = false;
      },
        (error) => {
          this.thinking = false;
          this._snackBar.open("Something went wrong! Please try again and check your internet connection.", "okay", { duration: 3000 });
        }));
      if (!this.questions.includes(_question))
        this.questions.push(_question);
      this.questionFC.reset();
    }
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

  /**
   * Prompt system to ask user if they want a prediction
   * @param text
   */
  public askPrediction(text: string) {
    this.thinking = true;
    this.subscriptions.push(this.apiService.getAnswer(text, this.sqlQuery, this.uuid, this.voiceOutput).subscribe((res) => {
      this.responseAnswer = res;
      this.thinking = false;
    },
      (error) => {
        this.thinking = false;
        this._snackBar.open("Something went wrong! Please try again and check your internet connection.", "okay", { duration: 3000 });
      }));
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
