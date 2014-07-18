module BeRated {
	export interface IPlayerPurchaseStats {
		item: string;
		timesPurchased: number;
		purchasesPerRound: number;
		killsPerPurchase: number;
	}
} 