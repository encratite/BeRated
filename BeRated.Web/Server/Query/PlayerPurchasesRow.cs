using System;

namespace BeRated.Query
{
	class PlayerPurchasesRow
	{
		public string Item { get; set; }
		public int TimesPurchased { get; set; }
		public Decimal PurchasesPerRound { get; set; }
	}
}
