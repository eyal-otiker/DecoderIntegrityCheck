using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public class UsersCRUD
    {
        public const string connectionString = "mongodb://localhost:27017";
        public const string databaseUsersName = "UsersLog";

        public UsersCRUD()
        {
            
        }

        public IUserDataRepository ConnectToDatabase()
        {
            IDatabaseContext databaseContext = new DatabaseContext(connectionString, databaseUsersName);
            IUserDataRepository summaryCollectionRepository = new UsersDataRepository(databaseContext);
            return summaryCollectionRepository;
        }

        public bool AddUser(string userName, string password, string decoderPermission)
        {
            IUserDataRepository testObjRepository = ConnectToDatabase();

            if (IsUserNameNotExist(userName, testObjRepository) == false)
                return false;

            testObjRepository.Add(new UserData
            {
               UserName = userName,
               Password = HashingClass.Hash(password),
               Permission = SetPremission(testObjRepository),
               DeoderPermission = (DecoderPermission)int.Parse(decoderPermission)
            });

            return true;
        }

        private Permission SetPremission(IUserDataRepository testObjRepository)
        {
            IMongoCollection<UserData> mongoCollection = testObjRepository.GetCollection();
            List<UserData> usersList = mongoCollection.Find(FilterDefinition<UserData>.Empty).ToList();

            if (usersList.Count == 0)
                return Permission.Admin;
            else
                return Permission.User;
        }

        private bool IsUserNameNotExist(string userName, IUserDataRepository testObjRepository)
        {
            FilterDefinition<UserData> filter = Builders<UserData>.Filter.Eq(item => item.UserName, userName);
            IMongoCollection<UserData> mongoCollection = testObjRepository.GetCollection();
            List<UserData> usersList = mongoCollection.Find(filter).ToList();

            if (usersList.Count == 0)
                return true;
            else
                return false;
        }

        public Tuple<bool, string> Login(string userName, string password) // for login
        {
            Tuple<bool, string> loginParmeters = new Tuple<bool, string>(false, ""); // isUserExist, userRole

            IUserDataRepository testObjRepository = ConnectToDatabase();

            FilterDefinition<UserData> filters = Builders<UserData>.Filter.Eq(item => item.UserName, userName);
            List<UserData> userData = testObjRepository.GetCollection().Find(filters).ToList();

            if (userData.Count != 0 && HashingClass.Veritify(password, userData.ToArray()[0].Password) == true)
                loginParmeters = new Tuple<bool, string>(true, userData.ToArray()[0].Permission.ToString());

            return loginParmeters;
        }

        public DecoderPermission GetDecoderPermissionForUser(string userName)
        {
            IUserDataRepository testObjRepository = ConnectToDatabase();
            FilterDefinition<UserData> filters = Builders<UserData>.Filter.Eq(item => item.UserName, userName);
            List<UserData> userData = testObjRepository.GetCollection().Find(filters).ToList();
            return userData.ToArray()[0].DeoderPermission;
        }

        public Permission GetUserPermission(string userName)
        {
            IUserDataRepository testObjRepository = ConnectToDatabase();
            FilterDefinition<UserData> filters = Builders<UserData>.Filter.Eq(item => item.UserName, userName);
            List<UserData> userData = testObjRepository.GetCollection().Find(filters).ToList();
            return userData.ToArray()[0].Permission;
        }

        public List<UserData> GetUsersInDatabase()
        {
            IUserDataRepository testObjRepository = ConnectToDatabase();
            List<UserData> userData = testObjRepository.GetCollection().Find(Builders<UserData>.Filter.Empty).ToList();
            return userData;
        }
    }
}
