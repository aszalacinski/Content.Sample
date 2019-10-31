using MediatR;
using System;
using System.Reflection;

namespace HAS.Content.Model
{
    public class EventLog : IEntity
    {
        public string Id { get; private set; }
        public DateTime CaptureDate { get; private set; }
        public string Assembly { get; private set; }
        public string Event { get; private set; }
        public string ProfileId { get; private set; }
        public string HubId { get; private set; }
        public object Message { get; private set; }
        public object Result { get; private set; }

        private EventLog(IMediator mediator) 
        { 
            
        }

        private EventLog(object command)
        {
            CaptureDate = DateTime.UtcNow;
            Assembly = command.GetType().Assembly.GetName().Name;
            Event = command.GetType().Name;

            PropertyInfo profileInfo = command.GetType().GetProperty("ProfileId") ?? command.GetType().GetProperty("InstructorId");
            if(profileInfo != null)
            {
                ProfileId = (string)profileInfo.GetValue(command, null);
            }

            PropertyInfo hubInfo = command.GetType().GetProperty("HubId");
            if(hubInfo != null)
            {
                HubId = (string)hubInfo.GetValue(command, null);
            }

            if(!Event.Equals("UploadAudioCommand"))
            {
                Message = command;
            }
            else
            {
                Message = "Upload Audio - Command to large to upload";
            }
        }

        public static EventLog Create(object item) => new EventLog(item);

        public void AddProfileId(string profileId) => ProfileId = profileId;
        public void AddResult(object result) => Result = result;
    }
}
