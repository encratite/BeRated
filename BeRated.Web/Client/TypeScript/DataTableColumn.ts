module BeRated {
	export class DataTableColumn {
		description: string;
		select: (any) => any;
		render: (any) => Node = null;
		defaultSort: boolean = false;

		constructor(description: string, select: (any) => any, render: (any) => Node = null, defaultSort?: boolean) {
			this.description = description;
			this.select = select;
			if (render !== undefined)
				this.render = render;
			if (defaultSort !== undefined)
				this.defaultSort = defaultSort;
		}
	}
} 