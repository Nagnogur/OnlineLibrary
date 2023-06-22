using AutoMapper;
using Gateway.Entities;
using Gateway.Models;

namespace Gateway.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<Identifier, IdentifierModel>().ReverseMap();
            CreateMap<Author, AuthorModel>().ReverseMap();
            CreateMap<LinkPrice, LinkModel>().ReverseMap();
            CreateMap<Review, ReviewModel>().ReverseMap();
            CreateMap<Book, BookModel>().ReverseMap();
            // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
        }
    }
}
