using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DecoderLibrary
{
    public enum Permission
    {
        User,
        Admin
    }

    public enum DecoderPermission
    {
        You,
        YouAndAdmin,
        EveryOne
    }

    public class UserData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Permission Permission { get; set; }
        public DecoderPermission DeoderPermission { get; set; }
    }
}
