namespace BeRated.Common
{
	static class Ratio
	{
		public static decimal? Get(int numerator, int denominator, bool nullIfZero = false)
        {
            if (nullIfZero && numerator == 0)
                return null;
            if (denominator != 0)
                return (decimal)numerator / denominator;
            else
                return null;
        }
	}
}
