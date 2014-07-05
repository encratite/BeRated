module BeRated {
	export interface IAllPlayerStats {
		id: number;
		name: string;
		kills: number;
		deaths: number;
		// null if deaths == 0
		killDeathRatio: number;
	}
} 