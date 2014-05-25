using Ashod.Database;
using Npgsql;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BeRated
{
	class Service
	{
		private Configuration _Configuration;
		private DbConnection _Connection;
		private DatabaseFactory _Factory;

		private string _CurrentMap = null;

		public Service(Configuration configuration)
		{
			_Configuration = configuration;
		}

		private bool IsWellFormedPacket(byte[] packet)
		{
			if (packet.Length <= 7)
				return false;
			for (int i = 0; i < 4; i++)
			{
				byte input = packet[i];
				if (input != 0xff)
					return false;
			}
			if (packet[4] != 'R' || packet[packet.Length - 1] != 0)
				return false;
			return true;
		}

		private string GetStringFromPacket(byte[] packet)
		{
			var packetWithoutHeader = packet.Skip(5);
			var packetWithoutSuffix = packetWithoutHeader.Take(packetWithoutHeader.Count() - 1);
			byte[] payload = packetWithoutSuffix.ToArray();
			string output = Encoding.UTF8.GetString(payload);
			return output;
		}

		private void Test()
		{
			ProcessLogLine("L 05/23/2014 - 19:10:37: Started map \"de_dust2\" (CRC \"1333465166\")");
			while (true)
			{
				ProcessLogLine("L 05/23/2014 - 19:12:32: \"SomeName<2><STEAM_1:0:123456><TERRORIST>\" [-589 2261 -122] killed \"Tom<8><STEAM_1:0:343434><CT>\" [-478 1860 -63] with \"hkp2000\" (headshot)\n");
				Thread.Sleep(1000);
			}
		}

		public void Run()
		{
			_Connection = new NpgsqlConnection(_Configuration.ConnectionString);
			_Connection.Open();
			_Factory = new DatabaseFactory(_Connection);
			// Test();
			var addresses = Dns.GetHostAddresses(_Configuration.Host);
			if (addresses.Length == 0)
				throw new ApplicationException("Unable to resolve host");
			var address = addresses.First();
			var localEndPoint = new IPEndPoint(address, _Configuration.Port);
			var client = new UdpClient(localEndPoint);
			while (true)
			{
				IPEndPoint remoteEndPoint = null;
				byte[] packet = client.Receive(ref remoteEndPoint);
				if (!IsWellFormedPacket(packet))
				{
					// Unknown data
					continue;
				}
				string line = GetStringFromPacket(packet);
				try
				{
					ProcessLogLine(line);
				}
				catch (Exception exception)
				{
					Console.WriteLine("Failed to process line: {0} ({1})", exception.Message, exception.GetType());
					Console.WriteLine(line);
				}
			}
		}

		private void ProcessLogLine(string line)
		{
			var mapPattern = new Regex("^L \\d{2}\\/\\d{2}\\/\\d+ - \\d{2}:\\d{2}:\\d{2}: Started map \"(.+?)\" \\(CRC \"\\d+\"\\)");
			var mapMatch = mapPattern.Match(line);
			if (mapMatch.Success)
			{
				ProcessMap(mapMatch);
				return;
			}
			var killPattern = new Regex("^L (\\d{2})\\/(\\d{2})\\/(\\d+) - (\\d{2}):(\\d{2}):(\\d{2}): \"(.+?)<\\d+><(.+?)><(TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] killed \"(.+?)<\\d+><(.+?)><(?:TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] with \"(.+?)\"( \\(headshot\\))?");
			var killMatch = killPattern.Match(line);
			if (killMatch.Success)
			{
				ProcessKill(killMatch);
				return;
			}
		}

		private void ProcessMap(Match match)
		{
			_CurrentMap = match.Groups[1].Value;
			Console.WriteLine("Current map is {0}", _CurrentMap);
		}

		private void ProcessKill(Match match)
		{
			var groups = match.Groups;
			int offset = 1;
			Func<string> getString = () => groups[offset++].Value;
			Func<int> getInt = () => Convert.ToInt32(getString());
			int month = getInt();
			int day = getInt();
			int year = getInt();
			int hour = getInt();
			int minute = getInt();
			int second = getInt();
			string killerName = getString();
			string killerSteamId = getString();
			bool killerIsCT = getString() == "CT";
			int killerX = getInt();
			int killerY = getInt();
			int killerZ = getInt();
			var killerPosition = new Vector(killerX, killerY, killerZ);
			string victimName = getString();
			string victimSteamId = getString();
			int victimX = getInt();
			int victimY = getInt();
			int victimZ = getInt();
			var victimPosition = new Vector(victimX, victimY, victimZ);
			double distance = killerPosition.Distance(victimPosition);
			string weapon = getString();
			bool headshot = getString() != "";
			var time = new DateTime(year, month, day, hour, minute, second);
			const string botId = "BOT";
			if (killerSteamId == botId || victimSteamId == botId)
				return;
			using (var transaction = _Connection.BeginTransaction())
			{
				int killerId = UpdateOrCreatePlayer(killerSteamId, killerName);
				int victimId = UpdateOrCreatePlayer(victimSteamId, victimName);
				_Factory.NonQuery(
					"insert into kill (time, killer_id, victim_id, killer_is_ct, weapon, distance, map) " +
					"values (@time, @killerId, @victimId,  @killerIsCT, @weapon, @distance, @map)",
					new CommandParameter("@time", time),
					new CommandParameter("@killerId", killerId),
					new CommandParameter("@victimId", victimId),
					new CommandParameter("@killerIsCT", killerIsCT),
					new CommandParameter("@weapon", weapon),
					new CommandParameter("@distance", distance),
					new CommandParameter("@map", _CurrentMap)
					);
				Console.WriteLine("{0} killed {1} with weapon {2}", killerName, victimName, weapon);
				transaction.Commit();
			}
		}

		private int UpdateOrCreatePlayer(string steamId, string name)
		{
			// For the compiler
			int id = 0;
			bool playerExisted = false;
			using (var reader = _Factory.Reader("select id from player where steam_id = @steamId", new CommandParameter("@steamId", steamId)))
			{
				if (reader.Read())
				{
					id = reader.GetInt32("id");
					playerExisted = true;
				}
			}
			if (playerExisted)
			{
				_Factory.NonQuery(
						"update player set name = @name where id = @id",
						new CommandParameter("@name", name),
						new CommandParameter("@id", id)
						);
			}
			else
			{
				using (var command = _Factory.Command(
						"insert into player (steam_id, name) values (@steamId, @name) returning id",
						new CommandParameter("@steamId", steamId),
						new CommandParameter("@name", name)
						))
				{
					id = (int)command.ExecuteScalar();
				}
			}
			return id;
		}
	}
}
