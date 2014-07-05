/// <reference path="IPlayerWeaponStats.ts"/>
/// <reference path="IPlayerEncounterStats.ts"/>

module BeRated {
	export interface IPlayerStats {
		id: number;
		name: string;
		weapons: Array<IPlayerWeaponStats>;
		encounters: Array<IPlayerEncounterStats>;
	}
} 