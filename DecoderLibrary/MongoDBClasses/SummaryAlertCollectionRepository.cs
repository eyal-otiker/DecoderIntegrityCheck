using MongoDB.Driver;

namespace DecoderLibrary
{
    public interface ISummaryAlertCollectionRepository
    {
        SummaryAlertCollectionItem Add(SummaryAlertCollectionItem obj);
        IMongoCollection<SummaryAlertCollectionItem> GetCollection();
    }

    internal class SummaryAlertCollectionRepository : MongoDbRepository<SummaryAlertCollectionItem>, ISummaryAlertCollectionRepository
    {
        public SummaryAlertCollectionRepository(IDatabaseContext databaseContext) : base(databaseContext)
        {

        }

        public SummaryAlertCollectionItem Add(SummaryAlertCollectionItem obj)
        {
            collection.InsertOne(obj);
            return obj;
        }

        public IMongoCollection<SummaryAlertCollectionItem> GetCollection()
        {
            return collection;
        }
    }
}
