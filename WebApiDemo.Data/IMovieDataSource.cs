using System.Collections.Generic;

namespace WebApiDemo.Data
{
    public interface IMovieDataSource
    {
        List<MovieData> GetAllData();
        
        MovieData GetDataById(int id);
        
        int Create(MovieData movie);
        
        void Update(MovieData movie);
    }
}
