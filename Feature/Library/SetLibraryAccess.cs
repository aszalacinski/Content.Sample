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
using static HAS.Content.Feature.Library.GetHubById;

namespace HAS.Content.Feature.Library
{
    public class SetLibraryAccess
    {
        private readonly IMediator _mediator;

        public SetLibraryAccess(IMediator mediator) => _mediator = mediator;

        public class SetLibraryAccessCommand : IRequest<string>
        {
            public string HubId { get; set; }
            public string LibraryId { get; set; }
            public string Access { get; set; }

            public SetLibraryAccessCommand(string hubId, string libraryId, string access)
            {
                HubId = hubId;
                LibraryId = libraryId;
                Access = access;
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<GetHubByIdResult, Hub>()
                        .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                        .ForMember(m => m.Libraries, opt => opt.MapFrom(src => src.Libraries));

                CreateMap<Tribe, TribeDAO>();
                CreateMap<Model.Content, ContentDAO>();
                CreateMap<Model.Library, LibraryDAO>()
                    .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
                CreateMap<Hub, HubDAO>()
                    .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)))
                    .ReverseMap();
            }
        }

        public class SetLibraryAccessCommandHandler : IRequestHandler<SetLibraryAccessCommand, string>
        {
            public readonly LibraryContext _db;
            private readonly IConfigurationProvider _configuration;
            private readonly IMediator _mediator;

            public SetLibraryAccessCommandHandler(LibraryContext db, IConfigurationProvider configuration, IMediator mediator)
            {
                _db = db;
                _configuration = configuration;
                _mediator = mediator;
            }

            public async Task<string> Handle(SetLibraryAccessCommand cmd, CancellationToken cancellationToken)
            {
                var result = await _mediator.Send(new GetHubByIdQuery(cmd.HubId));

                var mapper = new Mapper(_configuration);

                Hub hub = mapper.Map<Hub>(result);

                if (hub.Handle(cmd))
                {
                    var dao = mapper.Map<HubDAO>(hub);

                    try
                    {
                        var filter = Builders<HubDAO>.Filter.Eq(x => x.Id, dao.Id);
                        var options = new FindOneAndReplaceOptions<HubDAO> { ReturnDocument = ReturnDocument.After };

                        var update = await _db.Library.FindOneAndReplaceAsync(filter, dao, options);

                        return update.Libraries.Where(x => x.Id == ObjectId.Parse(cmd.LibraryId)).FirstOrDefault().Id.ToString();

                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }

                }

                return string.Empty;
            }
        }
    }
}
