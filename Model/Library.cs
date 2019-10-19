using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Content.Model
{
    public class Hub : IEntity
    {
        public string Id { get; private set; }
        public string InstructorId { get; private set; }
        public DateTime CreateDate { get; private set; }
        public IEnumerable<Content> Content { get; private set; }
        public IEnumerable<Library> Libraries { get; private set; }

        private Hub() { }

        private Hub(string id, string instructorId, DateTime createDate, List<Content> content, List<Library> libraries)
        {
            Id = id;
            InstructorId = instructorId;
            CreateDate = createDate;
            Content = content;
            Libraries = libraries;
        }

        public static Hub Create(string id, string instructorId, DateTime createDate, List<Content> content, List<Library> libraries)
            => new Hub(id, instructorId, createDate, content, libraries);
    }

    public class Content
    {
        public string Id { get; private set; }
        public DateTime AddDate { get; private set; }

        private Content() { }

        private Content(string id, DateTime addDate)
        {
            Id = id;
            AddDate = addDate;
        }

        public static Content Create(string id, DateTime addDate)
            => new Content(id, addDate);
    }

    public class Library : IEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DateTime CreateDate { get; private set; }
        public AccessType Access { get; private set; }
        public IEnumerable<Content> Content { get; private set; }
        public Tribe DefaultTribe { get; private set; }
        public IEnumerable<Tribe> Tribes { get; private set; }


        private Library() { }

        private Library(string name, string description, DateTime createDate, AccessType access, IEnumerable<Content> content, Tribe defaultTribe, IEnumerable<Tribe> tribes)
        {
            Name = name;
            Description = description;
            CreateDate = createDate;
            Access = access;
            Content = content;
            DefaultTribe = defaultTribe;
            Tribes = tribes;
        }

        public static Library Create(string name, string description, DateTime createDate, AccessType access, IEnumerable<Content> content, Tribe defaultTribe, IEnumerable<Tribe> tribes)
            => new Library(name, description, createDate, access, content, defaultTribe, tribes);
    }

    public class Tribe
    {
        public string Id { get; private set; }
        public DateTime AddDate { get; private set; }

        private Tribe() { }

        private Tribe(string id, DateTime addDate)
        {
            Id = id;
            AddDate = addDate;
        }

        public static Tribe Create(string id, DateTime addDate)
            => new Tribe(id, addDate);
    }

    public enum AccessType
    {
        NONE = 0,
        PUBLIC,
        PRIVATE
    }
}
