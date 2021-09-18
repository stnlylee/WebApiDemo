using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;

namespace WebApiDemo.Data
{
    [ExcludeFromCodeCoverage]
    public class MovieDataSource : IMovieDataSource
    {
        private static int _pk;
        private static DataSet _dsMovies = new DataSet();

        private static DataSet Movies
        {
            get
            {
                lock (_dsMovies)
                {
                    if (_dsMovies.Tables.Count > 0)
                        return _dsMovies;
                    var streamReader = new StreamReader(Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("WebApiDemo.Data.MoviesDataSource.xml"));
                    _dsMovies = new DataSet();
                    var num = (int) _dsMovies.ReadXml(streamReader, XmlReadMode.ReadSchema);
                    if (_dsMovies.Tables["Movie"].PrimaryKey == null ||
                        _dsMovies.Tables["Movie"].PrimaryKey.Length == 0)
                        _dsMovies.Tables["Movie"].PrimaryKey = new DataColumn[1]
                        {
                            _dsMovies.Tables["Movie"].Columns["Id"]
                        };
                    var dataView = new DataView(_dsMovies.Tables["Movie"], null, "Id DESC",
                        DataViewRowState.CurrentRows);
                    _pk = dataView.Count <= 0 ? 0 : Convert.ToInt32(dataView[0]["Id"]);
                    return _dsMovies;
                }
            }
        }

        public virtual List<MovieData> GetAllData()
        {
            var movieDataList = new List<MovieData>();
            if (Movies != null)
            {
                foreach (DataRow row in (InternalDataCollectionBase) Movies.Tables["Movie"].Rows)
                    movieDataList.Add(GetMovieData(row));
                Thread.Sleep(2000);
            }

            return movieDataList;
        }

        public virtual MovieData GetDataById(int id)
        {
            if (Movies != null)
            {
                Thread.Sleep(100);
                var dataRowArray = Movies.Tables["Movie"].Select("Id = " + id);
                if (dataRowArray.Length > 0)
                    return GetMovieData(dataRowArray[0]);
            }

            return null;
        }

        public virtual int Create(MovieData movie)
        {
            if (Movies == null)
                throw new Exception("Movies datasource is not available");
            lock (_dsMovies)
            {
                var row = _dsMovies.Tables["Movie"].NewRow();
                var num = ++_pk;
                movie.MovieId = num;
                row["Id"] = num;
                row["Title"] = !string.IsNullOrEmpty(movie.Title)
                    ? (object) movie.Title.Trim()
                    : throw new Exception("Movie Title is mandatory");
                if (!string.IsNullOrEmpty(movie.Genre))
                    row["Genre"] = movie.Genre.Trim();
                if (!string.IsNullOrEmpty(movie.Classification))
                    row["Classification"] = movie.Classification;
                row["Rating"] = movie.Rating;
                row["ReleaseDate"] = movie.ReleaseDate;
                _dsMovies.Tables["Movie"].Rows.Add(row);
                if (movie.Cast != null && movie.Cast.Length > 0)
                    AddCast(movie);
                _dsMovies.AcceptChanges();
                return num;
            }
        }

        public virtual void Update(MovieData movie)
        {
            if (Movies == null)
                throw new Exception("Movies datasource is not available");
            lock (_dsMovies)
            {
                var dataRowArray = _dsMovies.Tables["Movie"].Select("Id = " + movie.MovieId);
                if (dataRowArray.Length == 0)
                    throw new Exception("The movie ID " + movie.MovieId + " does not exist");
                dataRowArray[0]["Title"] = !string.IsNullOrEmpty(movie.Title)
                    ? (object) movie.Title
                    : throw new Exception("Movie Title is mandatory");
                dataRowArray[0]["Classification"] = string.IsNullOrEmpty(movie.Classification)
                    ? DBNull.Value
                    : (object) movie.Classification.Trim();
                dataRowArray[0]["Genre"] =
                    string.IsNullOrEmpty(movie.Genre) ? DBNull.Value : (object) movie.Genre.Trim();
                dataRowArray[0]["Rating"] = movie.Rating;
                dataRowArray[0]["ReleaseDate"] = movie.ReleaseDate;
                foreach (var dataRow in _dsMovies.Tables["Cast"].Select("MovieId = " + movie.MovieId))
                    dataRow.Delete();
                if (movie.Cast == null || movie.Cast.Length <= 0)
                    return;
                AddCast(movie);
            }
        }

        private void AddCast(MovieData movie)
        {
            for (var index = 0; index < movie.Cast.Length; ++index)
                if (movie.Cast[index].Trim().Length > 0)
                {
                    var row = _dsMovies.Tables["Cast"].NewRow();
                    row["MovieId"] = movie.MovieId;
                    row["ActorName"] = movie.Cast[index].Trim();
                    _dsMovies.Tables["Cast"].Rows.Add(row);
                }
        }

        private MovieData GetMovieData(DataRow r)
        {
            var movieData = new MovieData();
            movieData.MovieId = Convert.ToInt32(r["Id"]);
            movieData.Title = r["Title"].ToString();
            movieData.ReleaseDate = Convert.ToInt32(r["ReleaseDate"]);
            movieData.Rating = Convert.ToInt32(r["Rating"]);
            movieData.Genre = r["Genre"].ToString();
            movieData.Classification = r["Classification"].ToString();
            var dataRowArray = Movies.Tables["Cast"].Select("MovieId = " + movieData.MovieId);
            movieData.Cast = new string[dataRowArray.Length];
            for (var index = 0; index < dataRowArray.Length; ++index)
                movieData.Cast[index] = dataRowArray[index]["ActorName"].ToString();
            return movieData;
        }
    }
}