using System.Collections.Generic;

namespace BeRated.Model
{
	public class Matches : PlayerInfo
	{
		public List<PlayerGame> Games { get; private set; }

        public Matches(string name, string steamId, List<PlayerGame> games)
            : base(name, steamId)
        {
            Games = games;
        }
	}
}
