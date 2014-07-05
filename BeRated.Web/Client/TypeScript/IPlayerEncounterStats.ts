module BeRated {
	export interface IPlayerEncounterStats {
		opponentId: number;
		opponentName: string;
		encounters: number;
		kills: number;
		deaths: number;
		winPercentage: number;
	}
} 