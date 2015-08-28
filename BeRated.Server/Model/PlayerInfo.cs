namespace BeRated.Model
{
	public class PlayerInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public PlayerInfo()
		{
		}

		public PlayerInfo(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
