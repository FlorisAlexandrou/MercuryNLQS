import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Observable, BehaviorSubject } from 'rxjs';
import { DisplayTable } from '../models/displayTable.model';
import { DataSource, CollectionViewer } from "@angular/cdk/collections";

/**
 * Data source for the MaterialTable view. This class should
 * encapsulate all logic for fetching and manipulating the displayed data
 * (including sorting, pagination, and filtering).
 */
export class MaterialTableDataSource extends DataSource<DisplayTable> {
    paginator: MatPaginator;
    sort: MatSort;
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private tdataSubject = new BehaviorSubject<DisplayTable[]>([]);
    public loading$ = this.loadingSubject.asObservable();
    public dataLength: number;
    public data: DisplayTable[] = [];

    constructor() {
        super();
    }


    loadData(tableData: DisplayTable[]) {
        this.loadingSubject.next(true);
        this.data = tableData;
        this.dataLength = tableData.length;
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

    /**
     * Paginate the data (client-side). If you're using server-side pagination,
     * this would be replaced by requesting the appropriate data from the server.
     */
    public getPagedData(data: DisplayTable[]) {
        const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
        var pagedData = data.splice(startIndex, this.paginator.pageSize);
        this.tdataSubject.next(pagedData);
    }

    /**
     * Sort the data (client-side). If you're using server-side sorting,
     * this would be replaced by requesting the appropriate data from the server.
     */
    public getSortedData(data: DisplayTable[]) {
        if (!this.sort.active || this.sort.direction === '') {
            return data;
        }

        this.tdataSubject.next(data.sort((a, b) => {
            const isAsc = this.sort.direction === 'asc';
            switch (this.sort.active) {
                case 'brand': return compare(a.brand, b.brand, isAsc);
                case 'categoryName': return compare(a.categorY_NAME, b.categorY_NAME, isAsc);
                case 'periodStart': return compare(a.perioD_START, b.perioD_START, isAsc);
                case 'sales': return compare(a.m_SALES_VALUE, b.m_SALES_VALUE, isAsc);
                default: return 0;
            }
        }));
    }
}

/** Simple sort comparator for example ID/Name columns (for client-side sorting). */
function compare(a, b, isAsc) {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}
