using Moserware.Skills;

namespace BeRated.Cache
{
    class RatingPair
    {
        public Rating PreGameRating { get; private set; }

        public Rating PostGameRating { get; private set; }

        public RatingPair(Rating preGameRating, Rating postGameRating)
        {
            PreGameRating = preGameRating;
            PostGameRating = postGameRating;
        }
    }
}
