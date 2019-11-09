using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Feature.EventLog;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Data.ContentContext;
using static HAS.Content.Feature.Media.FindById;

namespace HAS.Content.Feature.Media
{
    public class UpdateMedia
    {
        public class UpdateMediaCommand : IRequest<UpdateMediaResult>, ICommandEvent
        {
            public string MediaId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public long Duration { get; set; }
        }

        public class UpdateMediaResult
        {
            public string MediaId { get; set; }
            public string ProfileId { get; set; }
        }

        public class UpdateMediaCommandHandler : IRequestHandler<UpdateMediaCommand, UpdateMediaResult>
        {
            public readonly ContentContext _db;
            private readonly MapperConfiguration _mapperConfiguration;
            private readonly IMediator _mediator;

            public UpdateMediaCommandHandler(ContentContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ContentDAOProfile>();
                    cfg.CreateMap<FindByIdResult, Model.Media>()
                        .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src.ContentDetails))
                        .ForMember(m => m.State, opt => opt.MapFrom(src => src.State))
                        .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src.Manifest));                        
                });
            }

            public async Task<UpdateMediaResult> Handle(UpdateMediaCommand cmd, CancellationToken cancellationToken)
            {
                var result = await _mediator.Send(new FindByIdQuery(cmd.MediaId));

                var mapper = new Mapper(_mapperConfiguration);

                Model.Media media = mapper.Map<Model.Media>(result);

                if(media.Handle(cmd))
                {
                    var dao = mapper.Map<ContentDAO>(media);

                    try
                    {
                        var filter = Builders<ContentDAO>.Filter.Eq(x => x.Id, dao.Id);
                        var options = new FindOneAndReplaceOptions<ContentDAO> { ReturnDocument = ReturnDocument.After };

                        var update = await _db.Content.FindOneAndReplaceAsync(filter, dao, options);

                        return new UpdateMediaResult
                        {
                            MediaId = dao.Id.ToString(),
                            ProfileId = dao.InstructorId
                        };
                    }
                    catch(Exception)
                    {
                        return null;
                    }

                }

                return null;
            }
        }
    }
}
