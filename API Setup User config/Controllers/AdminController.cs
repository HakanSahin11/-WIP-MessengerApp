using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API_Setup_User_config.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static API_Setup_User_config.Models.BsonSectioncs;
using static API_Setup_User_config.Controllers.UserController;

namespace API_Setup_User_config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // POST: api/Admin

        [HttpPost]
        public ActionResult Post([FromBody] JsonElement json)
        {
            //add to bsondocument and sent it the changes to the db, eventually use a former method
            //Removes part of the string which isnt used in the bson (id)
            int id = Convert.ToInt32(json.GetString("_id"));
            var resultStr = json.ToString().Replace($",\r\n  \"_id\": {id}", "");

            try
            {
                //Converts the elements in the json string to a dictionary to find the differences between the keys and values, so it can later be used in a foreach
                var dicOfChanges = JObject.Parse(resultStr).ToObject<Dictionary<string, string>>();
                foreach (var item in dicOfChanges)
                {
                    bsonObjects bson;
                    if (!item.Key.Contains("Age"))
                    {
                        //Makes a filter and update definition for the database changes of given user
                        bson = bsonSection(id, item.Key, item.Value, 0, new List<int>());
                    }
                    else
                    {
                        //used to make Age an integer in the mongodb
                        bson = bsonSection(id, item.Key, "", Convert.ToInt32(item.Value), new List<int>());

                    }
                    //saves changes to DB by using the definitions from Bson variable
                    BsonCollection("GateKeeper", "silvereye", "Users").UpdateOne(bson.Filter, bson.Update);
                }

                return Ok("Success");
            }
            catch
            {
                //failsafe
                return Ok("Failed");
            }
        }
    }
}
