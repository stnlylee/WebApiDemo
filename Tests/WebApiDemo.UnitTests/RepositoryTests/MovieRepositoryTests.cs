using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Data;
using WebApiDemo.Domain.Cache;
using WebApiDemo.Domain.Mappings;
using WebApiDemo.Domain.Repositories;
using WebApiDemo.Domain.Search;
using Xunit;

namespace WebApiDemo.UnitTests.RepositoryTests
{
    public class MovieRepositoryTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IMovieDataSource> _mockMovieDataSource;
        private readonly IMapper _mapper;
        private readonly Mock<IDistributedCacheProvider> _mockDistributedCacheProvider;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly MovieRepository _movieRepository;

        public MovieRepositoryTests()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MovieMappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;

            _mockMovieDataSource = new Mock<IMovieDataSource>();

            _mockLogger = new Mock<ILogger>();

            _mockDistributedCacheProvider = new Mock<IDistributedCacheProvider>();

            _mockConfig = new Mock<IConfiguration>();

            _movieRepository = new MovieRepository(_mockMovieDataSource.Object,
                _mapper, _mockLogger.Object, _mockDistributedCacheProvider.Object,
                _mockConfig.Object);
        }

        [Fact]
        public async Task MovieRepository_GetAll_Should_Return_All_Data()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetAllData()).Returns(LoadGetAllTestData());

            // Act
            var result = await _movieRepository.GetAll();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("movie1", result[0].Title);
        }

        [Theory]
        [InlineData("movieid")]
        [InlineData("title")]
        [InlineData("genre")]
        [InlineData("classification")]
        [InlineData("releasedate")]
        [InlineData("rating")]
        public async Task MovieRepository_GetAll_Should_Sort_Data(string sortOrder)
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetAllData()).Returns(LoadGetAllTestData());

            // Act
            var result = await _movieRepository.GetAll(sortOrder);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("movie1", result[0].Title);
        }

        [Fact]
        public async Task MovieRepository_GetByKey_Should_ThrowException()
        {
            // Arrange
            int key = 0;

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _movieRepository.GetByKey(key));

            // Assert
            _mockMovieDataSource.Verify(x => x.GetDataById(key), Times.Never);
            Assert.NotNull(ex);
            Assert.Equal("MovieRepository->GetByKey - movie key must be greater than 0", ex.Message);
        }

        [Fact]
        public async Task MovieRepository_GetByKey_Shoud_Get_Data()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetDataById(1)).Returns(LoadGetByKeyTestData());

            // Act
            var result = await _movieRepository.GetByKey(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("movie1", result.Title);
        }

        [Fact]
        public async Task MovieRepository_Search_Should_Return_Null_On_Invalid_SearchOption()
        {
            // Arrange
            SearchOption searchOption = new SearchOption { MovieId = "-1" };

            // Act
            var result = await _movieRepository.Search(searchOption);

            // Assert
            Assert.Null(result.SearchResult);
            Assert.False(result.IsSearchOptionValid);
        }

        [Fact]
        public async Task MovieRepository_Search_Should_Perform_Search_Return_Result()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetAllData()).Returns(LoadGetAllTestData());
            var searchOption = GenerateSearchOption();

            // Act
            var result = await _movieRepository.Search(searchOption);

            // Assert
            Assert.True(result.IsSearchOptionValid);
            Assert.NotNull(result.SearchResult);
            Assert.Single(result.SearchResult);
            Assert.Equal("movie3", result.SearchResult[0].Title);
        }

        [Fact]
        public async Task MovieRepository_Search_Should_Perform_Search_Return_Null()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetAllData()).Returns(LoadGetAllTestData());
            var searchOption = new SearchOption { MovieId = "4" };

            // Act
            var result = await _movieRepository.Search(searchOption);

            // Assert
            Assert.True(result.IsSearchOptionValid);
            Assert.Null(result.SearchResult);
        }

        [Fact]
        public async Task MovieRepository_Add_Should_ThrowException()
        {
            // Arrange
            Domain.Models.Movie movie = null;

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _movieRepository.Add(movie));

            // Assert
            _mockMovieDataSource.Verify(x => x.Create(It.IsAny<MovieData>()), Times.Never);
            Assert.NotNull(ex);
            Assert.Equal("MovieRepository->Add - movie must not be null", ex.Message);
        }

        [Fact]
        public async Task MovieRepository_Add_Should_Add_Movie()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetDataById(1)).Returns(LoadGetByKeyTestData());
            _mockMovieDataSource.Setup(x => x.Create(It.IsAny<MovieData>())).Returns(1);

            // Act
            var result = await _movieRepository.Add(new Domain.Models.Movie());

            // Assert
            Assert.NotNull(result);
            Assert.Equal("movie1", result.Title);
        }

        [Fact]
        public async Task MovieRepository_Update_Should_ThrowException()
        {
            // Arrange
            Domain.Models.Movie movie = null;
            Domain.Models.Movie movieWithInvalidId = new Domain.Models.Movie();

            // Act
            var ex = await Assert .ThrowsAsync<ArgumentException>(() => _movieRepository.Update(movie));
            var ex2 = await Assert.ThrowsAsync<ArgumentException>(() => _movieRepository.Update(movieWithInvalidId));

            // Assert
            _mockMovieDataSource.Verify(x => x.Update(It.IsAny<MovieData>()), Times.Never);
            Assert.NotNull(ex);
            Assert.NotNull(ex2);
            string expectedErr = "MovieRepository->Update - movie must not be null and movie id must be greater than 0";
            Assert.Equal(expectedErr, ex.Message);
            Assert.Equal(expectedErr, ex2.Message);
        }

        [Fact]
        public async Task MovieRepository_Update_Should_Update_Movie()
        {
            // Arrange
            _mockMovieDataSource.Setup(x => x.GetDataById(1)).Returns(LoadGetByKeyTestData());
            _mockMovieDataSource.Setup(x => x.Update(It.IsAny<MovieData>())).Verifiable();

            // Act
            var result = await _movieRepository.Update(new Domain.Models.Movie { MovieId = 1 });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("movie1", result.Title);
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

        private MovieData LoadGetByKeyTestData()
        {
            var list = LoadGetAllTestData();
            return list[2];
        } 
        
        private List<MovieData> LoadGetAllTestData()
        {
            return new List<MovieData>
            {
                new MovieData
                {
                    MovieId = 3,
                    Title = "movie3",
                    Genre = "genre3",
                    Classification = "cc",
                    ReleaseDate = 2008,
                    Rating = 4,
                    Cast = new string[] {"cast5", "cast6"}
                },
                new MovieData
                {
                    MovieId = 2,
                    Title = "movie2",
                    Genre = "genre2",
                    Classification = "c",
                    ReleaseDate = 2000,
                    Rating = 5,
                    Cast = new string[] {"cast3", "cast4"}
                },
                new MovieData
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
