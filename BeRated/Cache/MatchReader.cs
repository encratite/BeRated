using System.Text.RegularExpressions;

namespace BeRated.Cache
{
	class MatchReader
	{
		private int _Offset = 1;
		private Match _Match;

		public MatchReader(Match match)
		{
			_Match = match;
		}

		public string String()
		{
			string output = _Match.Groups[_Offset].Value;
			_Offset++;
			return output;
		}

		public int Int()
		{
			string intString = String();
			int output = int.Parse(intString);
			return output;
		}
	}
}
