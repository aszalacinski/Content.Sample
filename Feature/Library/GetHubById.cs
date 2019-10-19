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
    public class GetHubById
    {
        private readonly IMediator _mediator;

        public GetHubById(IMediator mediator) => _mediator = mediator;

        public class GetHubByIdQuery : IRequest<GetHubByIdResult>
        {
            public string HubId { get; set; }

            public GetHubByIdQuery(string hubId) => HubId = hubId;
        }

        public class GetHubByIdResult
        {
            public string Id { get; private set; }
            public string InstructorId { get; private set; }
            public DateTime CreateDate { get; private set; }
            public IEnumerable<Model.Content> Content { get; private set; }
            public IEnumerable<Model.Library> Libraries { get; private set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<TribeDAO, Tribe>();
                CreateMap<ContentDAO, Model.Content>();
                CreateMap<LibraryDAO, Model.Library>()
                    .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                    .ForMember(m => m.DefaultTribe, opt => opt.MapFrom(src => src.DefaultTribe))
                    .ForMember(m => m.Tribes, opt => opt.MapFrom(src => src.Tribes));
                CreateMap<HubDAO, GetHubByIdResult>()
                    .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                    .ForMember(m => m.Libraries, opt => opt.MapFrom(src => src.Libraries));
            }
        }

        public class GetHubByProfileIdQueryHandler : IRequestHandler<GetHubByIdQuery, GetHubByIdResult>
        {
            private readonly LibraryContext _db;
            private readonly IConfigurationProvider _configuration;

            public GetHubByProfileIdQueryHandler(LibraryContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<GetHubByIdResult> Handle(GetHubByIdQuery request, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_configuration);

                var projection = Builders<HubDAO>.Projection.Expression(x => mapper.Map<GetHubByIdResult>(x));

                var hub = await _db.Library
                                    .Find(x => x.Id == ObjectId.Parse(request.HubId))
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                return hub;

            }
        }
    }
}
