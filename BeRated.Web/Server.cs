using Ashod.WebSocket;

namespace BeRated
{
	class Server : WebSocketServer
	{
		string _ConnectionString;

		public Server(int webSocketPort, string connectionString)
			: base(webSocketPort)
		{
			_ConnectionString = connectionString;
		}

		public override void Run()
		{
			base.Run();
		}

		#region Web socket methods

		[WebSocketServerMethod]
		string Test(int integerArgument, string stringArgument)
		{
			return "Hello World";
		}

		#endregion
	}
}
