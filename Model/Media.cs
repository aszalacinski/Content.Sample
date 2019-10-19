using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Content.Model
{
    public class Media : IEntity
    {
        public string Id { get; private set; }
        public string InstructorId { get; private set; }
        public string InstructorName { get; private set; }
        public string FileName { get; private set; }
        public FileType FileType { get; private set; }
        public string FileExtension { get; private set; }
        public ContentDetails ContentDetails { get; private set; }
        public DateTime RecordingDate { get; private set; }
        public DateTime UploadDate { get; private set; }
        public StateDetails State { get; private set; }
        public Manifest Manifest { get; private set; }

        private Media() { }

        private Media(string id, string instructorId, string instructorName, string fileName, FileType fileType, string fileExtension, DateTime recordingDate, DateTime uploadDate, ContentDetails contentDetails, StateDetails stateDetails, Manifest manifest)
        {
            Id = id;
            InstructorId = instructorId;
            InstructorName = instructorName;
            FileName = fileName;
            FileType = fileType;
            FileExtension = fileExtension;
            RecordingDate = recordingDate;
            UploadDate = uploadDate;
            ContentDetails = contentDetails;
            State = stateDetails;
            Manifest = manifest;
        }

        public static Media Create(string id, string instructorId, string instructorName, string fileName, FileType fileType, string fileExtension, DateTime recordingDate, DateTime uploadDate, ContentDetails contentDetails, StateDetails stateDetails, Manifest manifest)
        {
            return new Media(id, instructorId, instructorName, fileName, fileType, fileExtension, recordingDate, uploadDate, contentDetails, stateDetails, manifest);
        }
    }

    public enum FileType
    {
        audio,
        video,
        picture,
        shortVideo
    }

    public class ContentDetails
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public long Duration { get; private set; }
        public long Size { get; private set; }
        public double LikeScore { get; private set; }
        public List<string> Tags { get; private set; }
        
        private ContentDetails() { }

        private ContentDetails(string title, string description, long duration, long size, double likeScore, IEnumerable<string> tags)
        {
            Title = title;
            Description = description;
            Duration = duration;
            Size = size;
            LikeScore = likeScore;
            Tags = tags.ToList();
        }

        public static ContentDetails Create(string title, string description, long duration, long size, double likeScore, IEnumerable<string> tags)
        {
            return new ContentDetails(title, description, duration, size, likeScore, tags);
        }
    }

    public class StateDetails
    {
        public StatusType Status { get; private set; }
        public DateTime Published { get; private set; }
        public DateTime Staged { get; private set; }
        public DateTime Archived { get; private set; }

        private StateDetails() { }

        private StateDetails(StatusType statusType, DateTime publishedDate, DateTime stagedDate, DateTime archivedDate)
        {
            Status = statusType;
            Published = publishedDate;
            Staged = stagedDate;
            Archived = archivedDate;
        }

        public static StateDetails Create(StatusType statusType, DateTime publishedDate, DateTime stagedDate, DateTime archivedDate)
            => new StateDetails(statusType, publishedDate, stagedDate, archivedDate);
    }

    public enum StatusType
    {
        NONE = 0,
        STAGED,
        PUBLISHED,
        ARCHIVED
    }

    public class Manifest
    {
        public Dictionary<string, string> Items { get; private set; }

        private Manifest() { }

        private Manifest(Dictionary<string, string> items)
        {
            Items = items;
        }

        public static Manifest Create(Dictionary<string, string> items)
            => new Manifest(items);
    }
}
