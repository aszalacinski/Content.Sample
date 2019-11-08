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
using static HAS.Content.Data.ContentContext;
using static HAS.Content.Feature.Media.GetMediaCloudUrlByArrayOfFilenames;

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
            public string FileType { get; set; }
            public string FileExtension { get; set; }
            public ContentDetails ContentDetails { get; set; }
            public DateTime RecordingDate { get; set; }
            public DateTime UploadDate { get; set; }
            public StateDetails State { get; set; }
            public Model.Manifest Manifest { get; set; }
            public Uri Uri { get; set; }
        }

        public class FindByProfileIdQueryHandler : IRequestHandler<FindByProfileIdQuery, IEnumerable<FindByProfileIdResult>>
        {
            private readonly ContentContext _db;
            private readonly IMediator _mediator;
            private readonly MapperConfiguration _mapperConfiguration;

            public FindByProfileIdQueryHandler(ContentContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {

                    cfg.AddProfile<ContentProfile>();
                    cfg.CreateMap<ContentDAO, FindByProfileIdResult>()
                            .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src))
                            .ForMember(m => m.State, opt => opt.MapFrom(src => src))
                            .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src));
                });
            }

            public async Task<IEnumerable<FindByProfileIdResult>> Handle(FindByProfileIdQuery query, CancellationToken cancellationToken)
            {

                var mapper = new Mapper(_mapperConfiguration);

                var projection = Builders<ContentDAO>.Projection.Expression(x => mapper.Map<FindByProfileIdResult>(x));

                var results = await _db.Content
                                        .Find(x => x.InstructorId == query.ProfileId)
                                        .Project(projection)
                                        .ToListAsync();

                if(results.Count == 0)
                {
                    return new List<FindByProfileIdResult>();
                }
                else
                {
                    var firstMedia = results[0];


                    string containerRef = $"{firstMedia.InstructorName[0]}{firstMedia.InstructorName.Split(' ')[1]}-{firstMedia.InstructorId}".ToLower();

                    //create array of filenames
                    string[] fileNames = results.Select(x => x.FileName).ToArray();

                    var uris = await _mediator.Send(new GetMediaCloudUrlByArrayOfFilenamesQuery(containerRef, firstMedia.FileType, fileNames, firstMedia.FileExtension));

                    foreach(var result in results)
                    {
                        var uri = uris[result.FileName];
                        result.Uri = uri;
                    }

                    return results;
                }
            }
        }
    }
}
