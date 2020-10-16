import { Component, OnInit, ViewChild, NgZone, Input, OnChanges, SimpleChanges, OnDestroy, Output, EventEmitter, ChangeDetectionStrategy, ElementRef, ViewChildren, QueryList, AfterViewInit } from '@angular/core';
import { DisplayTable } from '../models/displayTable.model';
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import { ChartData } from '../models/chartData.model';
import { Answer } from '../models/Answer.model';
import { Conversation } from '../models/conversation.model';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { VisualizationDataSource } from './visualization-datasource';
import { tap } from 'rxjs/operators';
import { ApiService } from '../api.service';
import { Subscription } from 'rxjs';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-visualization',
  templateUrl: './visualization.component.html',
  styleUrls: ['./visualization.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisualizationComponent implements OnInit, OnChanges, OnDestroy, AfterViewInit {
    private XYChart: am4charts.XYChart;
    private amPieChart: am4charts.PieChart;
    private amRadarChart: am4charts.RadarChart;

    @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
    @ViewChild('sortData', { static: false }) sort: MatSort;
    dataSource: VisualizationDataSource;
    displayedColumns = ['brand', 'categoryName', 'productName', 'periodStart', 'sales'];

    @Input('answer') answer: Answer;
    @Input('question') question: string;
    @Input('uuid') uuid: string;
    @Output('promptAnswered') promptAnswered = new EventEmitter<string>();
    @Output('sqlQueryComposed') sqlQueryComposed = new EventEmitter<string>();
    @ViewChild('scrollView', { static: false }) scrollView: CdkVirtualScrollViewport;
    @ViewChildren('messages') messages: QueryList<any>;

    generatedQueryText = '';
    allConversations: Conversation[] = [];
    conversationIndex = 0;
    private chartData: ChartData[] = [];
    private tableData: DisplayTable[] = [];
    private measurable = '';
    private prompts: string[] = [];
    private showTable = false;
    private showChart = false;
    private subscriptions: Subscription[] = [];

    constructor(private zone: NgZone, private api: ApiService) { }

    ngOnInit() {
        this.dataSource = new VisualizationDataSource(this.api, this.uuid);
        window.onbeforeunload = () => this.ngOnDestroy();
    }

    ngAfterViewInit() {
        this.messages.changes.subscribe(this.scrollToBottom);
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
        const _conversation: Conversation = { question: this.question, answer: '' };
        this.allConversations = [...this.allConversations, _conversation];
         // Reset UI when question is asked
         this.prompts = [];
         if (this.showChart) {
             //    this.zone.runOutsideAngular(() => {
             //        am4core.disposeAllCharts();
             //});
             this.chartData = [];
             this.showChart = false;
             this.generatedQueryText = '';
         }
         else if (this.showTable) {
             this.showTable = false;
             this.dataSource.disconnect();
             this.subscriptions.forEach(sub => sub.unsubscribe());
             this.tableData = [];
             this.generatedQueryText = '';
             this.dataSource = new VisualizationDataSource(this.api, this.uuid);
        }
    }

    handleAnswer() {
        console.log("Response: ", this.answer);
        const res = this.answer;

        // Get measurable
        if (res.listMeasures.length > 0)
            this.measurable = res.listMeasures[0];

        // Get SQL Query
        if (res.sqlQuery) {
            this.generatedQueryText = res.sqlQuery;
            this.sqlQueryComposed.emit(this.generatedQueryText);
        }

        // Get Scalar Value (result of "sum", "avg" etc)
        if (res.scalar > 0) {
            this.allConversations[this.conversationIndex].answer = res.scalar.toString();
            this.conversationIndex++;
            return;
        }

        // Check if there is data in the response
        if (res.queryResult) {
            // If query is too large to visualize
            if (res.queryResult.length > 3000) {
                console.log(res.listMeasures);
                console.log(res.queryResult);
                this.tableData = res.queryResult;
                this.allConversations[this.conversationIndex].answer = "A query with more than 3000 rows is suitable only for a table!"
                this.conversationIndex++;
                this.tableVisualize();
                return;
            }
        }

        const botAnswer = res.qna.answers[0].answer;
        // Query can be visualized in both table or chart
        switch (botAnswer) {
            case "What type of chart?":
                this.chartData = res.queryResult;
                this.prepareChartData();
                break;
            case "Table":
                this.tableData = res.queryResult;
                console.log("tableData: ", this.tableData);
                this.tableVisualize();
                return;
            case "Bar Chart":
                this.onShowChart(botAnswer);
                this.barChart();
                return;
            case "Pie Chart":
                this.onShowChart(botAnswer);
                this.pieChart();
                return;
            case "Radar Chart":
                this.onShowChart(botAnswer);
                this.radarChart();
                return;
            case "3D Chart":
                this.onShowChart(botAnswer);
                this.barChart3D();
                return;
            case "Line Chart":
                this.onShowChart(botAnswer);
                this.lineChart();
                return;
        }

        // Normal qna response without data
        this.allConversations[this.conversationIndex].answer = botAnswer;
        this.conversationIndex++;
        if (res.qna.answers[0].context) {
            for (let prompt of res.qna.answers[0].context.prompts) {
                this.prompts.push(prompt.displayText);
            }
        }
    }

    private tableVisualize() {
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;

        this.subscriptions.push(this.paginator.page.pipe(tap(() => this.loadTDataPage())).subscribe());
        this.subscriptions.push(this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0));
        this.subscriptions.push(this.sort.sortChange.pipe(tap(() => this.dataSource.getSortedData(this.dataSource.data))).subscribe());
        console.log(this.tableData);
        this.dataSource.loadData(this.tableData, this.measurable);
        this.showTable = true;
        this.allConversations[this.conversationIndex].answer = 'Please find the table below!'
        this.conversationIndex++;
    }

    loadTDataPage() {
        this.dataSource.getPagedData(this.dataSource.data);
    }

    private prepareChartData() {
        this.chartData.forEach(cd => {
            cd.timestamp = new Date(cd.perioD_START).getTime();
        });
        this.chartData.sort((a, b) => (a.timestamp > b.timestamp) ? 1 : -1);
        console.log("Chart Data: ", this.chartData);
    }

    private createChart(chartType: string) {
        let chart;
        switch (chartType) {
            case "XYChart":
                chart = am4core.create("chartdiv", am4charts.XYChart);
                break;
            case "RadarChart":
                chart = am4core.create("chartdiv", am4charts.RadarChart);
                break;
            case "PieChart":
                chart = am4core.create("chartdiv", am4charts.PieChart);
        }

        chart.exporting.menu = new am4core.ExportMenu();
        am4core.options.minPolylineStep = 5;
        chart.svgContainer.htmlElement.style.height = "60vh";
        chart.svgContainer.htmlElement.style.width = "100%";
        chart.data = this.chartData;
        return chart;
    }

    private barChart() {
        this.zone.runOutsideAngular(() => {
            let chart = this.createChart('XYChart');
            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.ColumnSeries());
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "timestamp";
            series.columns.template.tooltipText = "{valueY.value}";

            let scrollbarX = new am4charts.XYChartScrollbar();
            scrollbarX.series.push(series);
            chart.scrollbarX = scrollbarX;

            this.XYChart = chart;
            this.showChart = true;
        });
    }

    private barChart3D() {
        this.zone.runOutsideAngular(() => {
            let chart = this.createChart("XYChart");

            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.ColumnSeries3D());
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "timestamp";

            series.columns.template.tooltipText = "{valueY.value}";

            this.XYChart = chart;
            this.showChart = true;
        });
    }


    private pieChart() {
        this.zone.runOutsideAngular(() => {
            let chart = this.createChart("PieChart");

            let pieSeries = chart.series.push(new am4charts.PieSeries());
            pieSeries.dataFields.value = "m_SALES_VALUE";
            pieSeries.dataFields.category = "timestamp"; 

            this.amPieChart = chart;
            this.showChart = true;
        });
    }

    private radarChart() {
        let chart = this.createChart('RadarChart');
        let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        dateAxis.title.text = "Dates";
        let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.title.text = "Sales";
        let series = chart.series.push(new am4charts.RadarSeries());
        series.dataFields.valueY = "m_SALES_VALUE";
        series.dataFields.dateX = "timestamp";
        series.tooltipText = "{valueY.value}"
        chart.cursor = new am4charts.RadarCursor();
        this.amRadarChart = chart;
        this.showChart = true;
    }

    private lineChart() {
        this.zone.runOutsideAngular(() => {
            let chart = this.createChart('XYChart');
            let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.title.text = "Dates";

            let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Sales";

            let series = chart.series.push(new am4charts.LineSeries());
            series.dataFields.valueY = "m_SALES_VALUE";
            series.dataFields.dateX = "timestamp";
            series.tooltipText = "{value}"
            series.strokeWidth = 2;
            series.minBulletDistance = 15;

            // Drop-shaped tooltips
            series.tooltip.background.cornerRadius = 20;
            series.tooltip.background.strokeOpacity = 0;
            series.tooltip.pointerOrientation = "vertical";
            series.tooltip.label.minWidth = 40;
            series.tooltip.label.minHeight = 40;
            series.tooltip.label.textAlign = "middle";
            series.tooltip.label.textValign = "middle";

            let bullet = series.bullets.push(new am4charts.CircleBullet());
            bullet.circle.strokeWidth = 2;
            bullet.circle.radius = 4;
            bullet.circle.fill = am4core.color("#fff");

            let bullethover = bullet.states.create("hover");
            bullethover.properties.scale = 1.3;
            this.XYChart = chart;

            chart.cursor = new am4charts.XYCursor();
            chart.cursor.behavior = "panXY";
            chart.cursor.xAxis = dateAxis;
            chart.cursor.snapToSeries = series;

            let scrollbarX = new am4charts.XYChartScrollbar();
            scrollbarX.series.push(series);
            chart.scrollbarX = scrollbarX;

            this.showChart = true;
        });
    }

    private onShowChart(chartName: string) {
        this.allConversations[this.conversationIndex].answer = `Your ${chartName} is on the way!`
        this.conversationIndex++;
    }

    public answerPrompt(prompt: string) {
        this.promptAnswered.emit(prompt);
    }

    scrollToBottom = () => {
        try {
            this.scrollView.scrollToIndex(this.allConversations.length, "smooth");
        } catch (err) { }
    }


    ngOnDestroy() {
        // Clear table on user exit/page refresh
        this.api.deleteTable(this.uuid).subscribe();
        this.subscriptions.forEach(sub => sub.unsubscribe());
        this.zone.runOutsideAngular(() => {
            if (this.XYChart) {
                this.XYChart.dispose();
            }
            if (this.amPieChart)
                this.amPieChart.dispose();
        });
    }
}
