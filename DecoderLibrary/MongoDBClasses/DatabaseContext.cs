using MongoDB.Driver;

namespace DecoderLibrary
{
    interface IDatabaseContext
    {
        IMongoDatabase Database { get; }
        MongoClient MongoClient { get; }
    }

    internal class DatabaseContext : IDatabaseContext
    {
        public IMongoDatabase Database { get; }
        public MongoClient MongoClient { get; }

        public DatabaseContext(string connectionString, string databaseName)
        {
            MongoClient = new MongoClient(connectionString);
            Database = MongoClient.GetDatabase(databaseName);
        }
    }
}
