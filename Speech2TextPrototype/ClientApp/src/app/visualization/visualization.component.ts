import { Component, OnInit, ViewChild, NgZone } from '@angular/core';
import { DisplayTable } from '../models/displayTable.model';
import { VisualizationService } from '../visualization.service';
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { ChartData } from '../models/chartData.model';
declare var $;

am4core.useTheme(am4themes_animated);
@Component({
  selector: 'app-visualization',
  templateUrl: './visualization.component.html',
  styleUrls: ['./visualization.component.css']
})
export class VisualizationComponent implements OnInit {
    private XYChart: am4charts.XYChart;
    private amPieChart: am4charts.PieChart;
    private amRadarChart: am4charts.RadarChart;
    @ViewChild('DTable', { static: false }) table;
    dataTable: any;

    private chartData: ChartData[] = [];
    private tableData: DisplayTable[] = [];
    private measurable: string;
    private answer: string;
    private prompts: string[] = [];
    private test = 'row.m_SALES_VALUE';

    private showTable = false;
    private showChart = false;
    constructor(private visualization: VisualizationService, private zone: NgZone) { }

    ngOnInit() {
        this.visualization.currentAnswer.subscribe(res => {
            console.log(res);
            // Reset UI when question is asked
            if (res == null) {

                if (this.showChart) {
                //    this.zone.runOutsideAngular(() => {
                //        am4core.disposeAllCharts();
                //});
                    this.chartData = [];
                    this.showChart = false;
                }

                this.answer = "";
                this.prompts = [];
                if (this.dataTable) {
                    this.showTable = false;
                    this.dataTable.DataTable().destroy();
                }
            }

            // If query can be visualized
            else if (res.qna.answers && res.queryResult.length > 0) {
                console.log('query: ' + res.query);
                //console.log(res.queryResult);
                this.measurable = res.listMeasures[0];
                this.chartData = res.queryResult;
                this.tableData = res.queryResult;
                console.log(this.tableData);
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
                this.measurable = res.listMeasures[0];
                console.log('query: ' + res.query);
                console.log(res.listMeasures);
                console.log(res.queryResult);
                this.tableData = res.queryResult;
                this.answer = "This query result is suitable only for a table!"
                this.tableVisualize();
            }

            switch (this.answer) {
                case "Table":
                    this.tableVisualize();
                    break;
                case "What type of chart?":
                    this.prepareChartData();
                    break;
                case "Bar Chart":
                    this.BarChart();
                    this.answer = "Your Visualization is on the way!";
                    break;
                case "Pie Chart":
                    this.PieChart();
                    this.answer = "Your Visualization is on the way!";
                    break;
                case "Radar Chart":
                    this.radarChart();
                    this.answer = "Your Visualization is on the way!";
                    break;
                case "3D Chart":
                    this.BarChart3D();
                    this.answer = "Your Visualization is on the way!";
            }
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
