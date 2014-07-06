/// <reference path="IPlayerWeaponStats.ts"/>
/// <reference path="IPlayerEncounterStats.ts"/>
/// <reference path="IPlayerPurchases.ts"/>

module BeRated {
	export interface IPlayerStats {
		id: number;
		name: string;
		weapons: Array<IPlayerWeaponStats>;
		encounters: Array<IPlayerEncounterStats>;
		purchases: Array<IPlayerPurchases>;
	}
} 