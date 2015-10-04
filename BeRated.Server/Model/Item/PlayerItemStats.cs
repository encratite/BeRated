namespace BeRated.Model.Item
{
	public class PlayerItemStats
	{
		public string Item { get; set; }
		public int TimesPurchased { get; set; }
		public decimal PurchasesPerRound { get; set; }
		// Only set for weapons
		public decimal? KillsPerPurchase { get; set; }
	}
}
