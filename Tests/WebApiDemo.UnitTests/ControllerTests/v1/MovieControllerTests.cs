using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Api.Controllers.v1;
using WebApiDemo.Domain.Dto.Requests;
using WebApiDemo.Domain.Dto.Responses;
using WebApiDemo.Domain.Mappings;
using WebApiDemo.Domain.Search;
using WebApiDemo.Service.Movie;
using Xunit;

namespace WebApiDemo.UnitTests.ControllerTests.v1
{
    public class MovieControllerTests
    {
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<ILogger> _mockLogger;
        private readonly IMapper _mapper;
        private readonly MovieController _movieController;

        public MovieControllerTests()
        {
            _mockMovieService = new Mock<IMovieService>();

            _mockLogger = new Mock<ILogger>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MovieMappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;

            _movieController = new MovieController(_mockMovieService.Object, 
                _mockLogger.Object, _mapper);
        }

        [Fact]
        public async Task MovieController_GetAllMovies_Should_Return_NotFoundResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.GetAllMovies(It.IsAny<string>()))
                .ReturnsAsync(() => { return null; });

            // Act
            var result = await _movieController.GetAllMovies();

            // Assert
            Assert.IsType<ActionResult<List<MovieResponse>>>(result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.Equal(404, notFoundResult.StatusCode);
        }


        [Fact]
        public async Task MovieController_GetAllMovies_Should_Return_OkResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.GetAllMovies(It.IsAny<string>()))
                .ReturnsAsync(LoadGetAllTestData());

            // Act
            var result = await _movieController.GetAllMovies();

            // Assert
            Assert.IsType<ActionResult<List<MovieResponse>>>(result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult.Value as List<MovieResponse>;
            Assert.Equal(3, value.Count);
        }

        [Fact]
        public async Task MovieController_AddMovie_Should_Return_OkResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.Add(It.IsAny<Domain.Models.Movie>()))
                .ReturnsAsync(LoadGetByKeyTestData());

            // Act
            var result = await _movieController.AddMovie(new AddMovieRequest { Title = "movie1" });

            // Assert
            Assert.IsType<ActionResult<MovieResponse>>(result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult.Value as MovieResponse;
            Assert.Equal("movie1", value.Title);
        }

        [Fact]
        public async Task MovieController_UpdateMovie_Should_Return_OkResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.Update(It.IsAny<Domain.Models.Movie>()))
                .ReturnsAsync(LoadGetByKeyTestData());

            // Act
            var result = await _movieController.UpdateMovie(new UpdateMovieRequest { Title = "movie1" });

            // Assert
            Assert.IsType<ActionResult<MovieResponse>>(result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult.Value as MovieResponse;
            Assert.Equal("movie1", value.Title);
        }

        [Fact]
        public async Task MovieController_Search_Should_Return_BadRequestResult()
        {
            // Arrange
            SearchMovieRequest badRequest = new SearchMovieRequest
            {
                MinRating = "7"
            };

            // Act
            var result = await _movieController.Search(badRequest);

            // Assert
            Assert.IsType<ActionResult<List<MovieResponse>>>(result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task MovieController_Search_Should_Return_NotFoundResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.Search(It.IsAny<SearchOption>(), It.IsAny<string>()))
                .ReturnsAsync((null, true));

            // Act
            var result = await _movieController.Search(new SearchMovieRequest());

            // Assert
            Assert.IsType<ActionResult<List<MovieResponse>>>(result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task MovieController_Search_Should_Return_OkResult()
        {
            // Arrange
            _mockMovieService.Setup(x => x.Search(It.IsAny<SearchOption>(), It.IsAny<string>()))
                .ReturnsAsync((LoadGetAllTestData(), true));

            // Act
            var result = await _movieController.Search(new SearchMovieRequest());

            // Assert
            Assert.IsType<ActionResult<List<MovieResponse>>>(result);
            var okResult = result.Result as OkObjectResult;
            var value = okResult.Value as List<MovieResponse>;
            Assert.Equal(3, value.Count);
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
