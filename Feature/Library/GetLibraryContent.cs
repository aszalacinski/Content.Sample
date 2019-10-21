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
    public class GetLibraryContent
    {
        public GetLibraryContent() { }

        public class GetLibraryContentQuery : IRequest<IEnumerable<Model.Content>>
        {
            public string HubId { get; set; }
            public string LibraryId { get; set; }

            public GetLibraryContentQuery(string hubId, string libraryId)
            {
                HubId = hubId;
                LibraryId = libraryId;
            }
        }
        
        public class GetLibraryContentQueryHandler : IRequestHandler<GetLibraryContentQuery, IEnumerable<Model.Content>>
        {
            private readonly LibraryContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public GetLibraryContentQueryHandler(LibraryContext db)
            {
                _db = db;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<LibraryProfile>();
                });
            }

            public async Task<IEnumerable<Model.Content>> Handle(GetLibraryContentQuery request, CancellationToken cancellationToken)
            {

                var mapper = new Mapper(_mapperConfiguration);

                var projection = Builders<HubDAO>.Projection.Expression(x => x.Libraries.Where(y => y.Id == ObjectId.Parse(request.LibraryId)).FirstOrDefault().Content);

                var content = await _db.Library
                                    .Find(x => x.Id == ObjectId.Parse(request.HubId))
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                var mapped = mapper.Map<IEnumerable<Model.Content>>(content);

                return mapped;

            }
        }
    }
}
