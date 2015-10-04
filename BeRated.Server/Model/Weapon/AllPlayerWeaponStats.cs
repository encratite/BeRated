using System.Collections.Generic;

namespace BeRated.Model.Weapon
{
	public class AllPlayerWeaponStats : BasePlayerStats
	{
		public List<TimeSpanPlayerWeaponStats> Weapons { get; set; }
	}
}
