using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_Setup_User_config.Controllers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using API_Setup_User_config.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

//Error line 68
namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalChatController : Controller
    {
        public IMongoCollection<BsonDocument> BsonLog;

        [HttpPost]
        public ActionResult Post([FromBody] JsonElement json)
        {
            var currentId = Convert.ToInt32(json.GetString("CurrentID"));
            var matchId   = Convert.ToInt32(json.GetString("MatchID"));
            Chats chat = new Chats(json.GetString("message"), json.GetString("Timestamp"), Convert.ToBoolean(json.GetString("CurrentUser")));
            


            ChatController chatController = new ChatController();
            var chatListsMethod = chatController.ApiSetup("GateKeeper", "silvereye", currentId);
            var newMessage = true;
            foreach (var item in chatListsMethod.Chat)
            {
               if (matchId == item._id)
                {
                    newMessage = false;
                    var newChatLists = item.chatLists;
                    newChatLists.Add(chat);
                    item.chatLists = newChatLists;
                }
            }
            if (newMessage == true)
            {
             //   List<messages> messages = new List<messages> { new messages(matchId, new List<Chats> { chat }) };
                messages message = new messages(matchId, new List<Chats> { chat });
                chatListsMethod.Chat.Add(message);
                
            }
         //   messages TestnewMessage = new messages(matchId, List<Chats>(chat));

            UserController userController = new UserController();
            userController.dbSetup("System", "silvereye", "PersonalchatController", "", "", currentId);
            BsonLog = userController.BsonCollectionLog;
            BsonSectionLog(currentId, "Chat", chatListsMethod.Chat);

            return Ok($"{currentId}\n{matchId}\n{chat.message}");
        }

        public void BsonSectionLog(int id, string section, List<messages> list)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set(section, list);

            BsonLog.UpdateOne(filter, update);
            //Error with "authorization" with logs
        }
    }
}