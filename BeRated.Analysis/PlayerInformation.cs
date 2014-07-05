using System.Collections.Generic;

namespace BeRated
{
	public class PlayerInformation
	{
		public readonly PlayerIdentity Identity;
		public readonly List<PlayerKill> Kills = new List<PlayerKill>();
		public int KillCount = 0;
		public int DeathCount = 0;

		public PlayerInformation(PlayerIdentity identity)
		{
			Identity = identity;
		}

		public override string ToString()
		{
			return Identity.ToString();
		}
	}
}
