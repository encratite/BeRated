namespace BeRated.Model
{
	public class PlayerItemStats
	{
		public string Item { get; set; }
		public int TimesPurchased { get; set; }
		public decimal? PurchasesPerRound { get; set; }
		// Only set for weapons
		public decimal? KillsPerPurchase { get; set; }

		public PlayerItemStats(string item)
		{
			Item = item;
			TimesPurchased = 0;
			PurchasesPerRound = null;
			KillsPerPurchase = null;
		}
	}
}
