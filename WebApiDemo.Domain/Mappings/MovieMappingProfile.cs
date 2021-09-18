using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using WebApiDemo.Data;
using WebApiDemo.Domain.Dto.Requests;
using WebApiDemo.Domain.Dto.Responses;
using WebApiDemo.Domain.Search;

namespace WebApiDemo.Domain.Mappings
{
    [ExcludeFromCodeCoverage]
    public class MovieMappingProfile : Profile
    {
        public MovieMappingProfile()
        {
            CreateMap<MovieData, Models.Movie>().ReverseMap();
            
            CreateMap<AddMovieRequest, Models.Movie>().ReverseMap();
            
            CreateMap<UpdateMovieRequest, Models.Movie>().ReverseMap();
            
            CreateMap<MovieResponse, Models.Movie>().ReverseMap();

            CreateMap<SearchMovieRequest, SearchOption>();
        }
    }
}
