using BeRated.Database;

namespace BeRated.Model
{
    class AllPlayerStatsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int TeamKills { get; set; }
        // Only set if Deaths > 0
        public decimal? KillDeathRatio { get; set; }
        public int RoundsPlayed { get; set; }
        public decimal WinPercentage { get; set; }
        public int RoundsPlayedTerrorist { get; set; }
        public decimal WinPercentageTerrorist { get; set; }
        public int RoundsPlayedCounterTerrorist { get; set; }
        public decimal WinPercentageCounterTerrorist { get; set; }
        public int GamesPlayed { get; set; }
        public decimal GameWinPercentage { get; set; }
    }
}
