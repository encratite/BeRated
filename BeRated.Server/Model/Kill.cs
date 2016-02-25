using BeRated.Common;
using System;

namespace BeRated.Model
{
    public class Kill
    {
        public DateTime Time { get; set; }
		public PlayerInfo Killer { get; set; }
        public PlayerInfo Victim { get; set; }
        public Team KillerTeam { get; set; }
        public string Weapon { get; set; }
    }
}
