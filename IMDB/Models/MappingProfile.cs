using AutoMapper;
using IMDB.Core.Entities;
using IMDB.ViewModels;

namespace IMDB.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Actor, ActorViewModel>()
                .ReverseMap();
        }
    }
}
