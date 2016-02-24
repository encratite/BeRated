using System.Collections.Generic;

namespace BeRated.Model
{
	public class PlayerItems : PlayerInfo
	{
		public List<PlayerItemStats> Items { get; set; }

        public PlayerItems(string name, string steamId, List<PlayerItemStats> items)
            : base(name, steamId)
        {
            Items = items;
        }
	}
}
