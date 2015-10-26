﻿using System;

namespace BeRated.Logging.Data
{
	public class EndOfRound
	{
		public DateTime Time { get; set; }
		public string TriggeringTeam { get; set; }
		public string SfuiNotice { get; set; }
		public int TerroristScore { get; set; }
		public int CounterTerroristScore { get; set; }
	}
}
