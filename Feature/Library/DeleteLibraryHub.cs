using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Feature.EventLog;
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
using static HAS.Content.Feature.Library.GetHubById;

namespace HAS.Content.Feature.Library
{
    public class DeleteLibraryHub
    {
        public class DeleteLibraryHubCommand : IRequest<long>, ICommandEvent
        {
            public string HubId { get; set; }

            public string ProfileId { get; set; }

            public DeleteLibraryHubCommand(string hubId, string profileId)
            {
                HubId = hubId;
                ProfileId = profileId;
            }
        }

        public class DeleteLibraryHubCommandHandler : IRequestHandler<DeleteLibraryHubCommand, long>
        {
            public readonly LibraryContext _db;
            private readonly MapperConfiguration _mapperConfiguration;
            private readonly IMediator _mediator;

            public DeleteLibraryHubCommandHandler(LibraryContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<LibraryDAOProfile>();

                    cfg.CreateMap<GetHubByIdResult, HubDAO>()
                        .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                        .ForMember(m => m.Libraries, opt => opt.MapFrom(src => src.Libraries));

                });
            }

            public async Task<long> Handle(DeleteLibraryHubCommand cmd, CancellationToken cancellationToken)
            {

                var delete = await _db.Library.DeleteManyAsync(x => x.Id == ObjectId.Parse(cmd.HubId) && x.InstructorId == cmd.ProfileId);


                return delete.DeletedCount;
            }
        }
    }
}
