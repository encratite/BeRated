using BeRated.Common;

namespace BeRated.Cache
{
    class ItemPair
    {
        public string CounterTerroristItem { get; private set; }
        public string TerroristItem { get; private set; }

        public ItemPair(string counterTerroristItem, string terroristItem)
        {
            CounterTerroristItem = counterTerroristItem;
            TerroristItem = terroristItem;
        }

        public bool Translate(string item, Team team, ref string output)
        {
            if (item == CounterTerroristItem || item == TerroristItem)
            {
                output = team == Team.CounterTerrorist ? CounterTerroristItem : TerroristItem;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
