using MongoDB.Driver;

namespace Casino_onlineAPI.config
{
    public class MongoDBService
    {
        private readonly IConfiguration _configuration;

        private readonly IMongoDatabase _database;


        public MongoDBService(IConfiguration configuration)
        {
            _configuration = configuration;

            var conectionString = _configuration.GetConnectionString("DbConnection");

            var mongoUrl = MongoUrl.Create(conectionString);

            var mongoClient = new MongoClient(mongoUrl);
            
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }


        public IMongoDatabase? Database => _database;
    }
}
