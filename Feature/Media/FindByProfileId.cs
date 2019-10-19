using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.ContentContext;

namespace HAS.Content.Feature.Media
{
    public class FindByProfileId
    {
        private readonly IMediator _mediator;

        public FindByProfileId(IMediator mediator) => _mediator = mediator;

        public class FindByProfileIdQuery : IRequest<IEnumerable<FindByProfileIdResult>>
        {
            public FindByProfileIdQuery(string profileId)
            {
                ProfileId = profileId;
            }

            public string ProfileId { get; set; }
        }

        public class FindByProfileIdResult
        {
            public string Id { get; set; }
            public string InstructorId { get; set; }
            public string InstructorName { get; set; }
            public string FileName { get; set; }
            public string FileType { get; private set; }
            public string FileExtension { get; private set; }
            public ContentDetails ContentDetails { get; private set; }
            public DateTime RecordingDate { get; private set; }
            public DateTime UploadDate { get; private set; }
            public StateDetails State { get; private set; }
            public Model.Manifest Manifest { get; private set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<ContentDAO, ContentDetails>();
                CreateMap<ContentDAO, StateDetails>();
                CreateMap<ContentDAO, Manifest>();
                CreateMap<ContentDAO, FindByProfileIdResult>()
                    .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src))
                    .ForMember(m => m.State, opt => opt.MapFrom(src => src))
                    .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src));
            }
        }

        public class FindByProfileIdQueryHandler : IRequestHandler<FindByProfileIdQuery, IEnumerable<FindByProfileIdResult>>
        {
            private readonly ContentContext _db;
            private readonly IConfigurationProvider _configuration;

            public FindByProfileIdQueryHandler(ContentContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<IEnumerable<FindByProfileIdResult>> Handle(FindByProfileIdQuery request, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_configuration);

                var projection = Builders<ContentDAO>.Projection.Expression(x => mapper.Map<FindByProfileIdResult>(x));

                var results = await _db.Content
                                        .Find(x => x.InstructorId == request.ProfileId)
                                        .Project(projection)
                                        .ToListAsync();

                if(results.Count == 0)
                {
                    return new List<FindByProfileIdResult>();
                }

                return results;
            }
        }
    }
}
