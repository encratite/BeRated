module BeRated {
	export interface IPlayerEncounterStats {
		opponentId: number;
		opponentName: string;
		kills: number;
		deaths: number;
		winPercentage: number;
	}
} 