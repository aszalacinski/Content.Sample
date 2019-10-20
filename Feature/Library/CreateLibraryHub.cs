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

        public class CreateLibraryHubCommandHandler : IRequestHandler<CreateLibraryHubCommand, string>
        {
            public readonly LibraryContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public CreateLibraryHubCommandHandler(LibraryContext db)
            {
                _db = db; 
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<LibraryDAOProfile>();
                });
            }

            public async Task<string> Handle(CreateLibraryHubCommand cmd, CancellationToken cancellationToken)
            {
                var hub = Hub.Create(string.Empty, cmd.InstructorId, DateTime.UtcNow, new List<Model.Content>(), new List<Model.Library>());

                var mapper = new Mapper(_mapperConfiguration);

                var dao = mapper.Map<HubDAO>(hub);

                try
                {
                    await _db.Library.InsertOneAsync(dao);
                }
                catch(Exception)
                {
                    return string.Empty;
                }

                return dao.Id.ToString();
            }
        }
    }
}
