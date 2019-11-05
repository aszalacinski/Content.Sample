using HAS.Content.Data;
using MediatR;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.LibraryContext;

namespace HAS.Content.Feature.Library
{
    public class GetLibraryByName
    {
        public GetLibraryByName() { }

        public class GetLibraryByNameQuery : IRequest<GetLibraryByNameResult>
        {
            public string ProfileId { get; set; }
            public string LibraryName { get; set; }

            public GetLibraryByNameQuery(string profileId, string libraryName)
            {
                ProfileId = profileId;
                LibraryName = libraryName;
            }
        }

        public class GetLibraryByNameResult
        {
            public string HubId { get; set; }
            public string LibraryId { get; set; }
        }

        public class GetLibraryByNameQueryHandler : IRequestHandler<GetLibraryByNameQuery, GetLibraryByNameResult>
        {
            private readonly LibraryContext _db;
            public GetLibraryByNameQueryHandler(LibraryContext db)
            {
                _db = db;
            }

            public async Task<GetLibraryByNameResult> Handle(GetLibraryByNameQuery request, CancellationToken cancellationToken)
            {
                var projection = Builders<HubDAO>.Projection.Expression(x => new GetLibraryByNameResult { HubId = x.Id.ToString(), LibraryId = x.Libraries.FirstOrDefault(x => x.Name.Contains(request.LibraryName)).Id.ToString() });

                var library = await _db.Library
                                    .Find(x => x.InstructorId == request.ProfileId)
                                    .Project(projection)
                                    .FirstOrDefaultAsync();

                return library;

            }
        }
    }
}
