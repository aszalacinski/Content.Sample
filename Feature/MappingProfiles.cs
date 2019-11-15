using AutoMapper;
using HAS.Content.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HAS.Content.Data.ContentContext;
using static HAS.Content.Data.LibraryContext;
using static HAS.Content.Feature.Library.GetHubById;
using static HAS.Content.Feature.Library.GetHubByProfileId;
using static HAS.Content.Feature.Library.GetLibraryById;
using static HAS.Content.Feature.Media.FindById;
using static HAS.Content.Feature.Media.FindByProfileId;

namespace HAS.Content
{
    public class LibraryDAOProfile : Profile
    {
        public LibraryDAOProfile()
        {
            CreateMap<Tribe, TribeDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
            CreateMap<Model.Content, Data.LibraryContext.ContentDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
            CreateMap<Model.Library, LibraryDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
            CreateMap<Hub, HubDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
        }
    }

    public class LibraryProfile : Profile
    {
        public LibraryProfile()
        {
            CreateMap<TribeDAO, Tribe>();
            CreateMap<Data.LibraryContext.ContentDAO, Model.Content>();
            CreateMap<LibraryDAO, Model.Library>()
                .ForMember(m => m.Access, opt => opt.MapFrom(src => Enum.Parse(typeof(AccessType), src.Access)))
                .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(m => m.DefaultTribe, opt => opt.MapFrom(src => src.DefaultTribe))
                .ForMember(m => m.Tribes, opt => opt.MapFrom(src => src.Tribes));
            CreateMap<HubDAO, Hub>()
                .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(m => m.Libraries, opt => opt.MapFrom(src => src.Libraries));
        }
    }

    public class ContentProfile : Profile
    {
        public ContentProfile()
        {
            CreateMap<Data.ContentContext.ContentDAO, ContentDetails>();
            CreateMap<Data.ContentContext.ContentDAO, StateDetails>();
            CreateMap<Data.ContentContext.ContentDAO, Manifest>();
        }
    }

    public class ContentDAOProfile : Profile
    {
        public ContentDAOProfile()
        {
            CreateMap<Manifest, ManifestDAO>();
            CreateMap<Model.Media, Data.ContentContext.ContentDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)))
                .ForMember(m => m.Title, opt => opt.MapFrom(src => src.ContentDetails.Title))
                .ForMember(m => m.Description, opt => opt.MapFrom(src => src.ContentDetails.Description))
                .ForMember(m => m.Duration, opt => opt.MapFrom(src => src.ContentDetails.Duration))
                .ForMember(m => m.Size, opt => opt.MapFrom(src => src.ContentDetails.Size))
                .ForMember(m => m.LikeScore, opt => opt.MapFrom(src => src.ContentDetails.LikeScore))
                .ForMember(m => m.Tags, opt => opt.MapFrom(src => src.ContentDetails.Tags))
                .ForMember(m => m.Status, opt => opt.MapFrom(src => src.State.Status))
                .ForMember(m => m.Published, opt => opt.MapFrom(src => src.State.Published))
                .ForMember(m => m.Staged, opt => opt.MapFrom(src => src.State.Staged))
                .ForMember(m => m.Archived, opt => opt.MapFrom(src => src.State.Archived));
        }
    }


}
