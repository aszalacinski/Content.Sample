using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.ContentContext;

namespace HAS.Content.Feature.Media
{
    public class FindById
    {
        private readonly IMediator _mediator;

        public FindById(IMediator mediator) => _mediator = mediator;

        public class FindByIdQuery : IRequest<FindByIdResult> 
        { 
            public FindByIdQuery(string id)
            {
                Id = id;
            }

            public string Id { get; set; }
        }

        public class FindByIdResult 
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
            public Manifest Manifest { get; private set; }
        }

        public class FindByIdQueryHandler : IRequestHandler<FindByIdQuery, FindByIdResult>
        {
            private readonly ContentContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public FindByIdQueryHandler(ContentContext db)
            {
                _db = db;

                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ContentProfile>();
                    cfg.CreateMap<ContentDAO, FindByIdResult>()
                        .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src))
                        .ForMember(m => m.State, opt => opt.MapFrom(src => src))
                        .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src));
                });
            }

            public async Task<FindByIdResult> Handle(FindByIdQuery request, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                var projection = Builders<ContentDAO>.Projection.Expression(x => mapper.Map<FindByIdResult>(x));

                var media = await _db.Content
                                    .Find(x => x.Id == ObjectId.Parse(request.Id.ToString()))
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                return media;

            }
        }
    }
}
