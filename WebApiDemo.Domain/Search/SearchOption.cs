using System;
using System.Collections.Generic;

namespace WebApiDemo.Domain.Search
{
    public class SearchOption
    {
        public string MovieId { get; set; }

        public int MovieIdInt => TryParseStringToInt(MovieId);
        
        public string Title { get; set; }
        
        public string Genre { get; set; }
        
        public string Classification { get; set; }
        
        public string MinReleaseDate { get; set; }

        public int MinReleaseDateInt => TryParseStringToInt(MinReleaseDate);

        public string MaxReleaseDate { get; set; }

        public int MaxReleaseDateInt => TryParseStringToInt(MaxReleaseDate);

        public string MinRating { get; set; }

        public int MinRatingInt => TryParseStringToInt(MinRating);

        public string MaxRating { get; set; }

        public int MaxRatingInt => TryParseStringToInt(MaxRating);

        public string Cast { get; set; }
        
        public bool Valid
        {
            get
            {
                try
                {
                    if ((!string.IsNullOrEmpty(MovieId) && MovieIdInt <= 0)
                        || (!string.IsNullOrEmpty(MinReleaseDate) && MinReleaseDateInt <= 0)
                        || (!string.IsNullOrEmpty(MaxReleaseDate) && MaxReleaseDateInt <= 0)
                        || (!string.IsNullOrEmpty(MinReleaseDate) && !string.IsNullOrEmpty(MaxReleaseDate) && MinReleaseDateInt > MaxReleaseDateInt)
                        || (!string.IsNullOrEmpty(MinRating) && MinRatingInt <= 0)
                        || (!string.IsNullOrEmpty(MaxRating) && MaxRatingInt <= 0)
                        || (!string.IsNullOrEmpty(MinRating) && !string.IsNullOrEmpty(MaxRating) && MinRatingInt > MaxRatingInt)
                        || (!string.IsNullOrEmpty(MinRating) && !ValidRatings.Contains(MinRatingInt))
                        || (!string.IsNullOrEmpty(MaxRating) && !ValidRatings.Contains(MaxRatingInt)))
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private int TryParseStringToInt(string s)
        {
            int i = int.MinValue;
            if (!string.IsNullOrEmpty(s))
            {
                if (int.TryParse(s, out i))
                {
                    if (i <= 0)
                    {
                        return int.MinValue;
                    }

                    return i;
                }

                return int.MinValue;
            }

            return i;
        }

        private List<int> ValidRatings = new List<int> { 1, 2, 3, 4, 5 };
    }
}
