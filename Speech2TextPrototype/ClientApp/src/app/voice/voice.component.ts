import { Component } from '@angular/core';
import { ApiService } from '../api.service';
import { SalesValue } from './AnswerInterface'
import { error } from '@angular/compiler/src/util';

@Component({
  selector: 'app-voice-component',
  templateUrl: './voice.component.html',
  styleUrls: ['./voice.component.scss']
})
export class VoiceComponent {

    private resultSpeech2Text: string;
    private answer: string;
    private prompts: string[] = [];
    private thinking: boolean = false;
    private listening: boolean = false;
    private queryResult: SalesValue[];
    public voiceOutput: boolean = false;

    private debug: boolean = false;

    constructor(private apiService: ApiService) { }

    public Speech2Text() {
        this.listening = true;
        this.apiService.getText().subscribe((res) => {
            console.log(res);
            this.resultSpeech2Text = res
            this.listening = false;
            this.getAnswer(res);
        });
    }


    public getAnswer(question: string) {
        this.answer = "";
        this.queryResult = [];
        this.thinking = true;
        this.prompts = [];

        this.apiService.getAnswer(question, this.voiceOutput).subscribe((res) => {
            if (res.qna.answers) {
                this.answer = res.qna.answers[0].answer;
                if (res.qna.answers[0].context) {
                    for (let prompt of res.qna.answers[0].context.prompts) {
                        this.prompts.push(prompt.displayText);
                    }
                }
            }
            else {
                this.queryResult = res.queryResult;
                this.answer = "Showing the first 10 results"
            }
            this.thinking = false;
        }, (error) => {
            console.log(error);
            this.thinking = false;
        });
    }

    public textToSpeech(text: string) {
        this.apiService.textToSpeech(text).subscribe((res) => {
        });
    }

    public onVoiceOutput(value: boolean) {
        this.voiceOutput = value;
    }

}
