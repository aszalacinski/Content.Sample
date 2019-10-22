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
    public class AddLibraryHub
    {
        private readonly IMediator _mediator;

        public AddLibraryHub(IMediator mediator) => _mediator = mediator;

        public class AddLibraryHubCommand : IRequest<string>
        {
            public string InstructorId { get; set; }

            public AddLibraryHubCommand(string instructorId) => InstructorId = instructorId;
        }

        public class AddLibraryHubCommandHandler : IRequestHandler<AddLibraryHubCommand, string>
        {
            public readonly LibraryContext _db;
            private readonly MapperConfiguration _mapperConfiguration;

            public AddLibraryHubCommandHandler(LibraryContext db)
            {
                _db = db; 
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<LibraryDAOProfile>();
                });
            }

            public async Task<string> Handle(AddLibraryHubCommand cmd, CancellationToken cancellationToken)
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
