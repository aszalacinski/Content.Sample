using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.LibraryContext;

namespace HAS.Content.Feature.Library
{
    public class GetLibraryById
    {
        private readonly IMediator _mediator;

        public GetLibraryById(IMediator mediator) => _mediator = mediator;

        public class GetLibraryByIdQuery : IRequest<GetLibraryByIdResult>
        {
            public string HubId { get; set; }
            public string LibraryId { get; set; }

            public GetLibraryByIdQuery(string hubId, string libraryId)
            {
                HubId = hubId;
                LibraryId = libraryId;
            }
        }

        public class GetLibraryByIdResult
        {
            public string Id { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public DateTime CreateDate { get; private set; }
            public AccessType Access { get; private set; }
            public IEnumerable<Model.Content> Content { get; private set; }
            public Tribe DefaultTribe { get; private set; }
            public IEnumerable<Tribe> Tribes { get; private set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<TribeDAO, Tribe>();
                CreateMap<ContentDAO, Model.Content>();
                CreateMap<LibraryDAO, GetLibraryByIdResult>()
                    .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                    .ForMember(m => m.DefaultTribe, opt => opt.MapFrom(src => src.DefaultTribe))
                    .ForMember(m => m.Tribes, opt => opt.MapFrom(src => src.Tribes));
            }
        }

        public class GetHubByProfileIdQueryHandler : IRequestHandler<GetLibraryByIdQuery, GetLibraryByIdResult>
        {
            private readonly LibraryContext _db;
            private readonly IConfigurationProvider _configuration;

            public GetHubByProfileIdQueryHandler(LibraryContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<GetLibraryByIdResult> Handle(GetLibraryByIdQuery request, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_configuration);

                var projection = Builders<HubDAO>.Projection.Expression(x => mapper.Map<GetLibraryByIdResult>(x.Libraries.Where(y => y.Id == ObjectId.Parse(request.LibraryId)).FirstOrDefault()));

                var library = await _db.Library
                                    .Find(x => x.Id == ObjectId.Parse(request.HubId))
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                return library;

            }
        }
    }
}
