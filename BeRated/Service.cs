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

		public Service(Configuration configuration)
		{
			_Configuration = configuration;
		}

		private bool IsWellFormedPacket(byte[] packet)
		{
			if (packet.Length <= 5)
				return false;
			for (int i = 0; i < 4; i++)
			{
				byte input = packet[i];
				if (input != 0xff)
					return false;
			}
			byte lastByte = packet[packet.Length - 1];
			if (lastByte != 0)
				return false;
			return true;
		}

		private string GetStringFromPacket(byte[] packet)
		{
			var packetWithoutHeader = packet.Skip(4);
			var packetWithoutSuffix = packetWithoutHeader.Take(packetWithoutHeader.Count() - 1);
			byte[] payload = packetWithoutSuffix.ToArray();
			string output = Encoding.UTF8.GetString(payload);
			return output;
		}

		public void Run()
		{
			_Connection = new NpgsqlConnection(_Configuration.ConnectionString);
			_Connection.Open();
			_Factory = new DatabaseFactory(_Connection);
			while (true)
			{
				ProcessLogLine("RL 05/23/2014 - 19:12:32: \"SomeName<2><STEAM_1:0:123456><TERRORIST>\" [-589 2261 -122] killed \"Tom<8><BOT><CT>\" [-478 1860 -63] with \"hkp2000\" (headshot)\n");
				Thread.Sleep(1000);
			}
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
			var pattern = new Regex("RL (\\d{2})\\/(\\d{2})\\/(\\d+) - (\\d{2}):(\\d{2}):(\\d{2}): \"(.+?)<\\d+><(.+?)><(TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] killed \"(.+?)<\\d+><(.+?)><(?:TERRORIST|CT)>\" \\[(-?\\d+) (-?\\d+) (-?\\d+)\\] with \"(.+?)\"( \\(headshot\\))?");
			var match = pattern.Match(line);
			if (!match.Success)
				return;
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
			using (var transaction = _Connection.BeginTransaction())
			{
				int killerId;
				int killerOldRating;
				UpdateOrCreatePlayer(killerSteamId, killerName, out killerId, out killerOldRating);
				int victimId;
				int victimOldRating;
				UpdateOrCreatePlayer(victimSteamId, victimName, out victimId, out victimOldRating);
				var killerRating = new PlayerRating(killerOldRating);
				var victimRating = new PlayerRating(victimOldRating);
				PlayerRating.UpdateRatings(killerRating, victimRating);
				UpdateRating(killerId, killerRating.Rating);
				UpdateRating(victimId, victimRating.Rating);
				_Factory.NonQuery(
					"insert into kill (time, killer_id, killer_old_rating, killer_new_rating, victim_id, victim_old_rating, victim_new_rating, killer_is_ct, weapon, distance) " +
					"values (@time, @killerId, @killerOldRating, @killerNewRating, @victimId, @victimOldRating, @victimNewRating, @killerIsCT, @weapon, @distance)",
					new CommandParameter("@time", time),
					new CommandParameter("@killerId", killerId),
					new CommandParameter("@killerOldRating", killerOldRating),
					new CommandParameter("@killerNewRating", killerRating.Rating),
					new CommandParameter("@victimId", victimId),
					new CommandParameter("@victimOldRating", victimOldRating),
					new CommandParameter("@victimNewRating", victimRating.Rating),
					new CommandParameter("@killerIsCT", killerIsCT),
					new CommandParameter("@weapon", weapon),
					new CommandParameter("@distance", distance)
					);
				Console.WriteLine("{0} ({1}) killed {2} ({3}), gaining {4} rating", killerName, killerRating.Rating, victimName, victimRating.Rating, killerRating.Rating - killerOldRating);
				transaction.Commit();
			}
		}

		private void UpdateOrCreatePlayer(string steamId, string name, out int id, out int rating)
		{
			bool playerExisted = false;
			using (var reader = _Factory.Reader("select id, rating from player where steam_id = @steamId", new CommandParameter("@steamId", steamId)))
			{
				if (reader.Read())
				{
					id = reader.GetInt32("id");
					rating = reader.GetInt32("rating");
					playerExisted = true;
				}
				else
				{
					// For the compiler
					id = 0;
					rating = PlayerRating.InitialRating;
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
						"insert into player (steam_id, name, rating) values (@steamId, @name, @rating) returning id",
						new CommandParameter("@steamId", steamId),
						new CommandParameter("@name", name),
						new CommandParameter("@rating", rating)
						))
				{
					id = (int)command.ExecuteScalar();
				}
			}
		}

		private void UpdateRating(int id, int rating)
		{
			_Factory.NonQuery(
				"update player set rating = @rating where id = @id",
				new CommandParameter("@rating", rating),
				new CommandParameter("@id", id)
				);
		}
	}
}
