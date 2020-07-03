using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Setup_User_config.Models;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
using MongoDB.Bson;


namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateUserController : ControllerBase 
    {
        // POST: api/CreateUser
        [HttpPost]
        public ActionResult Post([FromBody] JsonElement json)
        {
            var hashing = Sha256.NewSha256Hash(json.GetString("Password"), null);
            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User"));
            var id = (JsonConvert.DeserializeObject<List<List<UserClass>>>(jsonStr).Last().Last())._id + 1;
            var newId = (JsonConvert.DeserializeObject<List<List<UserClass>>>(jsonStr).Select(x => x.Max(y => y._id))).Max() + 1;
            //add to bson
            var sendUser = new BsonDocument
            {
                {"_id"              , newId },
                {"Email"            , json.GetString("Email") },
                {"Password"         , hashing.hashSalt },
                {"FriendsList"      , new BsonArray()},
                {"IncFriendReq"     , new BsonArray()},
                {"SentFriendReq"    , new BsonArray()},
                {"UserType"         , json.GetString("UserType") },
                {"FirstName"        , json.GetString("FirstName")} ,
                {"LastName"         , json.GetString("LastName")} ,
                {"Gender"           , json.GetString("Gender")} ,
                {"Country"          , json.GetString("Country")} ,
                {"City"             , json.GetString("City")} ,
                {"Address"          , json.GetString("Address")} ,
                {"JobTitle"         , json.GetString("JobTitle")} ,
                {"Age"              , json.GetString("Age")} ,
                { "LoginBan"        , $"{DateTime.Now}"} ,
             };
            var sendLog = new BsonDocument
            {
                {"_id", newId },
                {"Chat",  new BsonArray()}
            };

            var sendSystem  = new BsonDocument
            {
                { "_id", newId },
                { "Salt", hashing.salt} 
        };
            CreateUser("GateKeeper", "silvereye", sendUser, "Users");
            CreateUser("GateKeeper", "silvereye", sendLog, "Log");
            CreateUser("System", "silvereye", sendSystem, "System");
            return Ok($"Success");
        }
        public void CreateUser(string user, string pass, BsonDocument doc, string coll) {
            IMongoDatabase client = new MongoClient($"mongodb://{user}:{pass}@localhost:27017").GetDatabase("Virksomhed");
            var collection = client.GetCollection<BsonDocument>(coll);
            collection.InsertOne(doc);
        }
    }
}
