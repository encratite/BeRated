using System.Collections.Generic;

namespace BeRated.Common
{
	public class PlayerInformation
	{
		public readonly PlayerIdentity Identity;
		public List<PlayerKill> Kills = new List<PlayerKill>();

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
