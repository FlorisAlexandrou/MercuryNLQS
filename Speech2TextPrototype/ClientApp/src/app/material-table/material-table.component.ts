import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ApiService } from '../api.service';
import { MaterialTableDataSource } from './material-table-datasource';
import { tap, merge } from 'rxjs/operators';

@Component({
  selector: 'app-material-table',
  templateUrl: './material-table.component.html',
  styleUrls: ['./material-table.component.css']
})
export class MaterialTableComponent implements AfterViewInit, OnInit {
  @ViewChild(MatPaginator, {static: false}) paginator: MatPaginator;
  @ViewChild('sortData', {static: false}) sort: MatSort;
  dataSource: MaterialTableDataSource;

  /** Columns displayed in the table. Columns IDs can be added, removed, or reordered. */
  displayedColumns = ['brand', 'categoryName', 'periodStart', 'sales'];

  constructor(private api: ApiService) { }
  ngOnInit() {
    this.dataSource = new MaterialTableDataSource(this.api);

    this.dataSource.loadData();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    this.paginator.page
      .pipe(
        tap(() => this.loadTDataPage())
      )
      .subscribe();

    this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

    this.sort.sortChange
      .pipe(
        tap(() => this.dataSource.getSortedData(this.dataSource.data))
      )
      .subscribe();
  }

  loadTDataPage() {
    this.dataSource.getPagedData(this.dataSource.data);
  }
}
