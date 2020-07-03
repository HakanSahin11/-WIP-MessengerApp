using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using API_Setup_User_config.Models;

namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        public chatListsMethod ApiSetup(string user, string pass, int id)
        {
            IMongoDatabase client = new MongoClient($"mongodb://{user}:{pass}@localhost:27017").GetDatabase("Virksomhed");
           var logsQuery =
                from c in client.GetCollection<chatListsMethod>(chatListsMethod.Name).AsQueryable()
                where c._id == id
                select c;
            List<chatListsMethod> list = new List<chatListsMethod>();
            chatListsMethod chatLists = new chatListsMethod(0, null);
            foreach (var item in logsQuery)
            {
                chatLists = item;
            }
            return chatLists;
        }
        // GET: api/Chat
        [HttpGet]
        public IEnumerable<string> GetChat()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Chat/5
        [HttpGet("{id}", Name = "GetChat")]
        public ActionResult Get(int id)
        {
            return Ok (ApiSetup("GateKeeper", "silvereye", id));
        }
    }
}
