using MongoDB.Driver;

namespace DecoderLibrary
{
    public interface IUserDataRepository
    {
        UserData Add(UserData obj);
        IMongoCollection<UserData> GetCollection();
    }

    internal class UsersDataRepository : MongoDbRepository<UserData>, IUserDataRepository
    {
        public UsersDataRepository(IDatabaseContext databaseContext) : base(databaseContext)
        {

        }

        public UserData Add(UserData obj)
        {
            collection.InsertOne(obj);
            return obj;
        }

        public IMongoCollection<UserData> GetCollection()
        {
            return collection;
        }
    }
}
