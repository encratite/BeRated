﻿using System;
using System.Text.RegularExpressions;

namespace BeRated.Cache
{
	class LogParser
    {
		public const string BotId = "BOT";
		public const string TerroristTeam = "TERRORIST";
		public const string CounterTerroristTeam = "CT";

		private const string DatePrefix = "^L (\\d{2})\\/(\\d{2})\\/(\\d+) - (\\d{2}):(\\d{2}):(\\d{2}): ";
		private const string PlayerPattern = "\"(.+?)<\\d+><(.+?)><(.*?)>\"";
		private const string PlayerWithoutTeamPattern = "\"(.+?)<\\d+><(.+?)>\"";

		private static Regex KillPattern = new Regex(DatePrefix + PlayerPattern + " \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] killed " + PlayerPattern + " \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] with \"(.+?)\"( \\(headshot\\))?");
		private static Regex MaxRoundsPattern = new Regex(DatePrefix + "server_cvar: \"mp_maxrounds\" \"(\\d+)\"");
		// grep -h -E -o "SFUI_[^""]+" * | sort | uniq
		private static Regex EndOfRoundPattern = new Regex(DatePrefix + "Team \"(TERRORIST|CT)\" triggered \"(SFUI_Notice_All_Hostages_Rescued|SFUI_Notice_Bomb_Defused|SFUI_Notice_CTs_Win|SFUI_Notice_Hostages_Not_Rescued|SFUI_Notice_Target_Bombed|SFUI_Notice_Target_Saved|SFUI_Notice_Terrorists_Win)\" \\(CT \"(\\d+)\"\\) \\(T \"(\\d+)\"\\)");
		private static Regex TeamSwitchPattern = new Regex(DatePrefix + PlayerWithoutTeamPattern + " switched from team <(.+?)> to <(.+?)>");
		private static Regex DisconnectPattern = new Regex(DatePrefix + PlayerPattern + " disconnected \\(reason \"(.+?)\"\\)");
		private static Regex PurchasePattern = new Regex(DatePrefix + PlayerPattern + " purchased \"(.+?)\"");

		private CacheManager _Cache;

		public LogParser(CacheManager cache)
		{
			_Cache = cache;
		}

		public Kill ReadPlayerKill(string line)
		{
			var match = KillPattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			var time = ReadDate(reader);
			string killerName = reader.String();
			string killerSteamId = reader.String();
			var killerTeam = reader.Team();
			int killerX = reader.Int();
			int killerY = reader.Int();
			int killerZ = reader.Int();
			string victimName = reader.String();
			string victimSteamId = reader.String();
			var victimTeam = reader.Team();
			int victimX = reader.Int();
			int victimY = reader.Int();
			int victimZ = reader.Int();
			string weapon = reader.String();
			bool headshot = reader.String() != "";
			var output = new Kill
			{
				Time = time,
				Killer = GetPlayer(killerName, killerSteamId),
				KillerTeam = killerTeam,
				KillerPosition = new Vector(killerX, killerY, killerZ),
				Victim = GetPlayer(victimName, victimSteamId),
				VictimTeam = victimTeam,
				VictimPosition = new Vector(victimX, victimY, victimZ),
				Headshot = headshot,
				Weapon = weapon,
			};
			return output;
		}

		public int? ReadMaxRounds(string line)
		{
			var match = MaxRoundsPattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			ReadDate(reader);
			int maxRounds = reader.Int();
			return maxRounds;
		}

		public EndOfRound ReadEndOfRound(string line)
		{
			var match = EndOfRoundPattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			var time = ReadDate(reader);
			string triggeringTeam = reader.String();
			string sfuiNotice = reader.String();
			int counterTerroristScore = reader.Int();
			int terroristScore = reader.Int();
			var output = new EndOfRound
			{
				Time = time,
				TriggeringTeam = triggeringTeam,
				SfuiNotice = sfuiNotice,
				TerroristScore = terroristScore,
				CounterTerroristScore = counterTerroristScore,
			};
			return output;
		}

		public TeamSwitch ReadTeamSwitch(string line)
		{
			var match = TeamSwitchPattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			var time = ReadDate(reader);
			string name = reader.String();
			string steamId = reader.String();
			var previousTeam = reader.Team();
			var currentTeam = reader.Team();
			var output = new TeamSwitch
			{
				Time = time,
				Player = GetPlayer(name, steamId),
				PreviousTeam = previousTeam,
				CurrentTeam = currentTeam,
			};
			return output;
		}

		public Disconnect ReadDisconnect(string line)
		{
			var match = DisconnectPattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			var time = ReadDate(reader);
			string name = reader.String();
			string steamId = reader.String();
			string team = reader.String();
			string reason = reader.String();
			var output = new Disconnect
			{
				Time = time,
				Player = GetPlayer(name, steamId),
				Team = team,
				Reason = reason,
			};
			return output;
		}

		public Purchase ReadPurchase(string line)
		{
			var match = PurchasePattern.Match(line);
			if (!match.Success)
				return null;
			var reader = new MatchReader(match);
			var time = ReadDate(reader);
			string name = reader.String();
			string steamId = reader.String();
			var team = reader.Team();
			string item = reader.String();
			var output = new Purchase
			{
				Time = time,
				Player = GetPlayer(name, steamId),
				Team = team,
				Item = item
			};
			return output;
		}

		private DateTime ReadDate(MatchReader reader)
		{
			int month = reader.Int();
			int day = reader.Int();
			int year = reader.Int();
			int hour = reader.Int();
			int minute = reader.Int();
			int second = reader.Int();
			var output = new DateTime(year, month, day, hour, minute, second);
			return output;
		}

		private Player GetPlayer(string name, string steamId)
		{
			var player = _Cache.GetPlayer(name, steamId);
			return player;
		}
    }
}