module BeRated {
	export class DataTableColumn {
		description: string;
		select: (record: any) => any;
		render: (value: any, record: any) => Node = null;
		defaultSort: boolean = false;
		defaultSortMode: SortMode = SortMode.Ascending;

		constructor(description: string, select: (record: any) => any, render: (value: any, record: any) => Node = null, defaultSort?: boolean, defaultSortMode?: SortMode) {
			this.description = description;
			this.select = select;
			if (render !== undefined)
				this.render = render;
			if (defaultSort !== undefined)
				this.defaultSort = defaultSort;
			if (defaultSortMode !== undefined)
				this.defaultSortMode = defaultSortMode;
		}
	}
} 