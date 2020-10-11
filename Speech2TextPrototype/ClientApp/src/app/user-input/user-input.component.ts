import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '../api.service';
import { Answer } from '../models/Answer.model'
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-UserInput-component',
  templateUrl: './user-input.component.html',
  styleUrls: ['./user-input.component.scss']
})
export class UserInputComponent implements OnInit, OnDestroy{
    private resultSpeech2Text: string;
    private thinking: boolean = false;
    private listening: boolean = false;
    public voiceOutput: boolean = false;
    private responseAnswer: Answer;
    private question: string;
    private submittedQuestion: string;
    private sqlQuery = '';
    private subscriptions: Subscription[] = [];
    private debug: boolean = false;
    questionFC = new FormControl('');
    constructor(private apiService: ApiService) { }

    ngOnInit() { }

    public Speech2Text() {
        this.listening = true;
        this.subscriptions.push(this.apiService.getText().subscribe((res) => {
            this.resultSpeech2Text = res
            this.listening = false;
            if (res && res != "Speech could not be recognized.") {
                this.questionFC.setValue(res);
                this.getAnswer();
            }
        }));
    }

    public getAnswer() {
        const _question = this.questionFC.value;
        if (_question) {
            this.submittedQuestion = _question;
            this.questionFC.setValue('');
            this.questionFC.markAsUntouched();
            this.questionFC.markAsPristine();
            this.thinking = true;
            this.subscriptions.push(this.apiService.getAnswer(_question, this.sqlQuery, this.voiceOutput).subscribe((res) => {
                this.responseAnswer = res;
                this.thinking = false;
            }));
        }
    }

    public toggleVoice(event: MatSlideToggleChange) {
        this.voiceOutput = event.checked;
    }

    // Logging for testing
    public log(value: string) {
        console.log(value);
    }

    public answerPrompt(answerToQuestion: string) {
        this.questionFC.setValue(answerToQuestion);
        this.getAnswer();
    }

    public saveQuery(query: string) {
        this.sqlQuery = query;
    }

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }
}
