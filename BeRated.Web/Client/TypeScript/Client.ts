/// <reference path="Configuration.ts"/>
/// <reference path="Route.ts"/>
/// <reference path="RpcClient.ts"/>
/// <reference path="DataTable.ts"/>
/// <reference path="IAllPlayerStats.ts"/>
/// <reference path="IPlayerStats.ts"/>

module BeRated {
	var client: Client = null;

	export class Client {
		private rpcClient: RpcClient;
		private routes: Array<Route>;

		private windowHasBeenLoaded: boolean = false;

		constructor() {
			if (client != null) {
				throw new Error('Client has already been instantiated');
			}
			client = this;
			this.initialiseRpcClient();
			this.setRoutes();
			window.onload = this.onLoad.bind(this);
		}

		private setRoutes() {
			this.routes = [
				new Route(/^\/$/, this.routeIndex.bind(this)),
				new Route(/^\/Player\/(\d+)$/, this.routePlayer.bind(this))
			];
		}

		private onLoad(event: any) {
			this.windowHasBeenLoaded = true;
			this.routeRequest();
		}

		private onConnect() {
			this.routeRequest();
		}

		private initialiseRpcClient() {
			var pattern = /^\w+:\/\/([^\/:]+)/;
			var match = pattern.exec(window.location.href);
			if (match == null)
				throw new Error('Unable to find host in location');
			var host = match[1];
			var port = Configuration.webSocketPort;
			var url = 'ws://' + host + ':' + port + '/';
			this.rpcClient = new RpcClient(url, this.onConnect.bind(this));
		}

		private routeRequest() {
			if (!this.windowHasBeenLoaded || !this.rpcClient.isConnected())
				return;
			var pattern = /^\w+:\/\/.+?(\/.*)/;
			var path = '';
			var match = pattern.exec(window.location.href);
			if (match != null)
				path = match[1];
			for (var i = 0; i < this.routes.length; i++) {
				var route = this.routes[i];
				var requestHasBeenRouted = route.match(path);
				if (requestHasBeenRouted)
					return;
			}
			throw new Error('Invalid path');
		}

		private routeIndex() {
			this.rpcClient.call('getAllPlayerStats', [], this.onGetAllPlayerStats.bind(this));
		}

		private routePlayer(playerIdString: string) {
			var playerId = parseInt(playerIdString);
			this.rpcClient.call('getPlayerStats', [playerId], this.onGetAllPlayerStats.bind(this));
		}

		private onGetAllPlayerStats(allPlayerStats: Array<IAllPlayerStats>) {
			var columns: Array<DataTableColumn> = [
				new DataTableColumn((record: IAllPlayerStats) => record.name, 'Name', true),
				new DataTableColumn((record: IAllPlayerStats) => record.kills, 'Kills'),
				new DataTableColumn((record: IAllPlayerStats) => record.deaths, 'Deaths'),
				new DataTableColumn((record: IAllPlayerStats) => record.killDeathRatio, 'Kill/death ratio')
			];
			var dataTable = new DataTable(allPlayerStats, columns);
			document.body.appendChild(dataTable.table);
		}

		private onGetPlayerStats(playerStats: IPlayerStats) {
			console.log(playerStats);
		}
	}
}