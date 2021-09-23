using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Domain.Repositories;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Service.Movie
{
    // there isn't much business logic for this demo so it just passes value to repository
    // but if there is it should happen here
    // eg. if movie release date is before 2000 then it should not be updated
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger _logger;

        public MovieService(IMovieRepository movieRepository, ILogger logger)
        {
            _movieRepository = movieRepository;
            _logger = logger;
        }

        public async Task<List<Domain.Models.Movie>> GetAllMovies(string sortOrder = null)
        {
            try
            {
                return await _movieRepository.GetAll(sortOrder);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieService)}->{nameof(GetAllMovies)} - error occurred on getting all movies");
                throw;
            }
        }

        public async Task<(List<Domain.Models.Movie> SearchResult, bool IsSearchOptionValid)> Search(SearchOption searchOption, string sortOrder = null)
        {
            try
            {
                return await _movieRepository.Search(searchOption, sortOrder);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieService)}->{nameof(Search)} - error occurred on searching movie");
                throw;
            }
        }

        public async Task<Domain.Models.Movie> Add(Domain.Models.Movie movie)
        {
            try
            {
                return await _movieRepository.Add(movie);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieService)}->{nameof(Add)} - error occurred on adding new movie");

                throw;
            }
        }

        public async Task<Domain.Models.Movie> Update(Domain.Models.Movie movie)
        {
            try
            {
                return await _movieRepository.Update(movie);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieService)}->{nameof(Update)} - error occurred on updating movie");
                throw;
            }
        }
    }
}
