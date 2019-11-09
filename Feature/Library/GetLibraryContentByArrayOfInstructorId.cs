using AutoMapper;
using HAS.Content.Data;
using HAS.Content.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Feature.Library.GetHubByProfileId;
using static HAS.Content.Feature.Library.GetLibraryByName;
using static HAS.Content.Feature.Library.GetLibraryContent;
using static HAS.Content.Feature.Media.GetMediaByArrayOfIds;

namespace HAS.Content
{
    public class GetLibraryContentByArrayOfInstructorId
    {
        public class GetLibraryContentByArrayOfInstructorIdQuery : IRequest<IEnumerable<GetMediaByArrayOfInstructorIdsResult>>
        {
            public string[] InstructorIds { get; private set; }

            public GetLibraryContentByArrayOfInstructorIdQuery(string[] ids) => InstructorIds = ids;
        }

        public class GetMediaByArrayOfInstructorIdsResult
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

        public class GetLibraryContentByArrayOfInstructorIdQueryHandler : IRequestHandler<GetLibraryContentByArrayOfInstructorIdQuery, IEnumerable<GetMediaByArrayOfInstructorIdsResult>>
        {
            private readonly IMediator _mediator;
            private readonly MapperConfiguration _mapperConfiguration;

            public GetLibraryContentByArrayOfInstructorIdQueryHandler(IMediator mediator)
            {
                _mediator = mediator;

                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<GetMediaByArrayOfIdsResult, GetMediaByArrayOfInstructorIdsResult>()
                        .ForMember(m => m.ContentDetails, opt => opt.MapFrom(src => src.ContentDetails))
                        .ForMember(m => m.State, opt => opt.MapFrom(src => src.State))
                        .ForMember(m => m.Manifest, opt => opt.MapFrom(src => src.Manifest));
                });
            }

            public async Task<IEnumerable<GetMediaByArrayOfInstructorIdsResult>> Handle(GetLibraryContentByArrayOfInstructorIdQuery query, CancellationToken cancellationToken)
            {
                var mapper = new Mapper(_mapperConfiguration);

                List<GetMediaByArrayOfInstructorIdsResult> results = new List<GetMediaByArrayOfInstructorIdsResult>();

                foreach (string id in query.InstructorIds)
                {
                    var defaultLibName = $"Default-{id}";

                    var lib = await _mediator.Send(new GetLibraryByNameQuery(id, defaultLibName));
                    var content = await _mediator.Send(new GetLibraryContentQuery(lib.HubId, lib.LibraryId));
                    var mediaIds = content.Select(x => x.Id).ToArray();

                    var iMedia = await _mediator.Send(new GetMediaByArrayOfIdsQuery(mediaIds));

                    var result = mapper.Map<IEnumerable<GetMediaByArrayOfInstructorIdsResult>>(iMedia);

                    results.AddRange(result);
                }

                return results;
            }
        }
    }
}
