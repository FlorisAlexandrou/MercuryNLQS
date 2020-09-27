import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '../api.service';
import { Answer } from '../models/Answer.model'
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';

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

    private subscriptions: Subscription[] = [];
    private debug: boolean = false;

    constructor(private apiService: ApiService) { }

    ngOnInit() { }

    public Speech2Text() {
        this.listening = true;
        this.subscriptions.push(this.apiService.getText().subscribe((res) => {
            this.resultSpeech2Text = res
            this.listening = false;
            if (res && res != "Speech could not be recognized.")
              this.getAnswer(res);
        }));
    }

    public getAnswer(question: string) {
        if (question) {
            this.submittedQuestion = question;
            this.thinking = true;
            this.subscriptions.push(this.apiService.getAnswer(question, this.voiceOutput).subscribe((res) => {
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

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }
}
