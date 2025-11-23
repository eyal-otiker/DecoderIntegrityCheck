using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class SummaryAlertCollectionItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; }
        public string UserName { get; set; }
        public DateTime CheckDate { get; set; }
        public string DecoderName { get; set; }
        public string ClientDecoderLocation { get; set; }
        public string DataLink { get; set; }
        public string Version { get; set; }
        public string DecoderWritingDate { get; set; }
        public string IcdTypeName { get; set; }
        public string NativeCheck { get; set; }
        public string ThroughtCheck { get; set; }
        public string CountProper { get; set; }
        public string CountNotProper { get; set; }
        public string CountNotValid { get; set; }
        public List<string> ProperList { get; set; }
        public List<string> NotProperList { get; set; }
        public List<string> NotValidList { get; set; }
    }
}
