using System;

namespace BeRated.Cache
{
	enum Team
	{
		Terrorist,
		CounterTerrorist,
	}

	static class TeamConverter
	{
		public static Team FromString(string team)
		{
			if (team == "TERRORIST")
				return Team.Terrorist;
			else if (team == "CT")
				return Team.CounterTerrorist;
			else
				throw new ArgumentException("Invalid team string");
		}
	}
}
