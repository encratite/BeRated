using System;
using System.Collections.Generic;
using System.Linq;

namespace BeRated.Cache
{
	class Player
	{
		public string Name { get; private set; }

		public string SteamId { get; private set; }

		public List<Kill> Kills { get; private set; }
		public List<Kill> Deaths { get; private set; }

		public List<Round> RoundsWon { get; private set; }
		public List<Round> RoundsLost { get; private set; }

        public IEnumerable<Round> Rounds
        {
            get
            {
                return RoundsWon.Concat(RoundsLost);
            }
        }

		public List<Game> Wins { get; private set; }
		public List<Game> Losses { get; private set; }
		public List<Game> Draws { get; private set; }

        public IEnumerable<Game> Games
        {
            get
            {
                return Wins.Concat(Losses).Concat(Draws);
            }
        }

		public List<Purchase> Purchases { get; private set; }

        private DateTime _NameTime;

		public Player(string name, string steamId, DateTime time)
		{
			Name = name;
			SteamId = steamId;

			Kills = new List<Kill>();
			Deaths = new List<Kill>();

			RoundsWon = new List<Round>();
			RoundsLost = new List<Round>();

			Wins = new List<Game>();
			Losses = new List<Game>();
			Draws = new List<Game>();

			Purchases = new List<Purchase>();

            _NameTime = time;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, SteamId);
		}

        public void SetName(string name, DateTime time)
        {
            if (time < _NameTime)
                return;
            Name = name;
            _NameTime = time;
        }
	}
}
