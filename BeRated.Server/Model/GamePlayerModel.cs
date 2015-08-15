namespace BeRated.Model
{
	class GamePlayerModel
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public GamePlayerModel(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
