﻿using AutoMapper;
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
    public class DeleteLibraryFromHub
    {
        public class DeleteLibraryFromHubCommand : IRequest<bool>, ICommandEvent
        {
            public string HubId { get; set; }
            public string LibraryId { get; set; }

            public DeleteLibraryFromHubCommand(string hubId, string libraryId)
            {
                HubId = hubId;
                LibraryId = libraryId;
            }

        }

        public class DeleteLibraryFromHubCommandHandler : IRequestHandler<DeleteLibraryFromHubCommand, bool>
        {
            public readonly LibraryContext _db;
            private readonly MapperConfiguration _mapperConfiguration;
            private readonly IMediator _mediator;

            public DeleteLibraryFromHubCommandHandler(LibraryContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<LibraryDAOProfile>();

                    cfg.CreateMap<GetHubByIdResult, Hub>()
                        .ForMember(m => m.Content, opt => opt.MapFrom(src => src.Content))
                        .ForMember(m => m.Libraries, opt => opt.MapFrom(src => src.Libraries));
                });
            }

            public async Task<bool> Handle(DeleteLibraryFromHubCommand cmd, CancellationToken cancellationToken)
            {

                var result = await _mediator.Send(new GetHubByIdQuery(cmd.HubId));

                var mapper = new Mapper(_mapperConfiguration);

                Hub hub = mapper.Map<Hub>(result);

                if (hub.Handle(cmd))
                {
                    var dao = mapper.Map<HubDAO>(hub);

                    try
                    {
                        var filter = Builders<HubDAO>.Filter.Eq(x => x.Id, dao.Id);
                        var options = new FindOneAndReplaceOptions<HubDAO> { ReturnDocument = ReturnDocument.After };

                        var update = await _db.Library.FindOneAndReplaceAsync(filter, dao, options);

                        return !update.Libraries.Any(x => x.Id == ObjectId.Parse(cmd.LibraryId));

                    }
                    catch (Exception)
                    {
                        return false;
                    }

                }

                return false;

            }
        }
    }
}
