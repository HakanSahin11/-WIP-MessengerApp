using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using API_Setup_User_config.Models;
using MongoDB.Driver.Linq;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using static API_Setup_User_config.Models.BsonSectioncs;

namespace API_Setup_User_config.Controllers
{
    /// Set up Bson for login ban on API lvl
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public List<List<UserClass>> DatabaseGet = new List<List<UserClass>>();
        public UserClass DatabaseGetOne { get;set; }
        public SaltClass DatabasePost { get; set; }
        public chatListsMethod ChatLists { get; set; }
      //  public static IMongoCollection<BsonDocument> BsonCollection { get;set; }
        public IMongoCollection<BsonDocument> BsonCollectionLog { get; set; }
        public bool Status { get; set; }

        public static IMongoCollection<BsonDocument> BsonCollection(string user, string pass, string section)
        {

            IMongoDatabase client = new MongoClient($"mongodb://{user}:{pass}@localhost:27017").GetDatabase("Virksomhed");

            return client.GetCollection<BsonDocument>( section);
        }

        public bool dbSetup(string user, string pass, string usage, string email, string match, int? id)
        {
            // sets the Mongodb registry to turn on "IgnoreExtraElements", so the usage of multiple combined classes can be used to sterilize the data from MongoDB, used for the class "UserClass"
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            IMongoDatabase client = new MongoClient($"mongodb://{user}:{pass}@localhost:27017").GetDatabase("Virksomhed");
            
            var result = true;
            switch (usage)
            {
                case "get":
                    var userQuery = from c in client.GetCollection<UserClass>(UserClass.Name).AsQueryable()
                                    select c;

                    foreach (UserClass output in userQuery)
                    {
                        List<UserClass> userClasses = new List<UserClass> { output };
                        DatabaseGet.Add(userClasses);
                    }
                    break;

                case "getOne":
                    IMongoQueryable<UserClass> usageQuery;
                    // BsonCollection = client.GetCollection<BsonDocument>("Users");
                    BsonCollection(user, pass, "Users");
                    if (email != null)
                    {
                           usageQuery =
                           from c in client.GetCollection<UserClass>(UserClass.Name).AsQueryable()
                           where c.Email == email
                           select c;
                    }
                    else
                    {
                        usageQuery =
                          from c in client.GetCollection<UserClass>(UserClass.Name).AsQueryable()
                          where c._id == id
                          select c;
                    }

                    foreach (UserClass output in usageQuery)
                    {
                        DatabaseGetOne = output;
                    }            
                    break;

                case "post":
                    DateTime LoginBan = DateTime.Now;
                    dbSetup("GateKeeper", "silvereye", "get", null, null, null);

                    bool emailSearch = false;
        
                    foreach (var items in DatabaseGet)
                    {
                        foreach (var item in items)
                        {
                            if (item.Email != null && item.Email.Contains(email))
                            {
                                DatabaseGetOne = item;
                                emailSearch = true;
                                LoginBan = Convert.ToDateTime(DatabaseGetOne.LoginBan);
                            }
                        }
                    }

                    if (emailSearch == false ||  LoginBan > DateTime.Now)
                    {
                        return false;
                    }
                    var postUsageQuery = from c in client.GetCollection<SaltClass>(SaltClass.Name).AsQueryable()
                                     where c._id == DatabaseGetOne._id
                                     select c;
                    
                    foreach (SaltClass output in postUsageQuery)
                    {
                        DatabasePost = output;
                    }
                    //if (Sha256.Sha256Hash(match, DatabasePost.Salt) != DatabaseGetOne.Password)
                    if (Sha256.NewSha256Hash(match, Convert.FromBase64String(DatabasePost.Salt)).hashSalt != DatabaseGetOne.Password)
                    {
                       /*
                            banTime += banTime;
                            DateTime banTimer = DateTime.Now.AddMinutes(banTime);
                            bsonSection(DatabaseGetOne._id, "LoginBan", Convert.ToString(banTimer), client.GetCollection<BsonDocument>(UserClass.Name));
                           */
                        result = false;
                    }
                    break;

                case "friendsList":
                    var friendsListNames =
                      from c in client.GetCollection<UserClass>(UserClass.Name).AsQueryable()
                      where c._id == id
                      select c;

                    foreach (UserClass output in friendsListNames)
                    {
                        DatabaseGetOne = output;
                    }
                    break;
                case "PostAddFriend":

                    try
                    {
                        dbSetup(user, pass, "getOne", email, match, id);
                        List<int> friendsList = new List<int>();
                        friendsList.AddRange(DatabaseGetOne.SentFriendReq);
                        friendsList.Add(Convert.ToInt32(match));
                        bsonObjects bson = bsonSection(id, "SentFriendReq", "", 0, friendsList);
                  //      BsonCollection.UpdateOne(bson.Filter, bson.Update);
                        BsonCollection(user, pass, "Users").UpdateOne(bson.Filter, bson.Update);


                        dbSetup(user, pass, "getOne", email, null, Convert.ToInt32(match));
                        friendsList.Clear();
                        friendsList.AddRange(DatabaseGetOne.IncFriendReq);
                        friendsList.Add(Convert.ToInt32(id));
                        bson = bsonSection(Convert.ToInt32(match), "IncFriendReq", "", 0, friendsList);
                        BsonCollection(user, pass, "Users").UpdateOne(bson.Filter, bson.Update);
                        Status = true;
                    }
                    catch
                    {
                        Status = false;
                    }
                    break;
                case "PostSentReq":
                    dbSetup(user, pass, "getOne", email, match, id);
                    break;

                case "PersonalchatController":
                    //being used to declare BsonCollectionLog for the PersonalchatControllers use, to when it has to make edits to the database
                    BsonCollectionLog = client.GetCollection<BsonDocument>("Log");
                    break;

                case "Delete":
                    FilterDefinition<BsonDocument> deletefilter;
                    IMongoCollection<BsonDocument> BsonDoc; 
                    if (match == "User")
                    {
                        BsonDoc = client.GetCollection<BsonDocument>("Users");
                        deletefilter = Builders<BsonDocument>.Filter.Eq("_id", id);
                    }
                    else if( match == "System")
                    {
                        BsonDoc = client.GetCollection<BsonDocument>("System");
                        deletefilter = Builders<BsonDocument>.Filter.Eq("_id", id);
                    }
                    else
                    {
                        BsonDoc = client.GetCollection<BsonDocument>("Log");
                        deletefilter = Builders<BsonDocument>.Filter.Eq("_id", id);
                    }

                    BsonDoc.DeleteOne(deletefilter);
                    break;
            }
            return result;
        }

        


        // GET: api/User
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                dbSetup("GateKeeper","silvereye", "get", null, null, null);
                return Ok(DatabaseGet);
            }
            catch (Exception e)
            {
                return Ok($"Error! \n\n{e.Message}");
            }
        }

        // GET: api/User/email
        [HttpGet("{email}", Name = "Get")]
        public ActionResult Get(string email)
        {
            dbSetup("GateKeeper", "silvereye", "getOne", email, null, null);
            return Ok(DatabaseGetOne);
        }

        // POST: api/User
        [HttpPost]
        public ActionResult Post([FromBody] JsonElement json)
        {
            //todo make a switch instead of if else
            string usage = json.GetString("usage");
            int id = Convert.ToInt32(json.GetString("id"));
            string value = json.GetString("value");


            if (usage == "Login" || usage == "friendsList")
            {
                string match = json.GetString("match");

                string email = json.GetString("email");


                if (usage == "Login")
                {
                    Post post = new Post(email, match);
                    LoginResult result = new LoginResult(dbSetup("System", "silvereye", "post", post.email, post.match, null), DatabaseGetOne.UserType);
                    return Ok(result);
                }
                else
                {
                    // return all the users first + lastnames by using the ID sent by the application
                    dbSetup("GateKeeper", "silvereye", "friendsList", null, null, id);
                    try
                    {
                        return Ok($"{DatabaseGetOne.FirstName} {DatabaseGetOne.LastName}");
                    }
                    catch
                    {
                        //case if 0 friends
                        return Ok("Potus");
                    }
                }
            }
            else if (usage == "AddFriends")
            {
                dbSetup("GateKeeper", "silvereye", "get", "", "", null);
                List<PostReturnUsers> UserDetails = new List<PostReturnUsers>();
                foreach (var item in DatabaseGet)
                {
                    item.ToList().ForEach(x => UserDetails.Add(new PostReturnUsers($"{x.FirstName} {x.LastName}",  x._id)));
                }
                return Ok(JsonConvert.SerializeObject(UserDetails));
            }
            else if (usage == "PostAddFriend")
            {
                dbSetup("GateKeeper", "silvereye", usage, null, value, id);
                return Ok(Status);
            }
            else if ( usage == "PostSentReq" || usage == "RecievedReq")
            {
                dbSetup("GateKeeper", "silvereye", "getOne", null, null, id);
                List<PostReturnUsers> postResult = new List<PostReturnUsers>();
                if (usage == "PostSentReq")
                {
                    foreach (var item in DatabaseGetOne.SentFriendReq)
                    {
                        dbSetup("GateKeeper", "silvereye", "getOne", null, null, item);
                        postResult.Add(new PostReturnUsers($"{DatabaseGetOne.FirstName} {DatabaseGetOne.LastName}", item));
                    }
                }
                else
                {
                    foreach (var item in DatabaseGetOne.IncFriendReq)
                    {
                        dbSetup("GateKeeper", "silvereye", "getOne", null, null, item);
                        postResult.Add(new PostReturnUsers($"{DatabaseGetOne.FirstName} {DatabaseGetOne.LastName}", item));
                    }
                }
                return Ok(JsonConvert.SerializeObject(postResult));
            }
            else if (usage == "FriendReqAccep")
            {
                dbSetup("GateKeeper", "silvereye", "getOne", null, null, id);
                List<int> IncReq = new List<int> (DatabaseGetOne.IncFriendReq);
                IncReq.Remove(Convert.ToInt32(value));
                var bson = bsonSection(id, "IncFriendReq", "", 0, IncReq);
                BsonCollection("GateKeeper","silvereye", "Users").UpdateOne(bson.Filter, bson.Update);

                List<int> friendsList = new List<int>(DatabaseGetOne.FriendsList);
                friendsList.Add(Convert.ToInt32(value));
                bson = bsonSection(id, "FriendsList", "", 0, friendsList);
                BsonCollection("GateKeeper","silvereye", "Users").UpdateOne(bson.Filter, bson.Update);

                dbSetup("GateKeeper", "silvereye", "getOne", null, null, Convert.ToInt32(value));
                List<int> SentReq = new List<int>(DatabaseGetOne.SentFriendReq);
                SentReq.Remove(id);
                bson = bsonSection(DatabaseGetOne._id, "SentFriendReq", "", 0, SentReq);
                BsonCollection("GateKeeper", "silvereye", "Users").UpdateOne(bson.Filter, bson.Update);

                friendsList.Clear();
                friendsList.AddRange(DatabaseGetOne.FriendsList);
                friendsList.Add(id);
                //Make the "cancel" friends request button work as well
                return Ok();
            }
            else if (usage == "CancelSentReq")
            {
                dbSetup("GateKeeper", "silvereye", "getOne", null, null, id);
                List<int> list = new List<int>(DatabaseGetOne.SentFriendReq);
                list.Remove(Convert.ToInt32(value));
                var bson = bsonSection(id, "SentFriendReq", "", 0, list);
                BsonCollection("GateKeeper","silvereye", "Users").UpdateOne(bson.Filter, bson.Update);

                dbSetup("GateKeeper", "silvereye", "getOne", null, null, Convert.ToInt32(value));
                list = new List<int>(DatabaseGetOne.IncFriendReq);
                list.Remove(id);
                bson = bsonSection(Convert.ToInt32(value), "IncFriendReq", "", 0, list);
                BsonCollection("GateKeeper","silvereye", "Users").UpdateOne(bson.Filter, bson.Update);

                return Ok();
            }
            else if(usage == "Delete")
            {
                //delete sections
                dbSetup("adminUser", "silvereye", "Delete", null, "User", id);
                dbSetup("adminUser", "silvereye", "Delete", null, "System", id);
                dbSetup("adminUser", "silvereye", "Delete", null, "Log", id);
                return Ok("Success");
            }
            else
            {
                return Ok();
            }
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
