import { Component, OnInit, ViewChild, NgZone, Input, OnChanges, SimpleChanges, ɵɵqueryRefresh } from '@angular/core';
import { DisplayTable } from '../models/displayTable.model';
import { VisualizationService } from '../visualization.service';
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { ChartData } from '../models/chartData.model';
import { Answer } from '../models/Answer.model';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MaterialTableDataSource } from './material-table-datasource';
import { tap } from 'rxjs/operators';
declare var $;

am4core.useTheme(am4themes_animated);
@Component({
  selector: 'app-visualization',
  templateUrl: './visualization.component.html',
  styleUrls: ['./visualization.component.css']
})
export class VisualizationComponent implements OnInit, OnChanges {
    private XYChart: am4charts.XYChart;
    private amPieChart: am4charts.PieChart;
    private amRadarChart: am4charts.RadarChart;

    @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
    @ViewChild('sortData', { static: false }) sort: MatSort;
    dataSource: MaterialTableDataSource;
    displayedColumns = ['brand', 'categoryName', 'periodStart', 'sales'];

    @Input('answer') answer: Answer;
    @Input('question') question: string;
    private chartData: ChartData[] = [];
    private tableData: DisplayTable[] = [];
    private measurable: string;
    private answerText = '';
    private prompts: string[] = [];

    private showTable = false;
    private showChart = false;
    constructor(private visualization: VisualizationService, private zone: NgZone) { }

    ngOnInit() {
        this.dataSource = new MaterialTableDataSource();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.question) {
            if (changes.question.currentValue != undefined) {
                this.handleQuestion();
            }
        }
        if (changes.answer) {
            if (changes.answer.currentValue != undefined) {
                this.handleAnswer();
            }
        }
    }

     handleQuestion() {
         // Reset UI when question is asked
         this.answerText = "";
         this.prompts = [];

         if (this.showChart) {
             //    this.zone.runOutsideAngular(() => {
             //        am4core.disposeAllCharts();
             //});
             this.chartData = [];
             this.showChart = false;
         }
         else if (this.showTable) {
             this.showTable = false;
             this.dataSource.disconnect();
             this.tableData = [];
         }
    }

    handleAnswer() {
        console.log("Response: ", this.answer);
        const res = this.answer;

        // Get measurable
        if (res.listMeasures)
            this.measurable = res.listMeasures[0];

        // Check if there is data in the response
        if (res.queryResult) {
            // If query is too large to visualize
            if (res.queryResult.length > 3000) {
                console.log('query: ' + res.query);
                console.log(res.listMeasures);
                console.log(res.queryResult);
                this.tableData = res.queryResult;
                this.answerText = "This query result is suitable only for a table!"
                this.tableVisualize();
                return;
            }
        }

        const botAnswer = res.qna.answers[0].answer;
        // Query can be visualized in both table or chart
        switch (botAnswer) {
            case "Table":
                this.tableData = res.queryResult;
                console.log("tableData: ", this.tableData);
                this.tableVisualize();
                break;
            case "What type of chart?":
                this.chartData = res.queryResult;
                this.prepareChartData();
                break;
            case "Bar Chart":
                this.BarChart();
                this.answerText = "Your Bar Chart is on the way!";
                break;
            case "Pie Chart":
                this.PieChart();
                this.answerText = "Your Pie Chart is on the way!";
                break;
            case "Radar Chart":
                this.radarChart();
                this.answerText = "Your Radar Chart is on the way!";
                break;
            case "3D Chart":
                this.BarChart3D();
                this.answerText = "Your 3D Chart is on the way!";
        }

        // Normal qna response without data
        this.answerText = botAnswer;
        if (res.qna.answers[0].context) {
            for (let prompt of res.qna.answers[0].context.prompts) {
                this.prompts.push(prompt.displayText);
            }
        }
    }

    private tableVisualize() {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        this.paginator.page
            .pipe(
                tap(() => this.loadTDataPage())
            ).subscribe();

        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

        this.sort.sortChange
            .pipe(
                tap(() => this.dataSource.getSortedData(this.dataSource.data))
            ).subscribe();

        this.dataSource.loadData(this.tableData);
        this.showTable = true;
    }

    loadTDataPage() {
        this.dataSource.getPagedData(this.dataSource.data);
    }

    private prepareChartData() {
        this.chartData.forEach(cd => {
            cd.timestamp = new Date(cd.perioD_START).getTime();
        });
        console.log("Chart Data: ", this.chartData);
    }

    private BarChart() {
        this.zone.runOutsideAngular(() => {
            let chart = am4core.create("chartdiv", am4charts.XYChart);
            am4core.options.minPolylineStep = 5;

            chart.data = this.chartData;

            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.CandlestickSeries());
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "perioD_START";

            series.tooltipText = "{valueY.value}";
            chart.cursor = new am4charts.XYCursor();

            this.XYChart = chart;
            this.showChart = true;
        });
    }

    private BarChart3D() {
        this.zone.runOutsideAngular(() => {
            let chart = am4core.create("chartdiv", am4charts.XYChart);
            am4core.options.minPolylineStep = 5;

            chart.data = this.chartData;

            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.ColumnSeries3D());
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "timestamp";

            series.tooltipText = "{valueY.value}";
            chart.cursor = new am4charts.XYCursor();

            this.XYChart = chart;
            this.showChart = true;
        });
    }


    private PieChart() {
        this.zone.runOutsideAngular(() => {
            let chart = am4core.create("chartdiv", am4charts.PieChart);
            am4core.options.minPolylineStep = 5;

            chart.data = this.chartData;

            let pieSeries = chart.series.push(new am4charts.PieSeries());
            pieSeries.dataFields.value = "m_SALES_VALUE";
            pieSeries.dataFields.category = "perioD_START"; 

            this.amPieChart = chart;
            this.showChart = true;
        });
    }

    private radarChart() {
        this.zone.runOutsideAngular(() => {
            let chart = am4core.create("chartdiv", am4charts.RadarChart);
            am4core.options.minPolylineStep = 5;

            chart.data = this.chartData;

            let dateAxis = chart.xAxes.push(new am4charts.DateAxis() as any);
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis() as any);
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.RadarSeries());
            series.name = "Sales";
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "perioD_START";

            this.amRadarChart = chart;  
            this.showChart = true;
        });
            }
}
