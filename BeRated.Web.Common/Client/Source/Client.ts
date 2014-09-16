/// <reference path="Configuration.ts"/>
/// <reference path="Route.ts"/>
/// <reference path="RpcClient.ts"/>
/// <reference path="DataTable.ts"/>

/// <reference path="IAllPlayerStats.ts"/>
/// <reference path="IPlayerStats.ts"/>
/// <reference path="IPlayerWeaponStats.ts"/>
/// <reference path="IPlayerEncounterStats.ts"/>
/// <reference path="IPlayerPurchaseStats.ts"/>
/// <reference path="IKillDeathRatioHistory.ts"/>

declare var Dygraph: any;

module BeRated {
	var client: Client = null;

	export class Client {
		private rpcClient: RpcClient;
		private routes: Array<Route>;

		private windowHasBeenLoaded: boolean = false;
		private loadingContent: boolean = true;

		constructor() {
			if (client != null) {
				throw new Error('Client has already been instantiated');
			}
			client = this;
			this.initialiseRpcClient();
			this.setRoutes();
			window.onload = this.onLoad.bind(this);
			window.onhashchange = this.onHashChange.bind(this);
		}

		private setRoutes() {
			this.routes = [
				new Route(/^#?$/, this.routeIndex.bind(this)),
				new Route(new RegExp('^#?' + Configuration.playerRoute + '\\/(\\d+)$'), this.routePlayer.bind(this))
			];
		}

		private onLoad(event: any) {
			this.windowHasBeenLoaded = true;
			this.routeRequest();
		}

		private onConnect() {
			this.routeRequest();
		}

		private onDisconnect(wasConnected: boolean) {
			this.clearBody();
			var message;
			if (wasConnected)
				message = 'Disconnected from server';
			else
				message = 'Unable to connect';
			var error = this.createHeader(Configuration.errorIcon, message);
			error.classList.add('error');
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

		private onHashChange(event: Event) {
			if(!this.loadingContent)
				this.routeRequest();
		}

		private routeRequest() {
			if (!this.windowHasBeenLoaded || !this.rpcClient.isConnected())
				return;
			this.startLoadingContent();
			var pattern = /^#?(.*)/;
			var match = pattern.exec(window.location.hash);
			var path = match[1];
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
			this.clearBody();
			this.addAllPlayerStatsTable(allPlayerStats);
			this.doneLoadingContent();
		}

		private addAllPlayerStatsTable(allPlayerStats: Array<IAllPlayerStats>) {
			var columns: Array<DataTableColumn> = [
				new DataTableColumn('Name', (record: IAllPlayerStats) => record.name, this.renderPlayerStatsName.bind(this), true),
				new DataTableColumn('Kills', (record: IAllPlayerStats) => record.kills),
				new DataTableColumn('Deaths', (record: IAllPlayerStats) => record.deaths),
				new DataTableColumn('Kill/death ratio', (record: IAllPlayerStats) => record.killDeathRatio),
				new DataTableColumn('Team kills', (record: IAllPlayerStats) => record.teamKills),
				new DataTableColumn('Games', (record: IAllPlayerStats) => record.gamesPlayed),
				new DataTableColumn('Rounds', (record: IAllPlayerStats) => record.roundsPlayed),
				new DataTableColumn('Rounds (CT)', (record: IAllPlayerStats) => record.roundsPlayedCounterTerrorist),
				new DataTableColumn('Rounds (T)', (record: IAllPlayerStats) => record.roundsPlayedTerrorist),
				new DataTableColumn('Games won', (record: IAllPlayerStats) => record.gameWinPercentage, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won', (record: IAllPlayerStats) => record.winPercentage, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won (T)', (record: IAllPlayerStats) => record.winPercentageTerrorist, this.renderPercentage.bind(this)),
				new DataTableColumn('Rounds won (CT)', (record: IAllPlayerStats) => record.winPercentageCounterTerrorist, this.renderPercentage.bind(this))
			];
			var header = this.createHeader(Configuration.indexIcon, Configuration.title);
			var dataTable = new DataTable(allPlayerStats, columns);
			dataTable.table.classList.add('indexTable');
			document.body.appendChild(header);
			document.body.appendChild(dataTable.table);
		}

		private onGetPlayerStats(playerStats: IPlayerStats) {
			this.clearBody();
			this.setTitle(playerStats.name);
            this.addHeader(playerStats);
            var leftContainer = document.createElement('div');
            leftContainer.className = 'leftSide';
            var rightContainer = document.createElement('div')
            rightContainer.className = 'rightSide';
            document.body.appendChild(leftContainer);
            document.body.appendChild(rightContainer);
			this.addWeaponTable(playerStats, leftContainer);
            this.addEncounterTable(playerStats, leftContainer);
            this.addPurchases(playerStats, leftContainer);
            this.addKillDeathRatioHistory(playerStats, leftContainer);
            this.addPlayerGames(playerStats, rightContainer);
			this.doneLoadingContent();
		}

		private addHeader(playerStats: IPlayerStats) {
			var header = this.createHeader(Configuration.playerIcon, playerStats.name);
			header.classList.add('playerHeader');
			header.onclick = () => window.location.hash = '';
			document.body.appendChild(header);
		}

		private addTable(data: Array<any>, columns: Array<DataTableColumn>, container: Node = document.body) {
			var dataTable = new DataTable(data, columns);
			var table = dataTable.table;
			table.classList.add('individualPlayerStatsTable');
			container.appendChild(table);
		}

		private addWeaponTable(playerStats: IPlayerStats, container: Node) {
			var weaponColumns: Array<DataTableColumn> = [
				new DataTableColumn('Weapon', (record: IPlayerWeaponStats) => record.weapon),
				new DataTableColumn('Kills', (record: IPlayerWeaponStats) => record.kills, null, true, SortMode.Descending),
				new DataTableColumn('Headshot kills', (record: IPlayerWeaponStats) => record.headshots),
				new DataTableColumn('Headshot kill percentage', (record: IPlayerWeaponStats) => record.headshotPercentage, this.renderPercentage.bind(this))
			];
			this.addTable(playerStats.weapons, weaponColumns, container);
		}

        private addEncounterTable(playerStats: IPlayerStats, container: Node) {
			var encounterColumns: Array<DataTableColumn> = [
				new DataTableColumn('Opponent', (record: IPlayerEncounterStats) => record.opponentName, this.renderEncounterStatsName.bind(this), true),
				new DataTableColumn('Encounters', (record: IPlayerEncounterStats) => record.encounters),
				new DataTableColumn('Kills', (record: IPlayerEncounterStats) => record.kills),
				new DataTableColumn('Deaths', (record: IPlayerEncounterStats) => record.deaths),
				new DataTableColumn('Win percentage', (record: IPlayerEncounterStats) => record.winPercentage, this.renderPercentage.bind(this))
			];
			this.addTable(playerStats.encounters, encounterColumns, container);
		}

        private addPurchases(playerStats: IPlayerStats, container: Node) {
			var purchasesColumns: Array<DataTableColumn> = [
				new DataTableColumn('Item', (record: IPlayerPurchaseStats) => record.item),
				new DataTableColumn('Purchases', (record: IPlayerPurchaseStats) => record.timesPurchased),
				new DataTableColumn('Purchases/round', (record: IPlayerPurchaseStats) => record.purchasesPerRound, null, true, SortMode.Descending),
				new DataTableColumn('Kills/purchase', (record: IPlayerPurchaseStats) => record.killsPerPurchase, null, false, SortMode.Descending)
			];
			this.addTable(playerStats.purchases, purchasesColumns, container);
		}

        private addKillDeathRatioHistory(playerStats: IPlayerStats, container: Node) {
			var header = document.createElement('h1');
			header.className = 'dataHeader';
			header.textContent = 'Kill/death ratio';
			var graphContainer = document.createElement('div');
			graphContainer.className = 'killDeathRatioGraph';
            container.appendChild(header);
            container.appendChild(graphContainer);
			var data = [];
			var lastDay: Date = null;
			playerStats.killDeathRatioHistory.forEach((x) => {
				var date = new Date(x.day);
				if (lastDay == null || !this.datesAreEqual(date, lastDay)) {
					var sample = [date, x.killDeathRatio];
					data.push(sample);
					lastDay = date;
				}
			});
			var options = {
				labels: [
					'Date',
					'KDR'
				],
				colors: [
					'black'
				],
				includeZero: true,
				valueRange: [0, null],
				axes: {
					x: {
						axisLabelFormatter: (date) => this.getDateString(date),
						valueFormatter: (ms) => {
							var date = new Date(ms);
							return this.getDateString(date);
						}
					}
				},
				xAxisLabelWidth: 80
			};
			var graph = new Dygraph(graphContainer, data, options);
		}

        private addPlayerGames(playerStats: IPlayerStats, container: Node) {
			var columns: Array<DataTableColumn> = [
				new DataTableColumn('Time', (record: IPlayerGame) => new Date(record.gameTime + 'Z'), this.renderGameTime.bind(this), true, SortMode.Descending),
				new DataTableColumn('Outcome', (record: IPlayerGame) => record.outcome, this.renderOutcome.bind(this)),
				new DataTableColumn('Score', (record: IPlayerGame) => record.playerScore * 100 + record.enemyScore, this.renderScore.bind(this)),
				new DataTableColumn('Team', (record: IPlayerGame) => this.getTeamValue(record.playerTeam), (value: string, record: IPlayerGame) => this.renderTeam(record.playerTeam)),
				new DataTableColumn('Enemy team', (record: IPlayerGame) => this.getTeamValue(record.enemyTeam), (value: string, record: IPlayerGame) => this.renderTeam(record.enemyTeam))
			];
            var dataTable = new DataTable(playerStats.games, columns);
            dataTable.table.classList.add('gamesTable');
            container.appendChild(dataTable.table);
		}

		private getTeamValue(players: Array<IGamePlayer>) {
			var output = '';
			players.forEach((player) => output += player.name + ', ');
			return output;
		}

		private getDateString(date: Date): string {
			var output: string = date.getUTCFullYear().toString();
			output += '-' + this.addZero(date.getUTCMonth() + 1);
			output += '-' + this.addZero(date.getUTCDate());
			return output;
		}

		private getTimeString(date: Date): string {
			var output = this.getDateString(date);
			output += ' ' + this.addZero(date.getUTCHours());
			output += ':' + this.addZero(date.getUTCMinutes());
			// output += ':' + this.addZero(date.getUTCSeconds());
			return output;
		}

		private addZero(input: number): string {
			if (input < 10)
				return '0' + input;
			else
				return '' + input;
		}

		private datesAreEqual(date1: Date, date2: Date): boolean {
			return date1.getFullYear() == date2.getFullYear() && date1.getMonth() == date2.getMonth() && date1.getDate() == date2.getDate();
		}

		private renderPlayer(name: string, id: number): Node {
			var node = document.createElement('a');
			node.href = '#' + Configuration.playerRoute + '/' + id;
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

		private renderGameTime(time: Date, record: IPlayerGame): Node {
			var text = this.getTimeString(time);
			var node = document.createTextNode(text);
			return node;
		}

		private renderScore(value: number, record: IPlayerGame): Node {
			var text = record.playerScore + ' - ' + record.enemyScore;
			var node = document.createTextNode(text);
			return node;
		}

		private renderOutcome(outcome: string, record: IPlayerGame): Node {
			var node = document.createElement('span');
			switch (outcome) {
				case 'loss':
					node.textContent = 'Loss';
					node.className = 'outcomeLoss';
					break;

				case 'win':
					node.textContent = 'Win';
					node.className = 'outcomeWin';
					break;

				case 'draw':
					node.textContent = 'Draw';
					node.className = 'outcomeDraw';
					break;
			}
			return node;
		}

		private renderTeam(players: Array<IGamePlayer>): Node {
			var container = document.createElement('div');
			var first = true;
			players.forEach((player) => {
				if (first) {
					first = false;
				}
				else {
					var separator = document.createTextNode(', ');
					container.appendChild(separator);
				}
				var node = this.renderPlayer(player.name, player.id);
				container.appendChild(node);
			});
			return container;
		}

		private createHeader(iconClass: string, title: string) {
			var header = document.createElement('h1');
			header.className = 'header';
			var icon = Utility.createIcon(iconClass);
			var titleNode = document.createTextNode(title);
			header.appendChild(icon);
			header.appendChild(titleNode);
			return header;
		}

		private clearBody() {
			while (document.body.firstChild)
				document.body.removeChild(document.body.firstChild);
		}

		private startLoadingContent() {
			this.loadingContent = true;
			document.onclick = this.blockClickEvent.bind(this);
		}

		private doneLoadingContent() {
			document.onclick = () => { };
			this.loadingContent = false;
		}

		private blockClickEvent(event: MouseEvent) {
			event.stopPropagation();
			event.preventDefault();
			event.stopImmediatePropagation();
			return false;
		}
	}
}