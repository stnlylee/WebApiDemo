using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Dto
{
    [ExcludeFromCodeCoverage]
    public abstract class MovieDtoBase
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
