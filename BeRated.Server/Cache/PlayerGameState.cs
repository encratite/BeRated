using Team = BeRated.Common.Team;

namespace BeRated.Cache
{
    class PlayerGameState
    {
        public Team Team { get; set; }

        public int? RoundPlayerLeft { get; set; }

		public int Kills { get; set; }
		public int Deaths { get; set; }

        public PlayerGameState(Player player, Team team)
        {
            Team = team;
            RoundPlayerLeft = null;
			Kills = 0;
			Deaths = 0;
        }
    }
}
