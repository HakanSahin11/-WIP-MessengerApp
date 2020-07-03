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
            var matchId = Convert.ToInt32(json.GetString("MatchID"));

            Chats chat = new Chats(json.GetString("message"), json.GetString("Timestamp"), Convert.ToBoolean(json.GetString("CurrentUser")));
            if (currentId != 0)
            {
                ChatSender(matchId, chat, currentId);
                return Ok($"{currentId}\n{matchId}\n{chat.message}");
            }
            else 
            {
                switch (chat.message.ToLower())
                {
                    case "!help":
                        chat.message = "List of commands: \n!Help: Shows list of available commands, \n!Info: Shows your personal information stored, \n!Rules: Shows the rules of the application, \n!Motd: Shows the last tweet sent by Potus";
                        break;

                    case "!info":
                        chat.message = Test(matchId);
                        break;

                    case "!rules":
                        chat.message = "Rules: \n1) Dont threaten other users\n2) Dont talk about self harming \n3) Do not activily trying to troll others to an extend seemed uncessesary \n4) No hatespeech / bullying will not be tolerated, epseicially regarding racism / gender / religious beliefs / etc\n5) Respect everyone's privary\n6) No Spam";
                        break;

                    //media of the day
                    case "!motd":
                        chat.message = "Show trumps last tweet";
                        break;

                    default:
                        chat.message = "Error! Please try again";
                        break;
                }
                ChatSender(currentId, chat, matchId);
            }
            return Ok();
        }

        public string Test(int currentId)
        {
            var result = new UserController();
            var result2 = result.dbSetup("GateKeeper", "silvereye", "getOne", null, null, currentId);
            var result3 = result.DatabaseGetOne;

            string overallResult = $"First Name:   {result3.FirstName}" +
                                 $"\nLast Name:   {result3.LastName}" +
                                 $"\nEmail:           {result3.Email}" +
                                 $"\nAge:             {result3.Age}" +
                                 $"\nGender:        {result3.Gender}" +
                                 $"\nJob Title:      {result3.JobTitle}" +
                                 $"\nCountry:       {result3.Country}" +
                                 $"\nCity:             {result3.City}" +
                                 $"\nAddress:      {result3.Address}";
            return overallResult;
        }


        public void ChatSender(int matchId, Chats chat, int currentId)
        {
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