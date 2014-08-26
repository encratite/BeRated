namespace BeRated
{
	class GamePlayer
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public GamePlayer(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
