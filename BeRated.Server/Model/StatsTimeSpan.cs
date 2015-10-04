namespace BeRated.Model
{
	public class StatsTimeSpan
	{
		public int? Days { get; private set; }

		public string Description { get; private set; }

		public StatsTimeSpan(int? days, string description)
		{
			Days = days;
			Description = description;
		}
	}
}
