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
				new Route(new RegExp('^\\/' + Configuration.playerRoute + '\\/(\\d+)$'), this.routePlayer.bind(this))
			];
		}

		private onLoad(event: any) {
			this.windowHasBeenLoaded = true;
			this.routeRequest();
		}

		private onConnect() {
			this.routeRequest();
		}

		private onDisconnect() {
			document.body.innerHTML = '';
			var error = document.createElement('p');
			error.className = 'error';
			error.textContent = 'Failed to connect to Web Socket server.';
			document.body.appendChild(error);
		}

		private initialiseRpcClient() {
			var pattern = /^\w+:\/\/([^\/:]+)/;
			var match = pattern.exec(window.location.href);
			if (match == null)
				throw new Error('Unable to find host in location');
			var host = match[1];
			var port = Configuration.webSocketPort;
			var url = 'ws://' + host + ':' + port + '/';
			this.rpcClient = new RpcClient(url, this.onConnect.bind(this), this.onDisconnect.bind(this));
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

		private setTitle(title: string) {
			document.title = Configuration.titlePrefix + title;
		}

		private routeIndex() {
			this.setTitle('Index');
			this.rpcClient.call('getAllPlayerStats', [], this.onGetAllPlayerStats.bind(this));
		}

		private routePlayer(playerIdString: string) {
			var playerId = parseInt(playerIdString);
			this.rpcClient.call('getPlayerStats', [playerId], this.onGetPlayerStats.bind(this));
		}

		private onGetAllPlayerStats(allPlayerStats: Array<IAllPlayerStats>) {
			var columns: Array<DataTableColumn> = [
				new DataTableColumn('Name', (record: IAllPlayerStats) => record.name, this.renderPlayerStatsName.bind(this), true),
				new DataTableColumn('Kills', (record: IAllPlayerStats) => record.kills),
				new DataTableColumn('Deaths', (record: IAllPlayerStats) => record.deaths),
				new DataTableColumn('Kill/death ratio', (record: IAllPlayerStats) => record.killDeathRatio),
				new DataTableColumn('Games', (record: IAllPlayerStats) => record.gamesPlayed),
				new DataTableColumn('Rounds', (record: IAllPlayerStats) => record.roundsPlayed),
				new DataTableColumn('Rounds (CT)', (record: IAllPlayerStats) => record.roundsPlayedCounterTerrorist),
				new DataTableColumn('Rounds (T)', (record: IAllPlayerStats) => record.roundsPlayedTerrorist),
				new DataTableColumn('Games won', (record: IAllPlayerStats) => record.gameWinPercentage, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won', (record: IAllPlayerStats) => record.winPercentage, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won (T)', (record: IAllPlayerStats) => record.winPercentageTerrorist, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won (CT)', (record: IAllPlayerStats) => record.winPercentageCounterTerrorist, this.renderPercentage.bind(this))
			];
			var header = document.createElement('h1');
			header.className = 'header';
			header.textContent = 'CS:GO Statistics';
			var dataTable = new DataTable(allPlayerStats, columns);
			dataTable.table.classList.add('indexTable');
			document.body.appendChild(header);
			document.body.appendChild(dataTable.table);
		}

		private onGetPlayerStats(playerStats: IPlayerStats) {
			this.setTitle(playerStats.name);
			var weaponColumns: Array<DataTableColumn> = [
				new DataTableColumn('Weapon', (record: IPlayerWeaponStats) => record.weapon),
				new DataTableColumn('Kills', (record: IPlayerWeaponStats) => record.kills, null, true, SortMode.Descending),
				new DataTableColumn('Headshot kills', (record: IPlayerWeaponStats) => record.headshots),
				new DataTableColumn('Headshot kill percentage', (record: IPlayerWeaponStats) => record.headshotPercentage, this.renderPercentage.bind(this))
			];
			var weaponTable = new DataTable(playerStats.weapons, weaponColumns);
			var encounterColumns: Array<DataTableColumn> = [
				new DataTableColumn('Opponent', (record: IPlayerEncounterStats) => record.opponentName, this.renderEncounterStatsName.bind(this), true),
				new DataTableColumn('Encounters', (record: IPlayerEncounterStats) => record.encounters),
				new DataTableColumn('Kills', (record: IPlayerEncounterStats) => record.kills),
				new DataTableColumn('Deaths', (record: IPlayerEncounterStats) => record.deaths),
				new DataTableColumn('Win percentage', (record: IPlayerEncounterStats) => record.winPercentage, this.renderPercentage.bind(this))
			];
			var encounterTable = new DataTable(playerStats.encounters, encounterColumns);
			var purchasesColumns: Array<DataTableColumn> = [
				new DataTableColumn('Item', (record: IPlayerPurchases) => record.item),
				new DataTableColumn('Purchases', (record: IPlayerPurchases) => record.timesPurchased),
				new DataTableColumn('Purchases/round', (record: IPlayerPurchases) => record.purchasesPerRound, null, true, SortMode.Descending),
				new DataTableColumn('Kills/purchase', (record: IPlayerPurchases) => record.killsPerPurchase, null, false, SortMode.Descending)
			];
			var purchasesTable = new DataTable(playerStats.purchases, purchasesColumns);
			var addTable = (table: HTMLTableElement) => {
				table.classList.add('individualPlayerStatsTable');
				document.body.appendChild(table);
			};
			var paragraph = document.createElement('p');
			paragraph.className = 'returnLink';
			var link = document.createElement('a');
			link.href = '/';
			link.textContent = 'Return to index';
			paragraph.appendChild(link);
			var header = document.createElement('h1');
			header.className = 'header';
			header.textContent = playerStats.name;
			document.body.appendChild(paragraph);
			document.body.appendChild(header);
			addTable(weaponTable.table);
			addTable(encounterTable.table);
			addTable(purchasesTable.table);
		}

		private renderPlayer(name: string, id: number): Node {
			var node = document.createElement('a');
			node.href = '/' + Configuration.playerRoute + '/' + id;
			node.textContent = name;
			return node;
		}

		private renderPlayerStatsName(name: string, record: IAllPlayerStats): Node {
			var node = this.renderPlayer(record.name, record.id);
			return node;
		}

		private renderEncounterStatsName(name: string, record: IPlayerEncounterStats): Node {
			var node = this.renderPlayer(record.opponentName, record.opponentId);
			return node;
		}

		private renderPercentage(percentage: number): Node {
			var text = percentage + '%';
			var node = document.createTextNode(text);
			return node;
		}
	}
}