class RPCClient {
	private url: string;
	private webSocket: WebSocket;
	private callbacks: { [id: number]: Function } = {};
	private id: number = 0;

	constructor(url: string) {
		this.url = url;
		this.connect();
	}

	call(method: string, arguments: Array<any>, callback: Function) {
		var id = this.id;
		this.id++;
		this.callbacks[id] = callback;
		var call = new CallMessage(id, method, arguments);
		var message = JSON.stringify(call);
		this.webSocket.send(message);
	}

	private connect() {
		var socket = new WebSocket(this.url);
		this.webSocket = socket;
		socket.onmessage = this.onMessage.bind(this);
		socket.onclose = this.onClose.bind(this);
	}

	private onMessage(event: any) {
		var message: IResultMessage = JSON.parse(event.data);
		if (message.error != null) {
			console.error('RPC error: ' + message.error);
			return;
		}
		var id = message.id;
		if (typeof this.callbacks[id] === 'undefined') {
			console.error('Unexpected RPC result ID: ' + id);
			return;
		}
		var callback = this.callbacks[id];
		delete this.callbacks[id];
		callback(message.result);
	}

	private onClose() {
		console.error('Disconnected from WebSocket server, attempting to reconnect');
		this.connect();
	}
}