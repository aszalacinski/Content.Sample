using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.LibraryContext;

namespace HAS.Content.Feature.Library
{
    public class CreateLibraryHub
    {
        private readonly IMediator _mediator;

        public CreateLibraryHub(IMediator mediator) => _mediator = mediator;

        public class CreateLibraryHubCommand : IRequest<string>
        {
            public string InstructorId { get; set; }

            public CreateLibraryHubCommand(string instructorId) => InstructorId = instructorId;
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Tribe, TribeDAO>().ReverseMap();
                CreateMap<Model.Content, ContentDAO>().ReverseMap();
                CreateMap<Model.Library, LibraryDAO>().ReverseMap();
                CreateMap<Hub, HubDAO>()
                    .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)))
                    .ReverseMap();
            }
        }

        public class CreateLibraryHubCommandHandler : IRequestHandler<CreateLibraryHubCommand, string>
        {
            public readonly LibraryContext _db;
            private readonly IConfigurationProvider _configuration;

            public CreateLibraryHubCommandHandler(LibraryContext db, IConfigurationProvider configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<string> Handle(CreateLibraryHubCommand cmd, CancellationToken cancellationToken)
            {
                var hub = Hub.Create(string.Empty, cmd.InstructorId, DateTime.UtcNow, new List<Model.Content>(), new List<Model.Library>());

                var mapper = new Mapper(_configuration);

                var dao = mapper.Map<HubDAO>(hub);

                try
                {
                    await _db.Library.InsertOneAsync(dao);
                }
                catch(Exception)
                {
                    return string.Empty;
                }

                return dao.InstructorId.ToString();
            }
        }
    }
}
