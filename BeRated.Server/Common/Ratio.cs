namespace BeRated.Common
{
	static class Ratio
	{
		public static decimal? Get(int numerator, int denominator)
        {
            if (denominator != 0)
                return (decimal)numerator / denominator;
            else
                return null;
        }
	}
}
