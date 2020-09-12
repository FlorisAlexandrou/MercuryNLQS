import { Component, OnInit } from '@angular/core';
import { ApiService } from '../api.service';
import { Answer } from '../models/Answer.model'

import { VisualizationService } from '../visualization.service';


@Component({
  selector: 'app-UserInput-component',
  templateUrl: './user-input.component.html',
  styleUrls: ['./user-input.component.scss']
})
export class UserInputComponent implements OnInit{
    private resultSpeech2Text: string;
    private thinking: boolean = false;
    private listening: boolean = false;
    public voiceOutput: boolean = false;
    private responseAnswer: Answer;
    private question: string;
    private submittedQuestion: string;
    private emptyAnswer: Answer = {}


    private debug: boolean = false;

    constructor(private apiService: ApiService, private visualization: VisualizationService) { }

    ngOnInit() {
        this.visualization.currentAnswer.subscribe(answer => this.responseAnswer = answer);
    }

    public Speech2Text() {
        this.listening = true;
        this.apiService.getText().subscribe((res) => {
            this.resultSpeech2Text = res
            this.listening = false;
            this.getAnswer(res);
        });
    }

    public getAnswer(question: string) {
        if (question) {
            this.submittedQuestion = question;
            this.thinking = true;
            this.apiService.getAnswer(question, this.voiceOutput).subscribe((res) => {
                this.responseAnswer = res;
                this.thinking = false;
            });
        }
    }

    // For testing only
    public textToSpeech(text: string) {
        this.apiService.textToSpeech(text).subscribe((res) => {
        });
    }

    public onVoiceOutput(value: boolean) {
        this.voiceOutput = value;
    }

    // Logging for testing
    public log(value: string) {
        console.log(value);
    }
}
