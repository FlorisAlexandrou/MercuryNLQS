import {
  Component, OnInit, ViewChild, NgZone, Input,
  OnDestroy, Output, EventEmitter, ChangeDetectionStrategy,
  ElementRef, AfterViewChecked
} from '@angular/core';
import { DisplayTable } from '../models/displayTable.model';
import * as am4core from '@amcharts/amcharts4/core';
import * as am4charts from '@amcharts/amcharts4/charts';
import { ChartData } from '../models/chartData.model';
import { Conversation } from '../models/conversation.model';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { VisualizationDataSource } from './visualization-datasource';
import { tap } from 'rxjs/operators';
import { ApiService } from '../api.service';
import { Subscription } from 'rxjs';
import { PredictionData } from '../models/predictionData.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { qna } from '../models/qna.model';
import { SqlAnswer } from '../models/sqlAnswer.model';
import { CognitiveService } from '../cognitive.service';

@Component({
  selector: 'app-visualization',
  templateUrl: './visualization.component.html',
  styleUrls: ['./visualization.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisualizationComponent implements OnInit, OnDestroy, AfterViewChecked {
  dataSource: VisualizationDataSource;
  displayedColumns = ['brand', 'categoryName', 'productName', 'periodStart', 'sales'];

  private XYChart: am4charts.XYChart;
  private amPieChart: am4charts.PieChart;
  private amRadarChart: am4charts.RadarChart;

  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild('sortData') sort: MatSort;
  @ViewChild('scrollContainer') scrollContainer: ElementRef;


  @Input() uuid: string;
  @Input() set question(question: string) {
    if (question)
      this.handleQuestion(question);
  }
  @Input() set chatbotAnswer(chatbotAnswer: qna) {
    if (chatbotAnswer)
      this.handleChatbotAnswer(chatbotAnswer);
  }
  @Input() set sqlAnswer(sqlAnswer: SqlAnswer) {
    if (sqlAnswer)
      this.handleSqlAnswer(sqlAnswer);
  }

  @Output('promptAnswered') promptAnswered = new EventEmitter<string>();
  @Output('sqlQueryComposed') sqlQueryComposed = new EventEmitter<string>();
  @Output('groupBy') groupBy = new EventEmitter<string>();
  @Output('askChatbot') askChatbot = new EventEmitter<string>();


  private chartData: ChartData[] = [];
  private tableData: DisplayTable[] = [];
  private prompts: string[] = [];
  private subscriptions: Subscription[] = [];
  private tableSubscriptions: Subscription[] = [];
  private predictionAsked = false;
  private processedMeasurable = '';
  // limit to prevent memory overflow on heroku
  private maxPredictionSize = 20;

  public measurable = '';
  public showTable = false;
  public showChart = false;
  public loadingPrediction = false;
  public conversationIndex = 0;
  public generatedQueryText = '';
  public allConversations: Conversation[] = [];

  constructor(
    private zone: NgZone,
    private apiService: ApiService,
    private _snackBar: MatSnackBar,
    private cognitiveService: CognitiveService
  ) { }

  ngOnInit() {
    this.dataSource = new VisualizationDataSource(this.apiService, this.uuid);
    window.onbeforeunload = () => this.ngOnDestroy();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  handleQuestion(question: string) {
    const _conversation: Conversation = { question: question, answer: '' };
    this.allConversations = [...this.allConversations, _conversation];
    // Reset prompts when question is asked
    this.prompts = [];
  }

  private handleSqlAnswer(sqlAnswer: SqlAnswer) {
    // Get measurable
    if (sqlAnswer.measures.length > 0) {
      this.measurable = sqlAnswer.measures[0];
      // Fix measurable to match json key dynamically for prediction and charts
      this.processedMeasurable = 'm' + this.measurable.slice(1);
    }

    // Get SQL Query
    if (sqlAnswer.sqlQuery) {
      this.generatedQueryText = sqlAnswer.sqlQuery;
      this.sqlQueryComposed.emit(this.generatedQueryText);
    }

    // Get Scalar Value (result of "sum", "avg" etc)
    if (sqlAnswer.scalar > -1) {
      this.allConversations[this.conversationIndex].answer = sqlAnswer.scalar.toFixed(2).toString();
      this.conversationIndex++;
      return;
    }

    // Handle errors
    if (sqlAnswer.error) {
      this.askChatbot.emit(sqlAnswer.error);
    }
    else
      this.askChatbot.emit('Granularity Type');
  }

  private handleChatbotAnswer(chatbotAnswer: qna) {
    const botAnswer = chatbotAnswer.answers[0].answer;
    // Query can be visualized in both table or chart
    switch (botAnswer) {
      case "What type of chart?":
        // Get Chart Data
        this.subscriptions.push(this.apiService.getChartData(this.uuid).subscribe((res: ChartData[]) => {
          this.chartData = res;
          this.prepareChartData();
        }));
        break;
      case "Table":
        this.loadingPrediction = true;
        this.subscriptions.push(this.apiService.getTableData(this.uuid).subscribe((res: DisplayTable[]) => {
          this.tableData = res;
          this.tableVisualize();
          this.loadingPrediction = false;
        }));
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
      case "Prediction":
        if (this.predictionAsked)
          this.getPrediction();
        else
          this.askChatbot.emit("okay");
        return;
    }

    if (botAnswer == "M_SALES_VALUE" || botAnswer == "M_SALES_VOLUME" || botAnswer == "M_SALES_ITEMS") {
      this.measurable = botAnswer;
      // Fix measurable to match json key dynamically for prediction and charts
      this.processedMeasurable = 'm' + this.measurable.slice(1);
      this.askChatbot.emit('Granularity Type');
      return;
    }

    if (botAnswer == 'PRODUCT_NAME' || botAnswer == 'CATEGORY_NAME' || botAnswer == 'BRAND') {
      this.groupBy.emit(botAnswer);
      return;
    }

    // Normal qna response without data
    this.allConversations[this.conversationIndex].answer = botAnswer;
    this.conversationIndex++;
    if (chatbotAnswer.answers[0].context) {
      for (let prompt of chatbotAnswer.answers[0].context.prompts) {
        this.prompts.push(prompt.displayText);
      }
    }
  }

  private tableVisualize() {
    this.tableData.sort((a, b) => (a.perioD_START > b.perioD_START) ? 1 : -1);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.tableSubscriptions.push(this.paginator.page.pipe(tap(() => this.loadTDataPage())).subscribe());
    this.tableSubscriptions.push(this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0));
    this.tableSubscriptions.push(this.sort.sortChange.pipe(tap(() => this.dataSource.getSortedData(this.dataSource.data))).subscribe());
    this.dataSource.loadData(this.tableData, this.measurable);
    this.showTable = true;
    this.allConversations[this.conversationIndex].answer = 'Please find the table below!'
    this.conversationIndex++;
    if (this.tableData.length > 2 && this.tableData.length < this.maxPredictionSize) {
      this.askChatbot.emit('Ask Prediction')
      const _conversation: Conversation = { question: '', answer: '' };
      this.allConversations = [...this.allConversations, _conversation];
      this.predictionAsked = true;
    }
  }

  loadTDataPage() {
    this.dataSource.getPagedData(this.dataSource.data);
  }

  private prepareChartData() {
    this.chartData.forEach(cd => {
      cd.timestamp = new Date(cd.perioD_START).getTime();
    });
    this.chartData.sort((a, b) => (a.timestamp > b.timestamp) ? 1 : -1);
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
      series.dataFields.valueY = this.processedMeasurable;
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
      series.dataFields.valueY = this.processedMeasurable;
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
      pieSeries.dataFields.value = this.processedMeasurable;
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
    series.dataFields.valueY = this.processedMeasurable;
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
      series.dataFields.valueY = this.processedMeasurable;
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
    if (this.chartData.length > 2 && this.chartData.length < this.maxPredictionSize) {
      this.askChatbot.emit('Ask Prediction')
      const _conversation: Conversation = { question: '', answer: '' };
      this.allConversations = [...this.allConversations, _conversation];
      this.predictionAsked = true;
    }
  }

  public answerPrompt(prompt: string) {
    this.promptAnswered.emit(prompt);
  }

  private scrollToBottom() {
    try {
      this.scrollContainer.nativeElement.scroll({
        top: this.scrollContainer.nativeElement.scrollHeight,
        behavior: 'smooth'
      });
    } catch (err) { }
  }

  private getPrediction() {
    this.loadingPrediction = true;
    let predictionData: PredictionData[] = [];

    const measurable = this.processedMeasurable;

    // Get prediction based on chart data
    if (this.chartData.length > 0) {
      // Populate chart data for model (Sarima) fitting
      this.chartData.forEach(r => {
        let row: PredictionData = <PredictionData>{};
        row.sales = r[measurable];
        row.date = r.perioD_START;
        predictionData.push(row);
      });
      // Append response prediction to current data
      this.subscriptions.push(
        this.cognitiveService.predict(predictionData).subscribe((res: PredictionData[]) => {
          res.forEach(pd => {
            let row: ChartData = <ChartData>{};
            row[measurable] = pd.sales;
            row.perioD_START = pd.date;
            this.chartData.push(row);
          });
          this.prepareChartData();
          // Append data to the active chart type
          if (this.XYChart.data.length > 0) this.XYChart.data = this.chartData;
          else if (this.amPieChart.data.length > 0) this.amPieChart.data = this.chartData;
          else if (this.amRadarChart.data.length > 0) this.amRadarChart.data = this.chartData;
          this.askChatbot.emit("Prediction Success");
        },
          (error) => {
            this._snackBar.open("Something went wrong with the prediction", "okay", { duration: 3000 });
          }, () => this.loadingPrediction = false));
    }
    // Get prediction based on table data
    else if (this.tableData.length > 0) {
      // Populate table data for model (Sarima) fitting
      this.tableData.forEach(r => {
        let row: PredictionData = <PredictionData>{};
        row.sales = r[measurable];
        row.date = r.perioD_START;
        predictionData.push(row);
      });
      // Prediction response to be saved on to DB displayTable for serverside pagination and sorting
      let newTableData: DisplayTable[] = [];
      this.subscriptions.push(
        this.cognitiveService.predict(predictionData).subscribe((res: PredictionData[]) => {
          res.forEach(pd => {
            let row: DisplayTable = <DisplayTable>{};
            row.uuid = this.tableData[0].uuid;
            row[measurable] = pd.sales;
            row.perioD_START = pd.date;
            row.brand = "N/A";
            row.producT_NAME = "N/A";
            row.categorY_NAME = "N/A";
            newTableData.push(row);
          });
          this.tableData.push(...newTableData);
          // Save prediction response to DB displayTable
          this.subscriptions.push(
            this.apiService.saveDisplayTableData(newTableData).subscribe((res) => {
              // Then reload table
              this.dataSource.loadData(this.tableData, this.measurable);
              this.askChatbot.emit("Prediction Success");
            },
              (error) => {
                this._snackBar.open("Something went wrong with the backend! Please refresh and try again.", "okay", { duration: 3000 });
              }));

        }, (error) => {
          this._snackBar.open("Something went wrong with the prediction", "okay", { duration: 3000 });
        }, () => this.loadingPrediction = false));
    }
  }

  public deleteTable() {
    this.showTable = false;
    this.dataSource.disconnect();
    this.tableSubscriptions.forEach(sub => sub.unsubscribe());
    this.tableData = [];
    this.generatedQueryText = '';
    this.dataSource = new VisualizationDataSource(this.apiService, this.uuid);
  }

  public deleteChart() {
    this.chartData = [];
    this.showChart = false;
    this.generatedQueryText = '';
  }

  ngOnDestroy() {
    // Clear table on user exit/page refresh
    this.apiService.deleteTable(this.uuid).subscribe();
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.tableSubscriptions.forEach(sub => sub.unsubscribe());
    this.zone.runOutsideAngular(() => {
      if (this.XYChart) {
        this.XYChart.dispose();
      }
      if (this.amPieChart)
        this.amPieChart.dispose();
    });
  }
}
