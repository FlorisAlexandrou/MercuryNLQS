import { Component } from '@angular/core';
import { ApiService } from '../api.service';
import { Entity } from './LanguageInterface';
import { error } from '@angular/compiler/src/util';

@Component({
  selector: 'app-voice-component',
  templateUrl: './voice.component.html',
  styleUrls: ['./voice.component.scss']
})
export class VoiceComponent {

    private resultSpeech2Text: string;
    private entities: Entity[];
    private answer: string;
    private prompts: string[] = [];
    private thinking: boolean = false;
    private listening: boolean = false;
    public voiceOutput: boolean = false;

    private debug: boolean = false;

    constructor(private apiService: ApiService) { }

    public Speech2Text() {
        this.listening = true;
        this.apiService.getText().subscribe((res) => {
            console.log(res);
            this.resultSpeech2Text = res.text;
            this.entities = res.entities;
            this.listening = false;
            this.getAnswer(res.text);
        });
    }


    public getAnswer(question: string) {
        this.answer = "";
        this.thinking = true;
        this.prompts = [];

        this.apiService.getEntities(question).subscribe((res) => {
            this.entities = res;
        })

        this.apiService.getAnswer(question, this.voiceOutput).subscribe((res) => {
            console.log(res.answers[0]);
            this.answer = res.answers[0].answer;
            if (res.answers[0].context) {
                for (let prompt of res.answers[0].context.prompts) {
                    this.prompts.push(prompt.displayText);
                }
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
