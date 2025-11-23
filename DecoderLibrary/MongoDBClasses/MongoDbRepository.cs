using MongoDB.Driver;

namespace DecoderLibrary
{
    abstract class MongoDbRepository<T>
    {
        protected IMongoDatabase _db;
        protected readonly IMongoCollection<T> collection;

        public MongoDbRepository(IDatabaseContext databaseContext)
        {
            _db = databaseContext.Database;
            collection = _db.GetCollection<T>(typeof(T).Name);
        }
    }
}