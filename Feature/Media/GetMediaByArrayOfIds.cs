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

namespace HAS.Content.Feature.Media
{
    public class GetMediaByArrayOfIds
    {
        public class GetMediaByArrayOfIdsQuery : IRequest<IEnumerable<GetMediaByArrayOfIdsResult>>
        {
            public string[] Ids { get; private set; }

            public GetMediaByArrayOfIdsQuery(string[] ids) => Ids = ids;
        }

        public class GetMediaByArrayOfIdsResult
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

        public class GetMediaByArrayOfIdsQueryHandler : IRequestHandler<GetMediaByArrayOfIdsQuery, IEnumerable<GetMediaByArrayOfIdsResult>>
        {
            private readonly ContentContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public GetMediaByArrayOfIdsQueryHandler(ContentContext db)
            {
                _db = db;

                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ContentProfile>();
                    cfg.CreateMap<ContentDAO, GetMediaByArrayOfIdsResult>()
                        .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src))
                        .ForMember(m => m.State, opt => opt.MapFrom(src => src))
                        .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src));
                });
            }

            public async Task<IEnumerable<GetMediaByArrayOfIdsResult>> Handle(GetMediaByArrayOfIdsQuery query, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);
                
               var projection = Builders<ContentDAO>.Projection.Expression(x => mapper.Map<GetMediaByArrayOfIdsResult>(x));

                var ids = query.Ids.Select(x => ObjectId.Parse(x)).ToList();

                var filter = Builders<ContentDAO>.Filter.In(x => x.Id, ids);

                var media = await _db.Content
                                    .Find(filter)
                                    .Project(projection)
                                    .ToListAsync();


                return media;
            }
        }
    }
}
