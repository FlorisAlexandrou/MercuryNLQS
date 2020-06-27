import { Component, OnInit, ViewChild, NgZone } from '@angular/core';
import { TData } from '../voice/AnswerInterface';
import { VisualizationService } from '../visualization.service';
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
declare var $;

am4core.useTheme(am4themes_animated);
@Component({
  selector: 'app-visualization',
  templateUrl: './visualization.component.html',
  styleUrls: ['./visualization.component.css']
})
export class VisualizationComponent implements OnInit {
    private chart: am4charts.XYChart;
    @ViewChild('DTable', { static: false }) table;
    dataTable: any;

    private queryResult: TData[] = [];
    private tableData: TData[] = [];
    private answer: string;
    private prompts: string[] = [];

    private showTable = false;
    constructor(private visualization: VisualizationService, private zone: NgZone) { }

    ngOnInit() {
        this.visualization.currentAnswer.subscribe(res => {
            // Reset UI when question is asked
            if (res == null) {
                this.tableData = [];
                this.showTable = false;
                this.answer = "";
                this.prompts = [];
                if (this.dataTable) {
                    this.dataTable.DataTable().destroy();
                }
                //console.log(this.chart);
                //if (this.chart.data) {
                //    this.chart.clearCache();
                //    this.chart = null;
                //}
            }

            // If query can be visualized
            else if (res.qna.answers && res.queryResult.length > 0) {
                console.log(res.queryResult);
                this.queryResult = res.queryResult;
                this.answer = res.qna.answers[0].answer;
            }

            // If user asked a normal question
            else if (res.qna.answers) {
                console.log(res.qna.answers[0].answer);
                this.answer = res.qna.answers[0].answer;
                if (res.qna.answers[0].context) {
                    for (let prompt of res.qna.answers[0].context.prompts) {
                        this.prompts.push(prompt.displayText);
                    }
                }
            }

            // If query is too large to visualize
            else if (res.queryResult) {
                console.log('query: ' + res.query);
                console.log(res.listMeasures);
                console.log(res.queryResult);
                this.tableData = res.queryResult;
                this.answer = "This query result is suitable only for a table!"
                this.tableVisualize();
            }

            if (this.answer == 'Bar Chart') {
                this.BarChart();
                this.answer = "Your Visualization is on the way!";
            }
        });
    }


    private BarChart() {
        this.zone.runOutsideAngular(() => {
            let chart = am4core.create("chartdiv", am4charts.XYChart);

            chart.data = this.queryResult;

            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.CandlestickSeries());
            series.dataFields.valueY = "m_SALES_VALUE"
            series.dataFields.dateX = "perioD_START";

            this.chart = chart;
        });
    }

    private tableVisualize() {
        // Set a tiny timeout so that angular can populate the table before turning it into a DataTable
        setTimeout(() => {
            this.dataTable = $(this.table.nativeElement);
            this.dataTable.DataTable();
            this.showTable = true;
        }, 1)
    }
}
