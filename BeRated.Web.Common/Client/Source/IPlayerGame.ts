module BeRated {
	export interface IPlayerGame {
		gameTime: string;
		playerScore: number;
		enemyScore: number;
		playerTeam: Array<IGamePlayer>;
		enemyTeam: Array<IGamePlayer>;
		outcome: string;
	}
} 