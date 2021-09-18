using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Dto.Requests
{
    [ExcludeFromCodeCoverage]
    public class SearchMovieRequest
    {
        public string MovieId { get; set; }

        public string Title { get; set; }

        public string Genre { get; set; }

        public string Classification { get; set; }

        public string MinReleaseDate { get; set; }

        public string MaxReleaseDate { get; set; }

        public string MinRating { get; set; }

        public string MaxRating { get; set; }

        public string Cast { get; set; }

        public string SortOrder { get; set; }
    }
}
