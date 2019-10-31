using HAS.Content.ApplicationServices.Messaging;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Content.Feature.Media.UploadAudio;

namespace HAS.Content.Feature.EventLog
{
    public class EventLogBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IQueueService _queueService;

        public EventLogBehavior(IQueueService queueService, IConfiguration configuration)
        {
            _queueService = queueService;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            Model.EventLog commandLog = null;

            if (request is ICommandEvent)
            {
                commandLog = Model.EventLog.Create(request);
            }

            var response = await next();

            if (commandLog != null)
            {
                if(response is UploadAudioCommandResult)
                {
                    var rspObj = response as UploadAudioCommandResult;

                    commandLog.AddProfileId(rspObj.ProfileId);
                }

                commandLog.AddResult(response);
                await _queueService.AddMessage(commandLog);
            }

            return response;
        }
    }
}
