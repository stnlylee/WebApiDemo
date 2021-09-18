using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Service.Movie
{
    public interface IMovieService
    {
        Task<List<Domain.Models.Movie>> GetAllMovies(string sortOrder = null);

        Task<(List<Domain.Models.Movie> SearchResult, bool IsSearchOptionValid)> Search(SearchOption searchOption, string sortOrder = null);

        Task<Domain.Models.Movie> Add(Domain.Models.Movie movie);

        Task<Domain.Models.Movie> Update(Domain.Models.Movie movie);
    }
}
