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
export class UserInputComponent implements OnInit, OnDestroy{
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
    filteredQuestions: Observable<string[]>;

  constructor(private apiService: ApiService, private _snackBar: MatSnackBar) { }

    ngOnInit() {
      this.generate_UUID();
      this.filteredQuestions = this.questionFC.valueChanges.pipe(
        startWith(''),
        map(value => this._filter(value))
      );
    }

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
                this._snackBar.open("Something went wrong! Please try again and check your internet connection.", "okay", {duration: 3000});
              }));
            if (!this.questions.includes(_question))
                this.questions.push(_question);
            this.questionFC.reset();
        }
    }

    public toggleVoice(event: MatSlideToggleChange) {
        this.voiceOutput = event.checked;
    }

    public answerPrompt(answerToQuestion: string) {
        this.questionFC.setValue(answerToQuestion);
        this.getAnswer();
    }

    public saveQuery(query: string) {
        this.sqlQuery = query;
  }

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

    private generate_UUID() {
        var dt = new Date().getTime();
        this.uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            var r = (dt + Math.random() * 16) % 16 | 0;
            dt = Math.floor(dt / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }

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
