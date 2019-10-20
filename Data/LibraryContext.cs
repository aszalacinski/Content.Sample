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
    public class LibraryContext
    {
        private readonly DbContext _db;
        private IMongoCollection<HubDAO> _library;

        public IMongoCollection<HubDAO> Library { get; }

        public LibraryContext(IConfiguration configuration)
        {
            _db = DbContext.Create("my-practice", configuration["MongoDB:Library:ConnectionString"]);
            _library = _db.Database.GetCollection<HubDAO>("library");
            Library = _library;
        }

        [BsonIgnoreExtraElements]
        public class HubDAO
        {
            [BsonId]
            [BsonElement("_id")]
            public ObjectId Id { get; set; }

            [BsonRepresentation(BsonType.ObjectId)]
            [BsonElement("i_id")]
            public string InstructorId { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("c_date")]
            public DateTime CreateDate { get; set; }

            [BsonElement("ctent")]
            public IEnumerable<ContentDAO> Content { get; set; }

            [BsonElement("libs")]
            public IEnumerable<LibraryDAO> Libraries { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class ContentDAO
        {
            [BsonRepresentation(BsonType.ObjectId)]
            [BsonElement("_id")]
            public ObjectId Id { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("adate")]
            public DateTime AddDate { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class LibraryDAO
        {
            [BsonId]
            [BsonElement("_id")]
            public ObjectId Id { get; set; }

            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("desc")]
            public string Description { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("cdate")]
            public DateTime CreateDate { get; set; }

            [BsonElement("access")]
            [BsonRepresentation(BsonType.String)]
            public string Access { get; set; }
            
            [BsonElement("ctent")]
            public IEnumerable<ContentDAO> Content { get; set; }

            [BsonElement("dtribe")]
            public TribeDAO DefaultTribe { get; set; }

            [BsonElement("tribes")]
            public IEnumerable<TribeDAO> Tribes { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class TribeDAO
        {
            [BsonRepresentation(BsonType.ObjectId)]
            [BsonElement("_id")]
            public ObjectId Id { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("adate")]
            public DateTime AddDate { get; set; }
        }
    }

}
