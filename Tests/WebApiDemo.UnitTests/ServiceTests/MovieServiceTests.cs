using Moq;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Domain.Repositories;
using WebApiDemo.Domain.Search;
using WebApiDemo.Service.Movie;
using Xunit;

namespace WebApiDemo.UnitTests.ServiceTests
{
    public class MovieServiceTests
    {
        private readonly Mock<IMovieRepository> _mockMovieRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly MovieService _movieService;

        public MovieServiceTests()
        {
            _mockMovieRepository = new Mock<IMovieRepository>();

            _mockLogger = new Mock<ILogger>();

            _movieService = new MovieService(_mockMovieRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task MovieService_GetAllMovies_Should_Get_All_Movies()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.GetAll(It.IsAny<string>()))
                .ReturnsAsync(LoadGetAllTestData());

            // Act
            var result = await _movieService.GetAllMovies();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task MovieService_Search_Should_Perform_Search()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.Search(It.IsAny<SearchOption>(), It.IsAny<string>()))
                .ReturnsAsync(LoadSearchTestData());

            // Act
            var result = await _movieService.Search(GenerateSearchOption());

            // Assert
            Assert.True(result.IsSearchOptionValid);
            Assert.NotNull(result.SearchResult);
            Assert.Equal(3, result.SearchResult.Count);
        }

        [Fact]
        public async Task MovieService_Add_Should_Add_Movie()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.Add(It.IsAny<Domain.Models.Movie>()))
                .ReturnsAsync(LoadGetByKeyTestData());

            // Act
            var result = await _movieService.Add(LoadGetByKeyTestData());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.MovieId);
        }

        [Fact]
        public async Task MovieService_Update_Should_Update_Movie()
        {
            // Arrange
            _mockMovieRepository.Setup(x => x.Update(It.IsAny<Domain.Models.Movie>()))
                .ReturnsAsync(LoadGetByKeyTestData());

            // Act
            var result = await _movieService.Update(LoadGetByKeyTestData());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.MovieId);
        }

        private SearchOption GenerateSearchOption()
        {
            return new SearchOption
            {
                MovieId = "3",
                Title = "3",
                Genre = "3",
                Classification = "c",
                MinReleaseDate = "2000",
                MaxReleaseDate = "2009",
                MinRating = "1",
                MaxRating = "5",
                Cast = "cast5"
            };
        }

        private (List<Domain.Models.Movie> SearchResult, bool IsSearchOptionValid) LoadSearchTestData()
        {
            (List<Domain.Models.Movie> SearchResult, bool IsSearchOptionValid) data =
                (LoadGetAllTestData(), true);
            
            return data;
        }
        
        private Domain.Models.Movie LoadGetByKeyTestData()
        {
            var list = LoadGetAllTestData();
            return list[2];
        }

        private List<Domain.Models.Movie> LoadGetAllTestData()
        {
            return new List<Domain.Models.Movie>
            {
                new Domain.Models.Movie
                {
                    MovieId = 3,
                    Title = "movie3",
                    Genre = "genre3",
                    Classification = "cc",
                    ReleaseDate = 2008,
                    Rating = 4,
                    Cast = new string[] {"cast5", "cast6"}
                },
                new Domain.Models.Movie
                {
                    MovieId = 2,
                    Title = "movie2",
                    Genre = "genre2",
                    Classification = "c",
                    ReleaseDate = 2000,
                    Rating = 5,
                    Cast = new string[] {"cast3", "cast4"}
                },
                new Domain.Models.Movie
                {
                    MovieId = 1,
                    Title = "movie1",
                    Genre = "genre1",
                    Classification = "a",
                    ReleaseDate = 1999,
                    Rating = 1,
                    Cast = new string[] {"cast1", "cast2"}
                },
            };
        }
    }
}
