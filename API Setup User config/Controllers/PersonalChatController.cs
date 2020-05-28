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

namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalChatController : Controller
    {
        // controller is used for saving messages sent to other users, both new and old users
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
             //  Adds a new id to the list, for usage of first time message sent / recieved
                messages message = new messages(matchId, new List<Chats> { chat });
                chatListsMethod.Chat.Add(message);
            }
            //  Gets current data / chats of the person
            UserController userController = new UserController();
            userController.dbSetup("GateKeeper", "silvereye", "PersonalchatController", "", "", currentId);
            BsonLog = userController.BsonCollectionLog;
            BsonSectionLog(currentId, "Chat", chatListsMethod.Chat);
            return Ok($"{currentId}\n{matchId}\n{chat.message}");
        }

        public void BsonSectionLog(int id, string section, List<messages> list)
        {
            //Sends data to the db Log, by using "GateKeeper" profile
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set(section, list);
            BsonLog.UpdateOne(filter, update);
        }
    }
}