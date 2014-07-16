module BeRated {
	export class DataTableRow {
		record: any;
		row: HTMLTableRowElement;

		constructor(record: any, row: HTMLTableRowElement) {
			this.record = record;
			this.row = row;
		}
	}
} 