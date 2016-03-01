namespace BeRated.Model
{
    public class GameRating
    {
        public double PreGameRating { get; private set; }
        public double PostGameRating { get; private set; }

        public GameRating(double preGameRating, double postGameRating)
        {
            PreGameRating = preGameRating;
            PostGameRating = postGameRating;
        }
    }
}
