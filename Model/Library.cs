using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HAS.Content.Feature.Library.AddContentToLibrary;
using static HAS.Content.Feature.Library.AddTribeToLibrary;
using static HAS.Content.Feature.Library.CreateNewLibraryInHub;
using static HAS.Content.Feature.Library.DeleteLibraryDefaultTribe;
using static HAS.Content.Feature.Library.DeleteLibraryFromHub;
using static HAS.Content.Feature.Library.DeleteTribeFromLibrary;
using static HAS.Content.Feature.Library.RemoveContentFromLibrary;
using static HAS.Content.Feature.Library.SetLibraryAccess;
using static HAS.Content.Feature.Library.SetLibraryDefaultTribe;

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

        public bool Handle(CreateNewLibraryInHubCommand cmd) => AddLibrary(cmd);

        public bool Handle(SetLibraryAccessCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(AddTribeToLibraryCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(SetLibraryDefaultTribeCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(AddContentToLibraryCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(RemoveContentFromLibraryCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(DeleteLibraryFromHubCommand cmd) => DeleteLibrary(cmd);

        public bool Handle(DeleteTribeFromLibraryCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        public bool Handle(DeleteLibraryDefaultTribeCommand cmd) => GetLibrary(cmd.LibraryId).Handle(cmd);

        private bool DeleteLibrary(DeleteLibraryFromHubCommand cmd)
        {
            if (Libraries.Any(x => x.Id.Equals(cmd.LibraryId)))
            {
                var list = Libraries.ToList();
                var lib = list.Where(x => x.Id.Equals(cmd.LibraryId)).FirstOrDefault();
                if (list.Contains(lib))
                {
                    if (list.Remove(lib))
                    {
                        this.Libraries = list;
                        return true;
                    }
                    return false;
                }
                return true;
            }
            return true;
        }

        private bool AddLibrary(CreateNewLibraryInHubCommand cmd)
        {
            if(!Libraries.Any(x => x.Name.Equals(cmd.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var list = Libraries.ToList();
                var lib = Library.Create(cmd.Name, cmd.Description, DateTime.UtcNow, AccessType.PUBLIC, new List<Content>(), null, new List<Tribe>());
                list.Add(lib);
                Libraries = list;
                return true;
            }
            return false;
        }

        #region Utilities

        private Library GetLibrary(string libraryId) => Libraries.ToList().Where(x => x.Id == libraryId).FirstOrDefault();

        #endregion
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

        public bool Handle(SetLibraryAccessCommand cmd)
        {
            switch(cmd.Access.ToUpper())
            {
                case "PRIVATE":
                    return SetAccessToPrivate();
                case "PUBLIC":
                default:
                    return SetAccessToPublic();
            }
        }

        public bool Handle(AddTribeToLibraryCommand cmd)
        {
            return AddTribeToLibrary(cmd.LibraryId);
        }

        public bool Handle(SetLibraryDefaultTribeCommand cmd)
        {
            return SetDefaultTribe(cmd.TribeId);
        }

        public bool Handle(AddContentToLibraryCommand cmd)
        {
            return AddContentToLibrary(cmd.MediaId);
        }

        public bool Handle(RemoveContentFromLibraryCommand cmd)
        {
            return RemoveContentFromLibrary(cmd.MediaId);
        }

        public bool Handle(DeleteTribeFromLibraryCommand cmd)
        {
            return RemoveTribeFromLibrary(cmd.TribeId);
        }

        public bool Handle(DeleteLibraryDefaultTribeCommand cmd)
        {
            return DeleteLibraryDefaultTribe();
        }

        private bool DeleteLibraryDefaultTribe()
        {
            if(DefaultTribe != null)
            {
                DefaultTribe = null;
            }

            return DefaultTribe == null;
        }

        private bool RemoveTribeFromLibrary(string tribeId)
        {
            if (Tribes.Any(x => x.Id.Equals(tribeId)))
            {
                if (!IsDefault(tribeId))
                {
                    var list = this.Tribes.ToList();
                    var tribe = list.Where(x => x.Id == tribeId).FirstOrDefault();
                    if (list.Remove(tribe))
                    {
                        this.Tribes = list;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        private bool RemoveContentFromLibrary(string mediaId)
        {
            if (Content.Any(x => x.Id == mediaId))
            {
                var list = Content.ToList();
                var content = list.Where(x => x.Id == mediaId).FirstOrDefault();
                if (list.Remove(content))
                {
                    this.Content = list;
                    return true;
                }
                return false;
            }
            return true;
        }

        private bool AddContentToLibrary(string mediaId)
        {
            if (!Content.Any(x => x.Id == mediaId))
            {
                var newContent = Model.Content.Create(mediaId, DateTime.UtcNow);
                var list = Content.ToList();
                list.Add(newContent);
                this.Content = list;
                return true;
            }

            return true;
        }

        private bool SetDefaultTribe(string tribeId)
        {
            if(Tribes.Any(x => x.Id.Equals(tribeId)))
            {
                if(!IsDefault(tribeId))
                {
                    var list = Tribes.ToList();
                    var tribe = list.Where(x => x.Id == tribeId).FirstOrDefault();

                    if(tribe.Equals(null))
                    {
                        var add = AddTribe(tribeId);
                    }

                    DefaultTribe = Tribes.Where(x => x.Id == tribeId).FirstOrDefault();
                }
            }

            return DefaultTribe.Id == tribeId;
        }

        private bool AddTribe(string tribeId)
        {
            if (!Tribes.Any(x => x.Id.Equals(tribeId)))
            {
                var list = this.Tribes.ToList();

                var newTribe = Tribe.Create(tribeId, DateTime.UtcNow);
                list.Add(newTribe);
                this.Tribes = list;
            }

            return Tribes.Any(x => x.Id == tribeId);
        }

        private bool RemoveTribe(string tribeId)
        {
            if (Tribes.Any(x => x.Id.Equals(tribeId)))
            {
                if (!IsDefault(tribeId))
                {
                    var list = this.Tribes.ToList();
                    var tribe = list.Where(x => x.Id == tribeId).FirstOrDefault();
                    if (list.Remove(tribe))
                    {
                        this.Tribes = list;
                    }
                }
            }

            return Tribes.Any(x => x.Id == tribeId);
        }

        private bool IsDefault(string tribeId)
        {
            if(DefaultTribe != null)
            {
                return DefaultTribe.Id == tribeId;
            }
            else
            {
                return false;
            }
        }

        private bool AddTribeToLibrary(string tribeId)
        {
            if (!Tribes.Any(x => x.Id.Equals(tribeId)))
            {
                var list = this.Tribes.ToList();
                var newTribe = Tribe.Create(tribeId, DateTime.UtcNow);
                list.Add(newTribe);
                this.Tribes = list;
            }

            return Tribes.Any(x => x.Id == tribeId);
        }

        private bool SetAccessToPrivate()
        {
            Access = AccessType.PRIVATE;
            return Access.Equals(AccessType.PRIVATE);
        }

        private bool SetAccessToPublic()
        {
            Access = AccessType.PUBLIC;
            return Access.Equals(AccessType.PUBLIC);
        }
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
