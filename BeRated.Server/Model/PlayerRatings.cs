using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerRatings
	{
		public List<PlayerRatingSample> MatchRating { get; set; }
		public List<PlayerRatingSample> KillRating { get; set; }
	}
}
