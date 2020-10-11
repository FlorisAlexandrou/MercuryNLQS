import { DisplayTable } from "./displayTable.model";
import { qna } from "./qna.model";

export interface Answer {
    qna?: qna;
    queryResult?: DisplayTable[];
    listMeasures?: string[];
    sqlQuery?: string;
    scalar?: number;
}
