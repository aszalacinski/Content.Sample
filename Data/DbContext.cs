using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace HAS.Content.Data
{
    public class DbContext
    {
        private IMongoClient _client { get; set; }
        private IMongoDatabase _database { get; set; }
        private DbContext() { }
        public static DbContext Create(string databaseName, string mongoDbConnectionString)
        {
            var _dbContext = new DbContext();

            var pack = new ConventionPack();
            pack.AddMemberMapConvention("LowerCaseElementName", m => m.SetElementName(m.MemberName.ToLower()));

            ConventionRegistry.Register("LowerCase", pack, type => true);

            _dbContext._client = new MongoClient(mongoDbConnectionString);
            _dbContext._database = _dbContext._client.GetDatabase(databaseName);

            return _dbContext;
        }

        public IMongoDatabase Database
        {
            get
            {
                return _database;
            }
        }
    }
}
