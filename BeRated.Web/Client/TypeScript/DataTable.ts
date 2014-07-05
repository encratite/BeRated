/// <reference path="DataTableColumn.ts"/>

module BeRated {
	enum SortMode {
		Ascending,
		Descending
	}

	export class DataTable {
		private data: Array<any> = null;
		private columns: Array<DataTableColumn>;
		private sortMode: SortMode = null;
		private defaultSortColumn: DataTableColumn = null;
		private sortColumn: DataTableColumn = null;

		table: HTMLTableElement = null;
		headerRow: HTMLTableRowElement = null;

		constructor(data: Array<any>, columns: Array<DataTableColumn>) {
			this.data = data;
			this.columns = columns;
			this.createTable();
		}

		private createTable() {
			var table = document.createElement('table');
			table.className = 'dataTable';
			this.table = table;
			this.headerRow = document.createElement('tr');
			this.columns.forEach(((column: DataTableColumn) => {
				if (column.defaultSort) {
					if (this.defaultSortColumn != null)
						throw new Error('There cannot be more than one default sort column');
					this.defaultSortColumn = column;
					this.sortMode = SortMode.Ascending;
				}
				if (this.defaultSortColumn == null)
					throw new Error('No default sort column has been specified');
				var header = document.createElement('th');
				header.innerText = column.description;
				this.headerRow.appendChild(header);
			}).bind(this));
			this.refresh();
		}

		private refresh() {
			this.sortData();
			this.table.innerHTML = '';
			this.table.appendChild(this.headerRow);
			this.data.forEach(((record: any) => {
				var row = document.createElement('tr');
				this.columns.forEach(((column: DataTableColumn) => {
					var cell = document.createElement('td');
					var value = column.selector(record);
					cell.innerText = value;
					row.appendChild(cell);
				}).bind(this));
				this.table.appendChild(row);
			}).bind(this));
		}

		private sortData() {
			var sortColumn = this.sortColumn;
			if (sortColumn == null)
				sortColumn = this.defaultSortColumn;
			this.data.sort(((record1: any, record2: any) => {
				var selector = sortColumn.selector;
				var property1 = selector(record1);
				var property2 = selector(record2);
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
	}
} 