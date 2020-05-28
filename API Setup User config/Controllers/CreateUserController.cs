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

namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateUserController : ControllerBase 
        //issue of the day: iherited controller above, routing name issues as always cuz inheritance
    {

        // POST: api/CreateUser
        [HttpPost]
        public ActionResult Post([FromBody] JsonElement json)
        {
            //UserClass user = new UserClass()

            var jsonStr = new WebClient().DownloadString(($"https://localhost:44371/api/User"));
            var id = (JsonConvert.DeserializeObject<List<List<UserClass>>>(jsonStr).Last().Last())._id + 1;
            //add to bson

            string send = $"{{" +
                $" \"_id\"           : \"{id}\",                          "  +
                $" \"Email\"         : \"{json.GetString("Email")}\",     "  +
                $" \"Password\"      : \"{json.GetString("Password")}\",  "  +
                $" \"FriendsList\"   : [ ],                               "  +
                $" \"IncFriendReq\"  : [ ],                               "  +
                $" \"SentFriendReq\" : [ ],                               "  +
                $" \"SentFriendReq\" : \"User\",                          "  +
                $"\"FirstName\"      : \"{json.GetString("FirstName")}\", "  +  
                $"\"LastName\"       : \"{json.GetString("LastName")}\",  "  +
                $"\"Gender\"         : \"{json.GetString("Gender")}\",    "  +
                $"\"Country\"        : \"{json.GetString("Country")}\",   "  +
                $"\"City\"           : \"{json.GetString("City")}\",      "  +
                $"\"Address\"        : \"{json.GetString("Address")}\",   "  +
                $"\"JobTitle\"       : \"{json.GetString("JobTitle")}\",  "  +
                $"\"Age\"            : \"{json.GetString("Age")}\",       "  +
                $"\"LoginBan\"       : \"{DateTime.Now}\"                 "  +
                $"}}";


            //chat



            return Ok();
        }

    }
}
