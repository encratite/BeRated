/// <reference path="DataTableColumn.ts"/>
/// <reference path="DataTableRow.ts"/>

module BeRated {
	export enum SortMode {
		Ascending,
		Descending
	}

	export class DataTable {
		private rows: Array<DataTableRow> = [];
		private columns: Array<DataTableColumn>;
		private sortMode: SortMode = null;
		private sortColumn: DataTableColumn = null;

		table: HTMLTableElement = null;
		headerRow: HTMLTableRowElement = null;

		constructor(data: Array<any>, columns: Array<DataTableColumn>) {
			this.columns = columns;
			this.createTable(data);
		}

		private createTable(data: Array<any>) {
			var table = document.createElement('table');
			table.className = 'dataTable';
			this.table = table;
			this.headerRow = document.createElement('tr');
			this.columns.forEach(((column: DataTableColumn) => {
				if (column.defaultSort) {
					if (this.sortColumn != null)
						throw new Error('There cannot be more than one default sort column');
					this.sortColumn = column;
					this.sortMode = column.defaultSortMode;
				}
				var header = document.createElement('th');
				header.onclick = (event: any) => this.onSortClick(column);
				header.innerText = column.description;
				this.headerRow.appendChild(header);
			}).bind(this));
			if (this.sortColumn == null)
				throw new Error('No default sort column has been specified');
			data.forEach(((record: any) => {
				var row = document.createElement('tr');
				this.columns.forEach(((column: DataTableColumn) => {
					var cell = document.createElement('td');
					var render = column.render;
					if (render == null) {
						var value = column.select(record);
						cell.innerText = value;
					}
					else {
						var node = render(record);
						cell.appendChild(node);
					}
					row.appendChild(cell);
				}).bind(this));
				this.table.appendChild(row);
				var dataTableRow = new DataTableRow(record, row);
				this.rows.push(dataTableRow);
			}).bind(this));
			this.refresh();
		}

		private refresh() {
			this.sortRows();
			this.table.innerHTML = '';
			this.table.appendChild(this.headerRow);
			this.rows.forEach(((row: DataTableRow) => {
				this.table.appendChild(row.row);
			}).bind(this));
		}

		private sortRows() {
			this.rows.sort(((row1: DataTableRow, row2: DataTableRow) => {
				var select = this.sortColumn.select;
				var property1 = select(row1.record);
				var property2 = select(row2.record);
				var output = 0;
				if (property1 < property2)
					output = -1;
				else if (property1 > property2)
					output = 1;
				if (this.sortMode == SortMode.Descending)
					output = - output;
				return output;
			}).bind(this));
		}

		private onSortClick(column: DataTableColumn) {
			if (this.sortColumn == column) {
				if (this.sortMode === SortMode.Ascending)
					this.sortMode = SortMode.Descending;
				else
					this.sortMode = SortMode.Ascending;
			}
			else {
				this.sortColumn = column;
				this.sortMode = SortMode.Ascending;
				if (this.rows.length > 0) {
					var firstRecord = this.rows[0].record;
					var firstValue = column.select(firstRecord);
					if (typeof firstValue === 'number')
						this.sortMode = SortMode.Descending;
				}
			}
			this.refresh();
		}
	}
} 