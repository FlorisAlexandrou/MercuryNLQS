import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Observable, BehaviorSubject } from 'rxjs';
import { DisplayTable } from '../models/displayTable.model';
import { DataSource, CollectionViewer } from "@angular/cdk/collections";
import { ApiService } from '../api.service';

/**
 * Data source for the MaterialTable view. This class should
 * encapsulate all logic for fetching and manipulating the displayed data
 * (including sorting, pagination, and filtering).
 */
export class VisualizationDataSource extends DataSource<DisplayTable> {
    paginator: MatPaginator;
    sort: MatSort;
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private tdataSubject = new BehaviorSubject<DisplayTable[]>([]);
    public loading$ = this.loadingSubject.asObservable();
    public dataLength: number;
    public data: DisplayTable[] = [];
    private measurable: string;

    constructor(private api : ApiService, private uuid: string) {
        super();
    }


    loadData(tableData: DisplayTable[], measurable: string) {
        this.loadingSubject.next(true);
        this.data = tableData;
        this.dataLength = tableData.length;
        this.measurable = measurable;
        this.getPagedData(this.data);
        this.loadingSubject.next(false);

    }

    /**
     * Connect this data source to the table. The table will only update when
     * the returned stream emits new items.
     * @returns A stream of the items to be rendered.
     */
    connect(CollectionViewer: CollectionViewer): Observable<DisplayTable[]> {
        // Combine everything that affects the rendered data into one update
        // stream for the data-table to consume.
        return this.tdataSubject.asObservable();
    }

    /**
     *  Called when the table is being destroyed. Use this function, to clean up
     * any open connections or free any held resources that were set up during connect.
     */
    disconnect(): void {
        this.tdataSubject.complete();
        this.loadingSubject.complete();
    }

    public getPagedData(data: DisplayTable[]) {
        this.loadingSubject.next(true);
        this.api.getPagedData(this.paginator.pageIndex, this.paginator.pageSize, this.uuid)
            .subscribe((res: any) => {
                this.tdataSubject.next(res);
                this.data = res;
                this.loadingSubject.next(false);
            });
    }

    public getSortedData(data: DisplayTable[]) {
        var column = this.sort.active;

        if (this.sort.active == 'sales') {
            column = this.measurable;
        }
        console.log(column);

        this.api.getSortedData(column, this.sort.direction, this.paginator.pageIndex, this.paginator.pageSize, this.uuid)
            .subscribe((res: any) => {
                console.log(res);
                this.tdataSubject.next(res)
                this.loadingSubject.next(false);
            });
    }
}
