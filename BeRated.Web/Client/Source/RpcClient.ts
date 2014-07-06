/// <reference path="CallMessage.ts"/>
/// <reference path="IResultMessage.ts"/>

module BeRated {
	export class RpcClient {
		private url: string;
		private webSocket: WebSocket;
		private onConnect: () => void;
		private onDisconnect: () => void;
		private callbacks: { [id: number]: Function } = {};
		private id: number = 0;
		private automaticReconnect: boolean = false;

		constructor(url: string, onConnect: () => void, onDisconnect?: () => void) {
			this.url = url;
			this.onConnect = onConnect;
			this.onDisconnect = onDisconnect;
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

		isConnected(): boolean {
			var isConnected = this.webSocket.readyState == 1;
			return isConnected;
		}

		private connect() {
			var socket = new WebSocket(this.url);
			this.webSocket = socket;
			socket.onopen = this.onOpen.bind(this);
			socket.onmessage = this.onMessage.bind(this);
			socket.onclose = this.onClose.bind(this);
			socket.onerror = this.onError.bind(this);
		}

		private onOpen(event: Event) {
			this.onConnect();
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

		private onClose(event: CloseEvent) {
			if (this.automaticReconnect) {
				console.error('Disconnected from server, attempting to reconnect');
				window.setTimeout(this.connect.bind(this), 5000);
			}
			else
				console.error('Disconnected from server');
			if(this.onDisconnect != null)
				this.onDisconnect();
		}

		private onError(event: ErrorEvent) {
			console.error(event.message);
		}
	}
}