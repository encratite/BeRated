/// <reference path="IPlayerWeaponStats.ts"/>
/// <reference path="IPlayerEncounterStats.ts"/>

module BeRated {
	export interface IPlayerStats {
		weapons: Array<IPlayerWeaponStats>;
		encounters: Array<IPlayerEncounterStats>;
	}
} 