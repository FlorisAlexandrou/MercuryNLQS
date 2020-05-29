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
}
