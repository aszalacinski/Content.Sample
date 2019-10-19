using HAS.Content.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace HAS.Content.Data
{
    public class ContentContext
    {
        private readonly DbContext _db;
        private IMongoCollection<ContentDAO> _media;

        public IMongoCollection<ContentDAO> Content { get; }

        public ContentContext(IConfiguration configuration)
        {
            _db = DbContext.Create("my-practice", configuration["MongoDB:Content:ConnectionString"]);
            _media = _db.Database.GetCollection<ContentDAO>("content");
            Content = _media;
        }
    }

    [BsonIgnoreExtraElements]
    public class ContentDAO
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("i_id")]
        public string InstructorId { get; set; }

        [BsonElement("iname")]
        public string InstructorName { get; set; }

        [BsonElement("cfname")]
        public string FileName { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("ftype")]
        public FileType FileType { get; set; }

        [BsonElement("fext")]
        public string FileExtension { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("rdate")]
        public DateTime RecordingDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("udate")]
        public DateTime UploadDate { get; set; }

        [BsonElement("ctitle")]
        public string Title { get; set; }

        [BsonElement("cdesc")]
        public string Description { get; set; }

        [BsonElement("cdur")]
        public long Duration { get; set; }

        [BsonElement("csize")]
        public long Size { get; set; }

        [BsonElement("lscore")]
        public double LikeScore { get; set; }

        [BsonElement("tags")]
        public IEnumerable<string> Tags { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public StatusType Status { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("pdate")]
        public DateTime Published { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("sdate")]
        public DateTime Staged { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("adate")]
        public DateTime Archived { get; set; }

        [BsonElement("manifest")]
        public ManifestDAO Manifest { get; set; }


    }

    [BsonIgnoreExtraElements]
    public class ManifestDAO
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        [BsonElement("items")]
        public Dictionary<string, string> Items { get; set; }
    }
}
