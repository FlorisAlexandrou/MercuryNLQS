export interface qna {
    answers: Answer[];
}

 interface Answer {
    answer: string;
    context: Context;
    questions: string[];
    score: number;
}

 interface Prompt {
    displayOrder: number;
    qnaId: number;
    qna: string;
    displayText: string;
}

interface Context {
    isContextOnly: boolean;
    prompts: Prompt[];
}
