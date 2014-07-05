using System.Text.RegularExpressions;

namespace BeRated
{
	class MatchReader
	{
		int _Offset = 1;
		Match _Match;

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
