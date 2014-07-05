module BeRated {
	export class DataTableColumn {
		selector: (any) => any;
		description: string;
		defaultSort: boolean = false;

		constructor(selector: (any) => any, description: string, defaultSort?: boolean) {
			this.selector = selector;
			this.description = description;
			if (defaultSort !== undefined)
				this.defaultSort = defaultSort;
		}
	}
} 