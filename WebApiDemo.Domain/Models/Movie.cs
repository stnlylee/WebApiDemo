using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Models
{
    [ExcludeFromCodeCoverage]
    public class Movie
    {
        public int MovieId { get; set; }

        public string Title { get; set; }

        public string Genre { get; set; }

        public string Classification { get; set; }

        public int ReleaseDate { get; set; }

        public int Rating { get; set; }

        public string[] Cast { get; set; }
    }
}
