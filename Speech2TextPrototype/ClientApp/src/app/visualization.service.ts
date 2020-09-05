import { Injectable } from '@angular/core';
import { Answer } from './models/Answer.model';
import { BehaviorSubject } from 'rxjs';
import { EventEmitter } from 'events';


@Injectable({
  providedIn: 'root'
})
export class VisualizationService {
    private responseAnswer: Answer;
    public questionEvent = new EventEmitter();

    private answerSource = new BehaviorSubject(this.responseAnswer);
    currentAnswer = this.answerSource.asObservable();

    constructor() { }

    // Pass answer object between components
    onReceiveAnswer(answer: Answer) {
        this.answerSource.next(answer);
    }

    onWaitForAnswer(answer: Answer) {
        this.answerSource.next(answer);
    }

}
