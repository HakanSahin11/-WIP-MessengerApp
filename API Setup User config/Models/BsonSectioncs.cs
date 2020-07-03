using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Setup_User_config.Models
{
    public class BsonSectioncs
    {
        public static bsonObjects bsonSection(int? id, string section, string input, int value, List<int> list)
        {
            //Current section is used to make filter and update definitions for the MongoDB changes
            FilterDefinition<BsonDocument> filter;
            UpdateDefinition<BsonDocument> update;
            if (input != "")
            {
                //Changes with given string input
                filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                update = Builders<BsonDocument>.Update.Set(section, input);
            }
            else if (value != 0)
            {
                //changes with integer value
                filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                update = Builders<BsonDocument>.Update.Set(section, value);
            }
            else
            {
                //changes with int List input (friendslist changes)
                filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                update = Builders<BsonDocument>.Update.Set(section, list);
            }
            return new bsonObjects(filter, update);

        }
    }
}
