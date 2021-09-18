using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Data;
using WebApiDemo.Domain.Cache;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Domain.Repositories
{
    public class MovieRepository : RepositoryBase<Models.Movie, int>, IMovieRepository
    {
        private readonly IMovieDataSource _movieDataSource;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDistributedCacheProvider _distributedCacheProvider;
        private readonly IConfiguration _config;
        private const string GetAllDataKey = "get-all-data";
        private const int DefaultCacheExpirationSeconds = 30;

        public MovieRepository(IMovieDataSource movieDataSource, 
            IMapper mapper, ILogger logger,
            IDistributedCacheProvider distributedCacheProvider,
            IConfiguration config)
        {
            _movieDataSource = movieDataSource;
            _mapper = mapper;
            _logger = logger;
            _distributedCacheProvider = distributedCacheProvider;
            _config = config;
        }
        
        public async override Task<List<Models.Movie>> GetAll(string sortOrder = null)
        {
            try
            {
                var mappedList = _mapper.Map<List<Models.Movie>>(TryGetAllFromCache());
                return GetSortedList(mappedList, sortOrder);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, 
                    $"{nameof(MovieRepository)}->{nameof(GetAll)} - error occurred on getting all movies");
                throw;
            }
        }

        public async override Task<Models.Movie> GetByKey(int key)
        {
            try
            {
                if (key <= 0)
                {
                    string error = $"{nameof(MovieRepository)}->{nameof(GetByKey)} - movie key must be greater than 0";
                    _logger.Error(error);
                    throw new ArgumentException(error);
                }

                return _mapper.Map<Models.Movie>(_movieDataSource.GetDataById(key));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, 
                    $"{nameof(MovieRepository)}->{nameof(GetByKey)} - error occurred on getting movie for key {key}");
                throw;
            }
        }

        public async override Task<(List<Models.Movie> SearchResult, bool IsSearchOptionValid)> Search(SearchOption searchOption, string sortOrder = null)
        {
            try
            {
                if (!searchOption.Valid)
                {
                    _logger.Warning($"{nameof(MovieRepository)}->{nameof(Search)} - invalid search option");
                    return (null, false);
                }

                // start filtering
                // basic idea is if conditions are there just do the fitering
                // otherwise pass the same list to next one
                // so it will be filtered one after another
                var searchResults = new List<Models.Movie>();

                searchResults = _mapper.Map<List<Models.Movie>>(TryGetAllFromCache());

                searchResults = !string.IsNullOrEmpty(searchOption.Title) ? searchResults.Where(x =>
                        x.Title.Contains(searchOption.Title, StringComparison.InvariantCultureIgnoreCase)).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.Genre) ? searchResults.Where(x =>
                        x.Genre.Contains(searchOption.Genre, StringComparison.InvariantCultureIgnoreCase)).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.Classification) ? searchResults.Where(x =>
                        x.Classification.Contains(searchOption.Classification, StringComparison.InvariantCultureIgnoreCase)).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.MinReleaseDate) && searchOption.MinReleaseDateInt > 0 ? searchResults.Where(x =>
                        x.ReleaseDate >= searchOption.MinReleaseDateInt).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.MaxReleaseDate) && searchOption.MaxReleaseDateInt > 0 ? searchResults.Where(x =>
                            x.ReleaseDate <= searchOption.MaxReleaseDateInt).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.MinRating) && searchOption.MinRatingInt > 0 ? searchResults.Where(x =>
                            x.Rating >= searchOption.MinRatingInt).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.MaxRating) && searchOption.MaxRatingInt > 0 ? searchResults.Where(x =>
                            x.Rating <= searchOption.MaxRatingInt).ToList() : searchResults;
                
                searchResults = !string.IsNullOrEmpty(searchOption.Cast) ? searchResults.Where(x =>
                        x.Cast.Contains(searchOption.Cast)).ToList() : searchResults;

                searchResults = !string.IsNullOrEmpty(searchOption.MovieId) && searchOption.MovieIdInt > 0 ? searchResults.Where(x =>
                        x.MovieId == searchOption.MovieIdInt).ToList() : searchResults;

                // sort
                searchResults = GetSortedList(searchResults, sortOrder);

                return searchResults.Any() ? (searchResults, true) : (null, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieRepository)}->{nameof(Search)} - error occurred on searching movie");
                throw;
            }
        }

        public async override Task<Models.Movie> Add(Models.Movie movie)
        {
            try
            {
                if (movie == null)
                {
                    string error = $"{nameof(MovieRepository)}->{nameof(Add)} - movie must not be null";
                    _logger.Error(error);
                    throw new ArgumentException(error);
                }

                int newKey = _movieDataSource.Create(_mapper.Map<MovieData>(movie));
                _logger.Information($"{nameof(MovieRepository)}->{nameof(Add)} - new movie ({newKey}, {movie.Title}) is added");
                return await GetByKey(newKey);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, 
                    $"{nameof(MovieRepository)}->{nameof(Add)} - error occurred on adding new movie");
                throw;
            }
        }

        public async override Task<Models.Movie> Update(Models.Movie movie)
        {
            try
            {
                if (movie == null || movie.MovieId <= 0)
                {
                    string error = $"{nameof(MovieRepository)}->{nameof(Update)} - movie must not be null and movie id must be greater than 0";
                    _logger.Error(error);
                    throw new ArgumentException(error);
                }

                _movieDataSource.Update(_mapper.Map<MovieData>(movie));
                _logger.Information($"{nameof(MovieRepository)}->{nameof(Update)} - movie ({movie.MovieId}, {movie.Title}) is updated");
                return await GetByKey(movie.MovieId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, 
                    $"{nameof(MovieRepository)}->{nameof(Update)} - error occurred on updating movie");
                throw;
            }
        }

        private List<Models.Movie> GetSortedList(List<Models.Movie> list, string sortOrder)
        {
            if (list == null)
            {
                return list;
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                return list.OrderBy(x => x.MovieId).ToList();
            }

            switch (sortOrder.ToLower())
            {
                case "movieid":
                    return list.OrderBy(x => x.MovieId).ToList();
                case "title":
                    return list.OrderBy(x => x.Title).ToList();
                case "genre":
                    return list.OrderBy(x => x.Genre).ToList();
                case "classification":
                    return list.OrderBy(x => x.Classification).ToList();
                case "releasedate":
                    return list.OrderBy(x => x.ReleaseDate).ToList();
                case "rating":
                    return list.OrderBy(x => x.Rating).ToList();
                default:
                    return list.OrderBy(x => x.MovieId).ToList();
            }
        }

        private List<MovieData> TryGetAllFromCache()
        {
            List<MovieData> list = null;

            list = _distributedCacheProvider.GetFromCache<List<MovieData>>(GetAllDataKey).Result;

            if (list == null)
            {
                list = _movieDataSource.GetAllData();
                var options = new DistributedCacheEntryOptions();
                options.SetSlidingExpiration(new TimeSpan(0, 0, GetCacheExpirationSeconds()));
                _distributedCacheProvider.SetCache<List<MovieData>>(GetAllDataKey, list, options);
            }

            return list;
        }

        private int GetCacheExpirationSeconds()
        {
            try
            {
                var section = _config.GetSection("Cache");
                var expire = int.Parse(
                    section.GetValue(typeof(int), "CacheExpirationSeconds")
                    .ToString());
                
                return expire;
            }
            catch (Exception)
            {
                return DefaultCacheExpirationSeconds;
            }
        }
    }
}
