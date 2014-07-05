class CallMessage {
	id: number;
	method: string;
	arguments: Array<any>;

	constructor(id: number, method: string, arguments: Array<any>) {
		this.id = id;
		this.method = method;
		this.arguments = arguments;
	}
}