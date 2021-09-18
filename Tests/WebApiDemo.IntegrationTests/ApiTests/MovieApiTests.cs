using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApiDemo.Domain.Dto.Requests;
using WebApiDemo.Domain.Dto.Responses;
using Xunit;
using Xunit.Priority;

namespace WebApiDemo.IntegrationTests.ApiTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class MovieApiTests :IClassFixture<CustomWebApplicationFactory<Api.Startup>>
    {
        private readonly HttpClient _client;
        private const string GetAllMovieUrl = "/v1/Movies";
        private const string UpsertMovieUrl = "/v1/Movie";
        private const string SearchMovieUrl = "/v1/Movie/Search?Title=man";

        public MovieApiTests(CustomWebApplicationFactory<Api.Startup> fixture)
        {
            _client = fixture.CreateClient();

            var byteArray = Encoding.ASCII.GetBytes("user:123");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(byteArray));
        }

        [Fact, Priority(1)]
        public async Task MovieApi_Should_Add_Movie()
        {
            string json = JsonConvert.SerializeObject(new AddMovieRequest
            {
                Title = "new movie",
                Genre = "new genre",
                Classification = "m",
                Rating = 5,
                ReleaseDate = 2000
            });

            StringContent httpContent = new StringContent(json, Encoding.Default, "application/json");
            var response  = await _client.PostAsync(UpsertMovieUrl, httpContent);
            var movieJson = await response.Content.ReadAsStringAsync();
            var  movie = JsonConvert.DeserializeObject<MovieResponse>(movieJson);
            Assert.Equal(81, movie.MovieId);
            Assert.Equal("new movie", movie.Title);
        }

        [Fact, Priority(2)]
        public async Task MovieApi_Should_Update_Movie()
        {
            string json = JsonConvert.SerializeObject(new UpdateMovieRequest
            {
                MovieId = 1,
                Title = "movie updated",
                Genre = "genre",
                Classification = "m",
                Rating = 5,
                ReleaseDate = 2000
            });

            StringContent httpContent = new StringContent(json, Encoding.Default, "application/json");
            var response = await _client.PutAsync(UpsertMovieUrl, httpContent);
            var movieJson = await response.Content.ReadAsStringAsync();
            var movie = JsonConvert.DeserializeObject<MovieResponse>(movieJson);
            Assert.Equal(1, movie.MovieId);
            Assert.Equal("movie updated", movie.Title);
        }

        [Fact, Priority(3)]
        public async Task MovieApi_Should_Get_All_Movies()
        {
            var response = await _client.GetAsync(GetAllMovieUrl);

            var moviesJson = await response.Content.ReadAsStringAsync();

            var movies = JsonConvert.DeserializeObject<List<MovieResponse>>(moviesJson);

            Assert.Equal(81, movies.Count);
        }

        [Fact, Priority(4)]
        public async Task MovieApi_Should_Search_Movie()
        {
            var response = await _client.GetAsync(SearchMovieUrl);

            var moviesJson = await response.Content.ReadAsStringAsync();

            var movies = JsonConvert.DeserializeObject<List<MovieResponse>>(moviesJson);

            Assert.Equal(4, movies.Count);
        }
    }
}
