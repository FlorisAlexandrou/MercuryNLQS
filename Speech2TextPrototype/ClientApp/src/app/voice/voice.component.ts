import { Component } from '@angular/core';
import { ApiService } from '../api.service';
import { Entity } from './LanguageInterface';

@Component({
  selector: 'app-voice-component',
  templateUrl: './voice.component.html'
})
export class VoiceComponent {

    private resultText: string;
    private entities: Entity[];
    private answer: string;
    private prompts: string[] = [];

    constructor(private apiService: ApiService) { }

    public Speech2Text() {
        this.resultText = "Listening..."
        this.apiService.getText().subscribe((res) => {
            console.log(res);
            console.log(res.entities)
            this.resultText = res.text;
            this.entities = res.entities;
        });
    }


    public getAnswer(question: string) {
        this.answer = "Thinking...";
        this.prompts = [];
        this.apiService.getAnswer(question).subscribe((res) => {
            console.log(res.answers[0]);
            this.answer = res.answers[0].answer;
            for (let prompt of res.answers[0].context.prompts) {
                this.prompts.push(prompt.displayText);
            }
        });
    }

    public textToSpeech(text: string) {
        this.apiService.textToSpeech(text).subscribe((res) => {
            console.log(res);
        });
    }

}
