using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BeRated
{
	class Service
	{
		private Configuration _Configuration;

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
				ProcessLogLine(line);
			}
		}

		private void ProcessLogLine(string line)
		{
			// 05/23/2014 - 19:12:32: "SomeName<2><STEAM_1:0:123456><TERRORIST>" [-589 2261 -122] killed "Tom<8><BOT><CT>" [-478 1860 -63] with "hkp2000" (headshot)
		}
	}
}
