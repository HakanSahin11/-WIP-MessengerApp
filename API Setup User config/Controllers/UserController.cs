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

        bool dbSetup(string user, string pass, string usage, string email, string match, int? id)
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
                    //should be unused atm, other than when requesting specific
                    var usageQuery =
                       from c in client.GetCollection<UserClass>(UserClass.Name).AsQueryable()
                       where c.Email == email
                       select c;

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
                            if (item.Email.Contains(email))
                            {
                                DatabaseGetOne = item;
                                emailSearch = true;
                                LoginBan = Convert.ToDateTime(DatabaseGetOne.LoginBan);
                            }
                        }
                    }

                    //   var test2 = DatabaseGet.Any(x => x.Any(y => y.Email == email)); Test version work in progress, gives bool so far instead if list
                    /*
                    var data = from e in DatabaseGet.AsParallel().WithDegreeOfParallelism(1)     gives whole object?
                               where e.Any(b => b.Email.Contains(email)) 
                               select e;
                                
                    */


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
                    //     dbSetup("GateKeeper", "silvereye", "getOne", value, null);
                    if (Sha256.Sha256Hash(match, DatabasePost.Salt) != DatabaseGetOne.Password)
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
            }


            return result;
        }

        public void bsonSection(int id, string section, string input, IMongoCollection<BsonDocument> Collection)
        {
            //sends the ban timer to the mongodb "LoginBan" - Needs to be set up
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var update = Builders<BsonDocument>.Update.Set(section, input);
            Collection.UpdateOne(filter, update);

            
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
            string match = json.GetString("match");

            string email = json.GetString("email");
            string usage = json.GetString("usage");

            int id = Convert.ToInt32(json.GetString("id"));
            if (usage == "login")
            {
                Post post = new Post(email, match);
                return Ok(dbSetup("System", "silvereye", "post", post.email, post.match, null));
            }
            else
            {
                // return all the users first + lastnames by using the ID sent by the application
                dbSetup("GateKeeper", "silvereye", "friendsList", null, null, id);
                return Ok($"{DatabaseGetOne.FirstName} {DatabaseGetOne.LastName}");
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
