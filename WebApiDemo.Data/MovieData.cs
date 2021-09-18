using System;
using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Data
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class MovieData
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
