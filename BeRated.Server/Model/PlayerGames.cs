using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerGames : PlayerInfo
	{
		public List<PlayerGame> Games { get; private set; }

        public PlayerGames(string name, string steamId, List<PlayerGame> games)
            : base(name, steamId)
        {
            Games = games;
        }
	}
}
