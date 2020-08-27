import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { map, catchError, finalize } from 'rxjs/operators';
import { Observable, of as observableOf, merge, BehaviorSubject, of } from 'rxjs';
import { ApiService } from '../api.service';
import { TData } from '../AnswerInterface';
import { DataSource, CollectionViewer } from "@angular/cdk/collections";

/**
 * Data source for the MaterialTable view. This class should
 * encapsulate all logic for fetching and manipulating the displayed data
 * (including sorting, pagination, and filtering).
 */
export class MaterialTableDataSource extends DataSource<TData> {
  paginator: MatPaginator;
  sort: MatSort;
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private tdataSubject = new BehaviorSubject<TData[]>([]);
  public loading$ = this.loadingSubject.asObservable();
  public dataLength: number;
  public data: TData[] = [];

  constructor(private api: ApiService) {
    super();
  }


  loadData() {
    this.loadingSubject.next(true);
    console.log("calling api!");
    const getAnswerSub = this.api
      .getAnswer("Show sales of agros for january 2016", false)
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe((res: any) => {
        console.log(res);
        this.data = res.queryResult;
        this.dataLength = res.queryResult.length;
        this.getPagedData(res.queryResult);
        //this.tdataSubject.next(data);
      });

  }

  /**
   * Connect this data source to the table. The table will only update when
   * the returned stream emits new items.
   * @returns A stream of the items to be rendered.
   */
  connect(CollectionViewer: CollectionViewer): Observable<TData[]> {
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
  public getPagedData(data: TData[]) {
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    var pagedData = data.splice(startIndex, this.paginator.pageSize);
    this.tdataSubject.next(pagedData);
  }

  /**
   * Sort the data (client-side). If you're using server-side sorting,
   * this would be replaced by requesting the appropriate data from the server.
   */
    public getSortedData(data: TData[]) {
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
