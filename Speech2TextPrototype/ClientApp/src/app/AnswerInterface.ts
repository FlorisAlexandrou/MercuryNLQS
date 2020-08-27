export interface ResponseAnswer {
    qna?: qna;
    queryResult?: TData[];
    listMeasures?: string[];
    query?: string;
}

export interface TData {
    tid: number;
    quanitity: number;
    price: number;
    categorY_ID: number;
    categorY_NAME: string;
    uniT_MEASUREMENT: string;
    producT_ID: number;
    product_NAME: string;
    brand: string;
    size: number;
    sizE_DETAILS: string;
    perioD_ID: number;
    perioD_START: Date;
    perioD_END: Date;
    outleT_ID: number;
    outleT_NAME: string;
    outleT_TYPE_ID: number;
    outleT_TYPE_NAME: string;
    areA_ID: number;
    areA_NAME: string;
    m_SALES_ITEMS: number;
    m_SALES_VALUE: number;
    m_SALES_VOLUME: number;
}

interface qna {
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
