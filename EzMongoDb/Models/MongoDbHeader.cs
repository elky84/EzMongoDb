﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace EzMongoDb.Models
{
    public class MongoDbHeader
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_Id")]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Created { get; set; } = DateTime.Now;


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Updated { get; set; }
    }

    public static class MongoDbHeaderUtil
    {
        public static string GenerateId(this MongoDbHeader header)
        {
            header.Id = ObjectId.GenerateNewId().ToString();
            return header.Id;
        }

        public static void CopyHeader(this MongoDbHeader source, MongoDbHeader dest)
        {
            dest.Id = source.Id;
            dest.Created = source.Created;
            dest.Updated = source.Updated;
        }
    }
}
