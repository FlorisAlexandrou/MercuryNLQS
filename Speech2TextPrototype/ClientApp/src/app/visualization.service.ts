import { Injectable } from '@angular/core';
import { ResponseAnswer } from './AnswerInterface';
import { BehaviorSubject } from 'rxjs';
import { EventEmitter } from 'events';


@Injectable({
  providedIn: 'root'
})
export class VisualizationService {
    private responseAnswer: ResponseAnswer;
    public questionEvent = new EventEmitter();

    private answerSource = new BehaviorSubject(this.responseAnswer);
    currentAnswer = this.answerSource.asObservable();

    constructor() { }

    // Pass answer object between components
    onReceiveAnswer(answer: ResponseAnswer) {
        this.answerSource.next(answer);
    }

    onWaitForAnswer(answer: ResponseAnswer) {
        this.answerSource.next(answer);
    }

}
