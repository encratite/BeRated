﻿using Ashod.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BeRated
{
	class Uploader: IDisposable
	{
		const int MaxRoundsDefault = 30;

		string _LogPath;
		string _ConnectionString;
		DatabaseCommander _Database;
		int _MaxRounds = MaxRoundsDefault;
		Dictionary<string, string> _Players = new Dictionary<string, string>();

		public Uploader(string logPath, string connectionString)
		{
			_LogPath = logPath;
			_ConnectionString = connectionString;
		}

		void IDisposable.Dispose()
		{
			if (_Database != null)
			{
				_Database.Dispose();
				_Database = null;
			}
		}

		public void Run()
		{
			var connection = new NpgsqlConnection(_ConnectionString);
			_Database = new DatabaseCommander(connection);
			var files = Directory.GetFiles(_LogPath);
			using (var transaction = _Database.Transaction())
			{
				_Database.NonQueryFunction("lock_tables");
				foreach (var file in files)
					ProcessLog(file);
				transaction.Commit();
			}
		}

		void ProcessLog(string path)
		{
			_MaxRounds = MaxRoundsDefault;
			_Players = new Dictionary<string, string>();
			Console.WriteLine(path);
			var lines = File.ReadLines(path);
			int lineCounter = 1;
			foreach (var line in lines)
			{
				ProcessLine(line, lineCounter);
				lineCounter++;
			}
		}

		void ProcessLine(string line, int lineCounter)
		{
			var kill = LogParser.ReadPlayerKill(line);
			if (kill != null)
			{
				if (kill.Killer.SteamId == LogParser.BotId || kill.Victim.SteamId == LogParser.BotId)
					return;
				var parameters = new []
				{
					new CommandParameter("kill_time", kill.Time),
					new CommandParameter("killer_steam_id", kill.Killer.SteamId),
					new CommandParameter("killer_team", kill.KillerTeam),
					new CommandParameter("killer_x", kill.KillerPosition.X),
					new CommandParameter("killer_y", kill.KillerPosition.Y),
					new CommandParameter("killer_z", kill.KillerPosition.Z),
					new CommandParameter("victim_steam_id", kill.Victim.SteamId),
					new CommandParameter("victim_team", kill.VictimTeam),
					new CommandParameter("victim_x", kill.VictimPosition.X),
					new CommandParameter("victim_y", kill.VictimPosition.Y),
					new CommandParameter("victim_z", kill.VictimPosition.Z),
					new CommandParameter("weapon", kill.Weapon),
					new CommandParameter("headshot", kill.Headshot),
				};
				_Database.NonQueryFunction("process_kill", parameters);
				return;
			}
			int? maxRounds = LogParser.ReadMaxRounds(line);
			if (maxRounds != null)
			{
				_MaxRounds = maxRounds.Value;
				return;
			}
			var teamSwitch = LogParser.ReadTeamSwitch(line);
			if (teamSwitch != null)
			{
				string steamId = teamSwitch.Player.SteamId;
				string team = teamSwitch.CurrentTeam;
				if (steamId == LogParser.BotId || (team != LogParser.TerroristTeam && team != LogParser.CounterTerroristTeam))
					return;
				_Players[steamId] = team;
				var parameters = new[]
				{
					new CommandParameter("name", teamSwitch.Player.Name),
					new CommandParameter("steam_id", steamId),
				};
				_Database.NonQueryFunction("update_player", parameters);
				return;
			}
			var disconnect = LogParser.ReadDisconnect(line);
			if (disconnect != null)
			{
				string steamId = disconnect.Player.SteamId;
				if (steamId == LogParser.BotId)
					return;
				_Players.Remove(steamId);
			}
			var endOfRound = LogParser.ReadEndOfRound(line);
			if (endOfRound != null)
			{
				if (endOfRound.TerroristScore == 0 && endOfRound.CounterTerroristScore == 0)
					return;
				string terroristIds = GetSteamIdsString(LogParser.TerroristTeam);
				string counterTerroristIds = GetSteamIdsString(LogParser.CounterTerroristTeam);
				var parameters = new[]
				{
					new CommandParameter("end_of_round_time", endOfRound.Time),
					new CommandParameter("triggering_team", endOfRound.TriggeringTeam),
					new CommandParameter("sfui_notice", endOfRound.SfuiNotice),
					new CommandParameter("terrorist_score", endOfRound.TerroristScore),
					new CommandParameter("counter_terrorist_score", endOfRound.CounterTerroristScore),
					new CommandParameter("max_rounds", _MaxRounds),
					new CommandParameter("terrorist_steam_ids", terroristIds),
					new CommandParameter("counter_terrorist_steam_ids", counterTerroristIds),
				};
				_Database.NonQueryFunction("process_end_of_round", parameters);
				return;
			}
			var purchase = LogParser.ReadPurchase(line);
			if (purchase != null)
			{
				string steamId = purchase.Player.SteamId;
				if (steamId == LogParser.BotId)
					return;
				string team = _Players[steamId];
				var parameters = new[]
				{
					new CommandParameter("steam_id", steamId),
					new CommandParameter("line", lineCounter),
					new CommandParameter("purchase_time", purchase.Time),
					new CommandParameter("team", team),
					new CommandParameter("item", purchase.Item),
				};
				_Database.NonQueryFunction("process_purchase", parameters);
				return;
			}
		}

		string GetSteamIdsString(string team)
		{
			var players = _Players.Where(pair => pair.Value == team).Select(pair => pair.Key).ToArray();
			string output = string.Join(",", players);
			return output;
		}
	}
}
