using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Domain.Dto.Requests;
using WebApiDemo.Domain.Dto.Responses;
using WebApiDemo.Domain.Search;
using WebApiDemo.Service.Movie;

namespace WebApiDemo.Api.Controllers.v1
{
    [Authorize]
    public class MovieController : ApiControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public MovieController(IMovieService movieService, ILogger logger, IMapper mapper)
        {
            _movieService = movieService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("/v1/[controller]s", Name = "GetAllMovies")]
        public async Task<ActionResult<List<MovieResponse>>> GetAllMovies(string sortOrder = null)
        {
            try
            {
                var movies = await _movieService.GetAllMovies(sortOrder);

                if (movies == null || !movies.Any())
                {
                    return NotFound(new ErrorResponse("No movie found"));
                }

                return Ok(_mapper.Map<List<MovieResponse>>(movies));
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieController)}->{nameof(GetAllMovies)} - error occurred on getting all movies");
                throw;
            }
        }

        [HttpPost("/v1/[controller]", Name = "AddMovie")]
        public async Task<ActionResult<MovieResponse>> AddMovie([FromBody] AddMovieRequest movie)
        {
            try
            {
                var newMovie = await _movieService.Add(_mapper.Map<Domain.Models.Movie>(movie));
                
                if (newMovie != null)
                {
                    return Ok(_mapper.Map<MovieResponse>(newMovie));
                }

                throw new Exception("No valid new movie returned");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieController)}->{nameof(AddMovie)} - error occurred on adding movie");
                throw;
            }
        }

        [HttpPut("/v1/[controller]", Name = "UpdateMovie")]
        public async Task<ActionResult<MovieResponse>> UpdateMovie([FromBody] UpdateMovieRequest movie)
        {
            try
            {
                var updatedMovie = await _movieService.Update(_mapper.Map<Domain.Models.Movie>(movie));

                if (updatedMovie != null)
                {
                    return Ok(_mapper.Map<MovieResponse>(updatedMovie));
                }

                throw new Exception("No valid updated movie returned");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieController)}->{nameof(UpdateMovie)} - error occurred on updating movie");
                throw;
            }
        }

        [HttpGet("/v1/[controller]/Search", Name = "Search")]
        public async Task<ActionResult<List<MovieResponse>>> Search([FromQuery] SearchMovieRequest searchMovieRequest)
        {
            try
            {
                var moviesTuple = await _movieService.Search(_mapper.Map<SearchOption>(searchMovieRequest), 
                    searchMovieRequest.SortOrder);

                if (!moviesTuple.IsSearchOptionValid)
                {
                    return BadRequest(new ErrorResponse("Search options are not valid"));
                }
                else
                {
                    if (moviesTuple.SearchResult == null || !moviesTuple.SearchResult.Any())
                    {
                        return NotFound(new ErrorResponse("No movie found"));
                    }

                    return Ok(_mapper.Map<List<MovieResponse>>(moviesTuple.SearchResult));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(MovieController)}->{nameof(Search)} - error occurred on seaarching movie");
                throw;
            }
        }
    }
}
