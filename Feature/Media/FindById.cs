using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HAS.Content.Feature.Media
{
    public class FindById
    {
        private readonly IMediator _mediator;

        public FindById(IMediator mediator) => _mediator = mediator;

        public class Query : IRequest<Result> 
        { 
            public Query(string id)
            {
                Id = id;
            }

            public string Id { get; set; }
        }

        public class Result 
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
                CreateMap<ContentDAO, Result>()
                    .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src))
                    .ForMember(m => m.State, opt => opt.MapFrom(src => src))
                    .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src));
            }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly ContentContext _db;
            private readonly IConfigurationProvider _configuration;

            public QueryHandler(ContentContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_configuration);

                var projection = Builders<Data.ContentDAO>.Projection.Expression(x => mapper.Map<Result>(x));

                var media = await _db.Content
                                    .Find(x => x.Id == ObjectId.Parse(request.Id.ToString()))
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                return media;

            }
        }
    }
}
