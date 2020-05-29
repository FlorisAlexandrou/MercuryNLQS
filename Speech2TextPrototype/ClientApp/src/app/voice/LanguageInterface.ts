export interface Language {
    text: string;
    entities: Entity[];
}

export interface Entity {
    text: string;
    category: string;
    subcategory: string;
    length: number;
    confidence: number;
}
